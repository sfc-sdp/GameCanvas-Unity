using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityBridge.Helpers;
using UnityEditor;
using UnityEngine;

namespace UnityBridge.Tools
{
    [BridgeTool("screenshot")]
    public static class Screenshot
    {
        private static readonly Type GameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");

        // A1: Prevent concurrent burst executions
        private static int _burstRunning;

        public static Task<JObject> HandleCommand(JObject parameters)
        {
            var action = parameters["action"]?.Value<string>() ?? "capture";

            return action.ToLowerInvariant() switch
            {
                "capture" => Capture(parameters),
                "burst" => Burst(parameters),
                _ => throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    $"Unknown action: {action}. Valid actions: capture, burst")
            };
        }

        private static byte[] EncodeTexture(Texture2D texture, string format, int quality)
        {
            return format == "jpg"
                ? texture.EncodeToJPG(quality)
                : texture.EncodeToPNG();
        }

        private static Task<JObject> Capture(JObject parameters)
        {
            var source = parameters["source"]?.Value<string>() ?? "game";
            var path = parameters["path"]?.Value<string>();
            var superSize = parameters["superSize"]?.Value<int>() ?? 1;
            var format = (parameters["format"]?.Value<string>() ?? "png").ToLowerInvariant();
            var quality = parameters["quality"]?.Value<int>() ?? 75;

            CaptureHelpers.ValidateFormat(format);
            quality = Mathf.Clamp(quality, 1, 100);

            var ext = CaptureHelpers.GetExtension(format);
            path = CaptureHelpers.ResolveOutputPath(path, "screenshot", ext);

            return source.ToLowerInvariant() switch
            {
                "game" => CaptureGameViewAsync(path, superSize),
                "scene" => Task.FromResult(CaptureSceneView(path, format, quality)),
                "camera" => Task.FromResult(CaptureCamera(parameters, format, quality)),
                _ => throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    $"Unknown source: {source}. Valid sources: game, scene, camera")
            };
        }

        private static Task<JObject> Burst(JObject parameters)
        {
            // A1: Prevent concurrent burst execution
            if (Interlocked.CompareExchange(ref _burstRunning, 1, 0) != 0)
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    "Burst capture already in progress.");
            }

            try
            {
                var count = parameters["count"]?.Value<int>() ?? 10;
                var intervalMs = parameters["interval_ms"]?.Value<int>() ?? 0;
                var format = (parameters["format"]?.Value<string>() ?? "jpg").ToLowerInvariant();
                var quality = parameters["quality"]?.Value<int>() ?? 75;
                var width = parameters["width"]?.Value<int>() ?? 1920;
                var height = parameters["height"]?.Value<int>() ?? 1080;
                var cameraName = parameters["camera"]?.Value<string>();
                var outputDir = parameters["outputDir"]?.Value<string>();

                CaptureHelpers.ValidateFormat(format);
                quality = Mathf.Clamp(quality, 1, 100);
                count = Mathf.Clamp(count, 1, 1000);
                CaptureHelpers.ClampDimensions(ref width, ref height);

                // S4: Guard against negative interval
                intervalMs = Mathf.Max(0, intervalMs);

                outputDir = CaptureHelpers.ResolveOutputDir(outputDir, "burst");
                var camera = CaptureHelpers.FindCamera(cameraName);

                var tcs = new TaskCompletionSource<JObject>(TaskCreationOptions.RunContinuationsAsynchronously);
                var framesCaptured = 0;
                var pendingWrites = 0;
                var droppedFrames = 0;
                var startTime = DateTime.UtcNow;
                var lastCaptureTime = DateTime.UtcNow;
                var paths = new List<string>();
                const int timeoutSeconds = 60;

                var ext = CaptureHelpers.GetExtension(format);
                var prevTarget = camera.targetTexture;
                var prevActive = RenderTexture.active;
                var renderTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);

                void OnUpdate()
                {
                    // Force editor to keep ticking at high rate
                    EditorApplication.QueuePlayerLoopUpdate();

                    try
                    {
                        if ((DateTime.UtcNow - startTime).TotalSeconds > timeoutSeconds)
                        {
                            Cleanup();
                            tcs.TrySetException(new ProtocolException(
                                ErrorCode.InternalError,
                                $"Burst capture timed out after {timeoutSeconds}s"));
                            return;
                        }

                        if (framesCaptured >= count)
                        {
                            // Wait for pending writes to finish
                            if (Interlocked.CompareExchange(ref pendingWrites, 0, 0) == 0)
                            {
                                Cleanup();
                                var elapsed = (DateTime.UtcNow - startTime).TotalSeconds;
                                var fps = elapsed > 0 ? framesCaptured / elapsed : 0;
                                tcs.TrySetResult(new JObject
                                {
                                    ["message"] = $"Burst capture complete: {framesCaptured} frames",
                                    ["frameCount"] = framesCaptured,
                                    ["droppedFrames"] = droppedFrames,
                                    ["outputDir"] = outputDir,
                                    ["format"] = format,
                                    ["elapsed"] = Math.Round(elapsed, 3),
                                    ["fps"] = Math.Round(fps, 1),
                                    ["paths"] = new JArray(paths)
                                });
                            }
                            return;
                        }

                        // Interval control
                        if (intervalMs > 0)
                        {
                            var elapsedSinceLastCapture = (DateTime.UtcNow - lastCaptureTime).TotalMilliseconds;
                            if (elapsedSinceLastCapture < intervalMs)
                                return;
                        }

                        // P2: Back-pressure - skip frame if too many writes pending
                        if (Interlocked.CompareExchange(ref pendingWrites, 0, 0) >= ToolConstants.BurstMaxPendingWrites)
                        {
                            droppedFrames++;
                            return;
                        }

                        // Capture frame
                        camera.targetTexture = renderTexture;
                        camera.Render();

                        RenderTexture.active = renderTexture;
                        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                        try
                        {
                            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                            texture.Apply();

                            var bytes = EncodeTexture(texture, format, quality);

                            var framePath = Path.Combine(outputDir, $"frame_{framesCaptured:D4}{ext}");
                            paths.Add(framePath);
                            framesCaptured++;
                            lastCaptureTime = DateTime.UtcNow;

                            // Async file write
                            Interlocked.Increment(ref pendingWrites);
                            ThreadPool.QueueUserWorkItem(_ =>
                            {
                                try
                                {
                                    File.WriteAllBytes(framePath, bytes);
                                }
                                catch (Exception ex)
                                {
                                    BridgeLog.Warn($"[Screenshot] Failed to write burst frame: {ex.Message}");
                                }
                                finally
                                {
                                    Interlocked.Decrement(ref pendingWrites);
                                }
                            });
                        }
                        finally
                        {
                            UnityEngine.Object.DestroyImmediate(texture);
                        }
                    }
                    catch (Exception ex)
                    {
                        Cleanup();
                        tcs.TrySetException(ex is ProtocolException ? ex :
                            new ProtocolException(ErrorCode.InternalError,
                                $"Burst capture failed: {ex.Message}"));
                    }
                }

                void Cleanup()
                {
                    EditorApplication.update -= OnUpdate;
                    camera.targetTexture = prevTarget;
                    RenderTexture.active = prevActive;
                    RenderTexture.ReleaseTemporary(renderTexture);
                    Interlocked.Exchange(ref _burstRunning, 0);
                }

                EditorApplication.update += OnUpdate;
                EditorApplication.QueuePlayerLoopUpdate();

                return tcs.Task;
            }
            catch
            {
                // Release burst lock if setup fails
                Interlocked.Exchange(ref _burstRunning, 0);
                throw;
            }
        }

        private static Task<JObject> CaptureGameViewAsync(string path, int superSize)
        {
            if (!EditorApplication.isPlaying)
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    "GameView screenshot requires Play Mode. Enter play mode first with action 'enter'.");
            }

            superSize = Mathf.Clamp(superSize, 1, 4);

            if (GameViewType == null)
            {
                throw new ProtocolException(
                    ErrorCode.InternalError,
                    "GameView type not found via reflection");
            }

            var gameView = EditorWindow.GetWindow(GameViewType, false, null, true);
            if (gameView == null)
            {
                throw new ProtocolException(
                    ErrorCode.InternalError,
                    "Failed to get GameView window");
            }

            gameView.Focus();
            gameView.Repaint();

            var tcs = new TaskCompletionSource<JObject>(TaskCreationOptions.RunContinuationsAsynchronously);
            var framesToWait = 2;
            var captured = false;
            var startTime = DateTime.UtcNow;
            const int timeoutSeconds = 10;

            void OnUpdate()
            {
                try
                {
                    if ((DateTime.UtcNow - startTime).TotalSeconds > timeoutSeconds)
                    {
                        EditorApplication.update -= OnUpdate;
                        tcs.TrySetException(new ProtocolException(
                            ErrorCode.InternalError,
                            "GameView screenshot capture timed out"));
                        return;
                    }

                    if (framesToWait > 0)
                    {
                        framesToWait--;
                        return;
                    }

                    if (!captured)
                    {
                        ScreenCapture.CaptureScreenshot(path, superSize);
                        captured = true;
                        return;
                    }

                    if (File.Exists(path) && new FileInfo(path).Length > 0)
                    {
                        EditorApplication.update -= OnUpdate;
                        tcs.TrySetResult(new JObject
                        {
                            ["message"] = "GameView screenshot captured",
                            ["path"] = path,
                            ["source"] = "game",
                            ["superSize"] = superSize
                        });
                    }
                }
                catch (Exception ex)
                {
                    EditorApplication.update -= OnUpdate;
                    tcs.TrySetException(ex is ProtocolException ? ex :
                        new ProtocolException(ErrorCode.InternalError,
                            $"GameView screenshot failed: {ex.Message}"));
                }
            }

            EditorApplication.update += OnUpdate;
            EditorApplication.QueuePlayerLoopUpdate();

            return tcs.Task;
        }

        private static JObject CaptureCamera(JObject parameters, string format, int quality)
        {
            var path = parameters["path"]?.Value<string>();
            var width = parameters["width"]?.Value<int>() ?? 1920;
            var height = parameters["height"]?.Value<int>() ?? 1080;
            var cameraName = parameters["camera"]?.Value<string>();

            CaptureHelpers.ClampDimensions(ref width, ref height);

            var ext = CaptureHelpers.GetExtension(format);
            path = CaptureHelpers.ResolveOutputPath(path, "camera", ext);

            var camera = CaptureHelpers.FindCamera(cameraName);

            var prevTarget = camera.targetTexture;
            var prevActive = RenderTexture.active;
            var renderTexture = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);

            try
            {
                camera.targetTexture = renderTexture;
                camera.Render();

                RenderTexture.active = renderTexture;
                var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                try
                {
                    texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                    texture.Apply();

                    var bytes = EncodeTexture(texture, format, quality);
                    File.WriteAllBytes(path, bytes);
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(texture);
                }

                return new JObject
                {
                    ["message"] = "Camera screenshot captured",
                    ["path"] = path,
                    ["source"] = "camera",
                    ["format"] = format,
                    ["width"] = width,
                    ["height"] = height,
                    ["camera"] = camera.name
                };
            }
            finally
            {
                camera.targetTexture = prevTarget;
                RenderTexture.active = prevActive;
                RenderTexture.ReleaseTemporary(renderTexture);
            }
        }

        private static JObject CaptureSceneView(string path, string format, int quality)
        {
            var sceneView = SceneView.lastActiveSceneView;

            if (sceneView == null)
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    "No active SceneView found");
            }

            var camera = sceneView.camera;
            var width = (int)sceneView.position.width;
            var height = (int)sceneView.position.height;

            var renderTexture = new RenderTexture(width, height, 24);
            var previousTarget = camera.targetTexture;
            // P5: Save current RenderTexture.active to restore in finally
            var previousActive = RenderTexture.active;

            try
            {
                camera.targetTexture = renderTexture;
                camera.Render();

                RenderTexture.active = renderTexture;
                var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
                try
                {
                    texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                    texture.Apply();

                    var bytes = EncodeTexture(texture, format, quality);
                    File.WriteAllBytes(path, bytes);
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(texture);
                }

                return new JObject
                {
                    ["message"] = "SceneView screenshot captured",
                    ["path"] = path,
                    ["source"] = "scene",
                    ["format"] = format,
                    ["width"] = width,
                    ["height"] = height
                };
            }
            finally
            {
                camera.targetTexture = previousTarget;
                // P5: Restore actual previous RenderTexture.active instead of null
                RenderTexture.active = previousActive;
                UnityEngine.Object.DestroyImmediate(renderTexture);
            }
        }
    }
}
