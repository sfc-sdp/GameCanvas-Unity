using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Unity.Collections;
using UnityBridge.Helpers;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace UnityBridge.Tools
{
    [BridgeTool("recorder")]
    [InitializeOnLoad]
    public static class Recorder
    {
        private static RecordingSession _session;

        static Recorder()
        {
            AssemblyReloadEvents.beforeAssemblyReload += ForceStop;
            EditorApplication.quitting += ForceStop;
        }

        public static Task<JObject> HandleCommand(JObject parameters)
        {
            var action = parameters["action"]?.Value<string>() ?? "status";

            return action.ToLowerInvariant() switch
            {
                "start" => Task.FromResult(Start(parameters)),
                "stop" => Task.FromResult(Stop()),
                "status" => Task.FromResult(GetStatus()),
                _ => throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    $"Unknown action: {action}. Valid actions: start, stop, status")
            };
        }

        private static JObject Start(JObject parameters)
        {
            if (_session != null && _session.IsRecording)
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    "Recording already in progress. Use 'stop' to end current recording.");
            }

            var fps = parameters["fps"]?.Value<int>() ?? 30;
            var format = (parameters["format"]?.Value<string>() ?? "jpg").ToLowerInvariant();
            var quality = parameters["quality"]?.Value<int>() ?? 75;
            var width = parameters["width"]?.Value<int>() ?? 1920;
            var height = parameters["height"]?.Value<int>() ?? 1080;
            var cameraName = parameters["camera"]?.Value<string>();
            var outputDir = parameters["outputDir"]?.Value<string>();

            CaptureHelpers.ValidateFormat(format);

            fps = Mathf.Clamp(fps, 1, 120);
            quality = Mathf.Clamp(quality, 1, 100);
            CaptureHelpers.ClampDimensions(ref width, ref height);

            outputDir = CaptureHelpers.ResolveOutputDir(outputDir, "recording");
            var camera = CaptureHelpers.FindCamera(cameraName);

            // M3: Build session in local var, assign to _session only on success
            var session = new RecordingSession(camera, fps, format, quality, width, height, outputDir);
            session.Start();
            _session = session;

            return new JObject
            {
                ["message"] = $"Recording started at {fps} FPS",
                ["fps"] = fps,
                ["format"] = format,
                ["quality"] = quality,
                ["width"] = width,
                ["height"] = height,
                ["outputDir"] = outputDir,
                ["camera"] = camera.name
            };
        }

        private static JObject Stop()
        {
            if (_session == null || !_session.IsRecording)
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    "No recording in progress.");
            }

            var result = _session.Stop();
            _session = null;
            return result;
        }

        private static JObject GetStatus()
        {
            if (_session == null || !_session.IsRecording)
            {
                return new JObject
                {
                    ["recording"] = false,
                    ["message"] = "No recording in progress"
                };
            }

            return _session.GetStatus();
        }

        private static void ForceStop()
        {
            if (_session == null || !_session.IsRecording) return;

            try
            {
                _session.Stop();
            }
            catch (Exception ex)
            {
                BridgeLog.Warn($"[Recorder] ForceStop error: {ex.Message}");
            }
            finally
            {
                _session = null;
            }
        }

        // Cached reflection: InternalEditorUtility.RepaintAllViews()
        private static readonly Action RepaintAllViews = CreateRepaintAllViews();

        private static Action CreateRepaintAllViews()
        {
            var type = Type.GetType("UnityEditorInternal.InternalEditorUtility,UnityEditor");
            var method = type?.GetMethod("RepaintAllViews", BindingFlags.Static | BindingFlags.Public);
            if (method == null) return null;
            return (Action)Delegate.CreateDelegate(typeof(Action), method);
        }

        private class RecordingSession
        {
            private const int RingSize = 3;

            private readonly Camera _camera;
            private readonly int _targetFps;
            private readonly string _format;
            private readonly int _quality;
            private readonly int _width;
            private readonly int _height;
            private readonly string _outputDir;
            private readonly string _ext;
            private readonly double _frameInterval;

            // Triple-buffered RenderTextures
            private RenderTexture[] _rtRing;
            // Per-slot busy flags (0=free, 1=busy)
            private int[] _slotBusy;
            // Pre-allocated readback buffers
            private NativeArray<byte>[] _readbackBuffers;

            // Play Mode FPS control: saved values to restore on Stop
            private int _savedTargetFrameRate;
            private int _savedVSyncCount;
            private bool _playModeOverrideActive;

            private int _frameCount;
            private int _droppedFrames;
            private int _pendingWrites;
            private int _pendingReadbacks;
            private int _failedWrites;
            private double _startTime;
            private double _lastCaptureTime;

            public bool IsRecording { get; private set; }

            public RecordingSession(Camera camera, int fps, string format, int quality, int width, int height, string outputDir)
            {
                _camera = camera;
                _targetFps = fps;
                _format = format;
                _quality = quality;
                _width = width;
                _height = height;
                _outputDir = outputDir;
                _ext = CaptureHelpers.GetExtension(format);
                _frameInterval = 1.0 / fps;
            }

            public void Start()
            {
                _frameCount = 0;
                _droppedFrames = 0;
                _pendingWrites = 0;
                _pendingReadbacks = 0;
                _failedWrites = 0;
                _startTime = EditorApplication.timeSinceStartup;
                _lastCaptureTime = 0;
                IsRecording = true;

                // Allocate triple-buffered RenderTextures
                _rtRing = new RenderTexture[RingSize];
                _slotBusy = new int[RingSize];
                _readbackBuffers = new NativeArray<byte>[RingSize];
                var bufferSize = _width * _height * 4; // RGBA32 = 4 bytes/pixel

                for (int i = 0; i < RingSize; i++)
                {
                    _rtRing[i] = RenderTexture.GetTemporary(_width, _height, 24, RenderTextureFormat.ARGB32);
                    _slotBusy[i] = 0;
                    _readbackBuffers[i] = new NativeArray<byte>(
                        bufferSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                }

                // Play Mode: override Application.targetFrameRate and VSync for high FPS
                if (EditorApplication.isPlaying)
                {
                    _savedTargetFrameRate = Application.targetFrameRate;
                    _savedVSyncCount = QualitySettings.vSyncCount;
                    QualitySettings.vSyncCount = 0;
                    Application.targetFrameRate = _targetFps;
                    _playModeOverrideActive = true;
                }

                EditorApplication.update += OnUpdate;
                EditorApplication.QueuePlayerLoopUpdate();

                BridgeLog.Info($"[Recorder] Started recording at {_targetFps} FPS ({_width}x{_height}, {_format}, ring={RingSize}, playMode={_playModeOverrideActive})");
            }

            public JObject Stop()
            {
                IsRecording = false;
                EditorApplication.update -= OnUpdate;

                // Restore Play Mode FPS settings
                if (_playModeOverrideActive)
                {
                    QualitySettings.vSyncCount = _savedVSyncCount;
                    Application.targetFrameRate = _savedTargetFrameRate;
                    _playModeOverrideActive = false;
                }

                // P1: Wait for both pending readbacks and writes to complete
                var waitStart = DateTime.UtcNow;
                var timeout = ToolConstants.RecorderStopTimeoutSeconds;
                while ((Interlocked.CompareExchange(ref _pendingReadbacks, 0, 0) > 0
                        || Interlocked.CompareExchange(ref _pendingWrites, 0, 0) > 0)
                       && (DateTime.UtcNow - waitStart).TotalSeconds < timeout)
                {
                    Thread.Sleep(10);
                }

                // Release ring resources
                for (int i = 0; i < RingSize; i++)
                {
                    if (_rtRing[i] != null)
                    {
                        RenderTexture.ReleaseTemporary(_rtRing[i]);
                        _rtRing[i] = null;
                    }
                    if (_readbackBuffers[i].IsCreated)
                    {
                        try { _readbackBuffers[i].Dispose(); }
                        catch (InvalidOperationException) { /* may be locked by pending readback */ }
                    }
                }

                var elapsed = EditorApplication.timeSinceStartup - _startTime;
                var fps = elapsed > 0 ? _frameCount / elapsed : 0;

                BridgeLog.Info($"[Recorder] Stopped. {_frameCount} frames in {elapsed:F2}s ({fps:F1} FPS, dropped: {_droppedFrames}, failed: {_failedWrites})");

                return new JObject
                {
                    ["message"] = $"Recording stopped: {_frameCount} frames",
                    ["frameCount"] = _frameCount,
                    ["droppedFrames"] = _droppedFrames,
                    ["failedWrites"] = _failedWrites,
                    ["elapsed"] = Math.Round(elapsed, 3),
                    ["fps"] = Math.Round(fps, 1),
                    ["outputDir"] = _outputDir,
                    ["format"] = _format
                };
            }

            public JObject GetStatus()
            {
                var elapsed = EditorApplication.timeSinceStartup - _startTime;
                var fps = elapsed > 0 ? _frameCount / elapsed : 0;

                return new JObject
                {
                    ["recording"] = true,
                    ["frameCount"] = _frameCount,
                    ["droppedFrames"] = _droppedFrames,
                    ["failedWrites"] = _failedWrites,
                    ["elapsed"] = Math.Round(elapsed, 3),
                    ["fps"] = Math.Round(fps, 1),
                    ["pendingWrites"] = Interlocked.CompareExchange(ref _pendingWrites, 0, 0),
                    ["pendingReadbacks"] = Interlocked.CompareExchange(ref _pendingReadbacks, 0, 0),
                    ["outputDir"] = _outputDir
                };
            }

            private void OnUpdate()
            {
                if (!IsRecording) return;

                try
                {
                    double now = EditorApplication.timeSinceStartup;
                    double elapsed = now - _startTime;

                    // FPS throttle
                    if (elapsed - _lastCaptureTime < _frameInterval)
                        return;

                    // P4: Only force repaint on capture tick, skip in Play Mode
                    if (!EditorApplication.isPlaying)
                    {
                        EditorApplication.QueuePlayerLoopUpdate();
                        RepaintAllViews?.Invoke();
                    }

                    _lastCaptureTime = elapsed;

                    CaptureFrame();
                }
                catch (Exception ex)
                {
                    BridgeLog.Error($"[Recorder] Frame capture error: {ex.Message}");
                }
            }

            private void CaptureFrame()
            {
                int slot = _frameCount % RingSize;

                // Back-pressure: skip if slot is still busy (GPU readback or encoding in progress)
                if (Interlocked.CompareExchange(ref _slotBusy[slot], 1, 0) != 0)
                {
                    _droppedFrames++;
                    return;
                }

                // Render to slot's RenderTexture
                var prevTarget = _camera.targetTexture;
                var prevActive = RenderTexture.active;

                _camera.targetTexture = _rtRing[slot];
                _camera.Render();

                _camera.targetTexture = prevTarget;
                RenderTexture.active = prevActive;

                int frameIndex = _frameCount;
                _frameCount++;

                if (SystemInfo.supportsAsyncGPUReadback)
                {
                    // P1: Track pending readbacks
                    Interlocked.Increment(ref _pendingReadbacks);

                    var req = AsyncGPUReadback.RequestIntoNativeArray(
                        ref _readbackBuffers[slot],
                        _rtRing[slot], 0, TextureFormat.RGBA32,
                        request => OnReadbackComplete(slot, frameIndex, request));
                    req.forcePlayerLoopUpdate = true;
                }
                else
                {
                    // M1: Synchronous fallback wrapped in try/finally for _slotBusy safety
                    try
                    {
                        RenderTexture.active = _rtRing[slot];
                        var texture = new Texture2D(_width, _height, TextureFormat.RGBA32, false);
                        try
                        {
                            texture.ReadPixels(new Rect(0, 0, _width, _height), 0, 0);
                            texture.Apply();

                            var bytes = _format == "jpg"
                                ? texture.EncodeToJPG(_quality)
                                : texture.EncodeToPNG();

                            WriteFrameAsync(slot, frameIndex, bytes);
                            // WriteFrameAsync takes ownership of slot release
                            return;
                        }
                        finally
                        {
                            UnityEngine.Object.DestroyImmediate(texture);
                        }
                    }
                    catch
                    {
                        // Ensure slot is released on any failure before WriteFrameAsync
                        Interlocked.Exchange(ref _slotBusy[slot], 0);
                        throw;
                    }
                    finally
                    {
                        RenderTexture.active = prevActive;
                    }
                }
            }

            private void OnReadbackComplete(int slot, int frameIndex, AsyncGPUReadbackRequest request)
            {
                // P1: Readback complete
                Interlocked.Decrement(ref _pendingReadbacks);

                if (request.hasError)
                {
                    BridgeLog.Warn($"[Recorder] AsyncGPUReadback failed for frame {frameIndex}");
                    Interlocked.Exchange(ref _slotBusy[slot], 0);
                    return;
                }

                Interlocked.Increment(ref _pendingWrites);
                var framePath = Path.Combine(_outputDir, $"frame_{frameIndex:D6}{_ext}");
                int w = _width;
                int h = _height;
                int q = _quality;
                string fmt = _format;

                ThreadPool.UnsafeQueueUserWorkItem(_ =>
                {
                    try
                    {
                        NativeArray<byte> encoded;
                        if (fmt == "jpg")
                        {
                            encoded = ImageConversion.EncodeNativeArrayToJPG(
                                _readbackBuffers[slot],
                                GraphicsFormat.R8G8B8A8_UNorm,
                                (uint)w, (uint)h, 0, q);
                        }
                        else
                        {
                            encoded = ImageConversion.EncodeNativeArrayToPNG(
                                _readbackBuffers[slot],
                                GraphicsFormat.R8G8B8A8_UNorm,
                                (uint)w, (uint)h);
                        }

                        var byteArray = encoded.ToArray();
                        if (encoded.IsCreated)
                        {
                            try { encoded.Dispose(); }
                            catch (InvalidOperationException) { /* Unity internal allocator */ }
                        }
                        File.WriteAllBytes(framePath, byteArray);
                    }
                    catch (Exception ex)
                    {
                        // M2: Track failed writes
                        Interlocked.Increment(ref _failedWrites);
                        BridgeLog.Warn($"[Recorder] Encode/write failed for frame {frameIndex}: {ex.Message}");
                    }
                    finally
                    {
                        Interlocked.Exchange(ref _slotBusy[slot], 0);
                        Interlocked.Decrement(ref _pendingWrites);
                    }
                }, null);
            }

            private void WriteFrameAsync(int slot, int frameIndex, byte[] bytes)
            {
                Interlocked.Increment(ref _pendingWrites);
                var framePath = Path.Combine(_outputDir, $"frame_{frameIndex:D6}{_ext}");

                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        File.WriteAllBytes(framePath, bytes);
                    }
                    catch (Exception ex)
                    {
                        // M2: Track failed writes
                        Interlocked.Increment(ref _failedWrites);
                        BridgeLog.Warn($"[Recorder] Failed to write frame {frameIndex}: {ex.Message}");
                    }
                    finally
                    {
                        Interlocked.Exchange(ref _slotBusy[slot], 0);
                        Interlocked.Decrement(ref _pendingWrites);
                    }
                });
            }
        }
    }
}
