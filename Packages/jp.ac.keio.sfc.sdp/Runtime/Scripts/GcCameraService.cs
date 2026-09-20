#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace GameCanvas
{
    public enum GcCameraState { Idle, RequestingPermission, Waiting, Running, NotGranted, NoDevice, TimedOut, Failed, Unsupported, Stopped }
    public enum GcCameraPermission { Unknown, NotGranted, Granted }
    /// <summary>Anyは背面を優先し、無ければ最初のカメラを使う。FrontとRearは指定した側だけを使う。</summary>
    public enum GcCameraFacing { Any, Front, Rear }

    internal readonly struct GcCameraFrame
    {
        internal readonly int Width, Height;
        internal readonly float Rotation;
        internal readonly bool Mirrored;
        internal GcCameraFrame(int width, int height, float rotation, bool mirrored)
        { Width = width; Height = height; Rotation = Mathf.Repeat(rotation, 360); Mirrored = mirrored; }
        internal bool SwapsAxes => GcMath.AlmostSame(Rotation, 90) || GcMath.AlmostSame(Rotation, 270);
    }
    internal interface IGcCameraBackend : IDisposable
    {
        bool Authorized { get; }
        // Tick advances this iterator once per frame. Backends yield null, not nested Unity yield instructions.
        IEnumerator RequestPermission();
        GcCameraDevice[] GetDevices();
        void Play(GcCameraDevice device, int width, int height, int fps);
        bool IsPlaying { get; }
        bool TryReadFrame(out GcCameraFrame frame);
        Texture? Texture { get; }
        void Focus(float2? point);
    }

    /// <summary>許可、映像待ち、中断を管理する。開始は操作時に呼び、中断後はもう一度Startする。</summary>
    public sealed class GcCameraService
    {
        readonly Func<IGcCameraBackend> createBackend;
        readonly Func<double> clock;
        IGcCameraBackend? backend;
        IEnumerator? permissionRequest;
        GcCameraFrame frame;
        double deadline;
        GcCameraFacing facing;
        string? requestedDevice;
        int requestedWidth, requestedHeight, requestedFps;
        bool permissionFinished;
        bool paused;
        public GcCameraState Status { get; private set; }
        /// <summary>最後の開始処理で確認した許可。停止後のOSの変更は追跡しない。</summary>
        public GcCameraPermission Permission { get; private set; }
        public string ErrorCode { get; private set; } = "";
        /// <summary>APIの対応プラットフォームかどうか。カメラの有無や許可を表す値ではない。</summary>
        public bool IsSupported { get; }
        /// <summary>開始時、許可を得てから調べた一覧。毎フレームの確保は発生しない。</summary>
        public IReadOnlyList<GcCameraDevice> Devices { get; private set; } = Array.Empty<GcCameraDevice>();
        public GcCameraDevice? Device { get; private set; }
        /// <summary>向きを補正した映像の幅。Running以外では0。</summary>
        public int Width => Status == GcCameraState.Running ? (frame.SwapsAxes ? frame.Height : frame.Width) : 0;
        /// <summary>向きを補正した映像の高さ。Running以外では0。</summary>
        public int Height => Status == GcCameraState.Running ? (frame.SwapsAxes ? frame.Width : frame.Height) : 0;
        /// <summary>この更新で新しい映像が届いたかどうか。</summary>
        public bool Updated { get; private set; }
        /// <summary>映像の向きを補正する時計回りの角度（度）。DrawCameraは自動で補正する。</summary>
        public float Rotation => frame.Rotation;
        /// <summary>元の映像が上下反転しているかどうか。DrawCameraは自動で補正する。</summary>
        public bool IsMirrored => frame.Mirrored;
        internal Texture? Texture => Status == GcCameraState.Running ? backend?.Texture : null;
        internal GcCameraFrame Frame => frame;

        internal GcCameraService(Func<IGcCameraBackend>? createBackend = null, Func<double>? clock = null, bool? supported = null)
        {
            this.createBackend = createBackend ?? (() => new Engine.GcInputCameraEngine());
            this.clock = clock ?? (() => Time.realtimeSinceStartupAsDouble);
            IsSupported = supported ?? (createBackend != null || HasPlatformBackend);
        }
        /// <summary>1台を開始する。幅・高さ・fpsは希望値。待ち時間は許可待ちを含む秒数。再呼び出しは前の処理を止めて開始し直す。</summary>
        public void Start(GcCameraFacing facing = GcCameraFacing.Any, int width = 640, int height = 480, int fps = 30, double timeoutSeconds = 30)
        {
            if (facing < GcCameraFacing.Any || facing > GcCameraFacing.Rear) throw new ArgumentOutOfRangeException(nameof(facing));
            StartCore(facing, null, width, height, fps, timeoutSeconds);
        }
        /// <summary>Devicesから選んだカメラを開始する。開始時に再確認し、外されていたらNoDeviceになる。</summary>
        public void Start(GcCameraDevice device, int width = 640, int height = 480, int fps = 30, double timeoutSeconds = 30)
        {
            if (device == null) throw new ArgumentNullException(nameof(device));
            StartCore(GcCameraFacing.Any, device.DeviceName, width, height, fps, timeoutSeconds);
        }
        void StartCore(GcCameraFacing facing, string? name, int width, int height, int fps, double timeoutSeconds)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            if (fps <= 0) throw new ArgumentOutOfRangeException(nameof(fps));
            if (double.IsNaN(timeoutSeconds) || double.IsInfinity(timeoutSeconds) || timeoutSeconds <= 0) throw new ArgumentOutOfRangeException(nameof(timeoutSeconds));
            Stop();
            Devices = Array.Empty<GcCameraDevice>();
            Permission = GcCameraPermission.Unknown;
            if (paused) return;
            if (!IsSupported) { Fail(GcCameraState.Unsupported, "GC-CAMERA-UNSUPPORTED"); return; }
            this.facing = facing; requestedDevice = name;
            requestedWidth = width; requestedHeight = height; requestedFps = fps;
            deadline = clock() + timeoutSeconds;
            Status = GcCameraState.RequestingPermission;
            try
            {
                backend = createBackend();
                permissionFinished = backend.Authorized;
                if (!permissionFinished) permissionRequest = backend.RequestPermission();
            }
            catch (Exception) { Fail(GcCameraState.Failed, "GC-CAMERA-NATIVE"); }
        }
        /// <summary>許可待ちも取り消し、映像とカメラを解放する。OSの許可画面そのものは閉じられない。</summary>
        public void Stop()
        {
            Close();
            Status = GcCameraState.Stopped;
            ErrorCode = "";
        }
        internal void SetPaused(bool value) { paused = value; if (value) Stop(); }
        internal void Tick()
        {
            Updated = false;
            if (backend == null) return;
            try
            {
                if (Status == GcCameraState.RequestingPermission)
                {
                    if (clock() >= deadline) { Fail(GcCameraState.TimedOut, "GC-CAMERA-TIMEOUT"); return; }
                    if (!permissionFinished)
                    {
                        if (permissionRequest!.MoveNext()) return;
                        DisposePermissionRequest();
                        permissionFinished = true;
                        // Android requires a frame between the permission callback and camera initialization.
                        return;
                    }
                    Permission = backend.Authorized ? GcCameraPermission.Granted : GcCameraPermission.NotGranted;
                    if (Permission != GcCameraPermission.Granted) { Fail(GcCameraState.NotGranted, "GC-CAMERA-PERMISSION"); return; }
                    Devices = Array.AsReadOnly(backend.GetDevices());
                    Device = SelectDevice();
                    if (Device == null) { Fail(GcCameraState.NoDevice, "GC-CAMERA-NO-DEVICE"); return; }
                    backend.Play(Device, requestedWidth, requestedHeight, requestedFps);
                    Status = GcCameraState.Waiting;
                    return;
                }
                if (!backend.Authorized)
                {
                    Permission = GcCameraPermission.NotGranted;
                    Fail(GcCameraState.NotGranted, "GC-CAMERA-PERMISSION"); return;
                }
                if (backend.TryReadFrame(out var next) && next.Width > 16 && next.Height > 16)
                {
                    frame = next; Updated = true; Status = GcCameraState.Running;
                }
                else if (Status == GcCameraState.Waiting && clock() >= deadline)
                    Fail(GcCameraState.TimedOut, "GC-CAMERA-TIMEOUT");
                else if (Status == GcCameraState.Running && !backend.IsPlaying)
                    Fail(GcCameraState.Failed, "GC-CAMERA-STOPPED");
            }
            catch (Exception) { Fail(GcCameraState.Failed, "GC-CAMERA-NATIVE"); }
        }
        GcCameraDevice? SelectDevice()
        {
            GcCameraDevice? fallback = null;
            for (var i = 0; i < Devices.Count; i++)
            {
                var candidate = Devices[i];
                if (candidate.IsDepth) continue;
                if (requestedDevice != null) { if (candidate.DeviceName == requestedDevice) return candidate; continue; }
                fallback ??= candidate;
                if (facing == GcCameraFacing.Front ? candidate.IsFront : !candidate.IsFront) return candidate;
            }
            return requestedDevice == null && facing == GcCameraFacing.Any ? fallback : null;
        }
        /// <summary>左下(0,0)、右上(1,1)の座標でピントを合わせる。起動前や非対応機種ではfalse。</summary>
        public bool Focus(float x, float y)
        {
            if (float.IsNaN(x) || x < 0 || x > 1) throw new ArgumentOutOfRangeException(nameof(x));
            if (float.IsNaN(y) || y < 0 || y > 1) throw new ArgumentOutOfRangeException(nameof(y));
            return FocusCore(new float2(x, y));
        }
        public bool ResetFocus() => FocusCore(null);
        bool FocusCore(float2? point)
        {
            if (Status != GcCameraState.Running || Device?.CanFocusPoint != true) return false;
            try { backend!.Focus(point); return true; }
            catch (Exception) { Fail(GcCameraState.Failed, "GC-CAMERA-NATIVE"); return false; }
        }
        void Fail(GcCameraState state, string code) { Close(); Status = state; ErrorCode = code; }
        void DisposePermissionRequest()
        {
            var old = permissionRequest; permissionRequest = null;
            (old as IDisposable)?.Dispose();
        }
        void Close()
        {
            var old = backend; backend = null;
            // Neither a cleanup failure nor an OS callback may keep an old stream alive in this service.
            try { DisposePermissionRequest(); } catch (Exception) { }
            try { old?.Dispose(); } catch (Exception) { }
            frame = default; Updated = false; Device = null;
        }
        static bool HasPlatformBackend =>
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE || UNITY_WEBGL
            true;
#else
            false;
#endif
    }
}
