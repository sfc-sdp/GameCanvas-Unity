#nullable enable
using System;
using System.Collections;
using UnityEngine;

namespace GameCanvas
{
    /// <summary>位置の利用許可。OSが区別できない場合はUnknown。</summary>
    public enum GcLocationPermission { Unknown, NotGranted, Approximate, Precise }
    public enum GcLocationState { Idle, RequestingPermission, Waiting, Running, NotGranted, Disabled, TimedOut, Failed, Unsupported, Stopped }
    /// <summary>緯度経度は度、精度はメートル、時刻はUTCのUNIX秒。模擬位置はIsMockで区別する。</summary>
    public readonly struct GcLocationSample
    {
        public double Latitude { get; }
        public double Longitude { get; }
        public double AccuracyMeters { get; }
        public double UnixTimeSeconds { get; }
        public bool IsMock { get; }
        public GcLocationSample(double latitude, double longitude, double accuracyMeters, double unixTimeSeconds, bool isMock)
        { Latitude = latitude; Longitude = longitude; AccuracyMeters = accuracyMeters; UnixTimeSeconds = unixTimeSeconds; IsMock = isMock; }
    }

    internal interface IGcLocationBackend : IDisposable
    {
        bool Enabled { get; }
        bool NeedsPermissionRequest { get; }
        bool IsRestricted { get; }
        GcLocationPermission ReadPermission();
        IEnumerator RequestPermission(Action<bool> timedOut);
        bool Start(bool precise);
        bool TryRead(out GcLocationSample sample);
    }

    /// <summary>位置情報の新API。実装はAndroidとiOS。毎フレームStatusを読む。</summary>
    public sealed class GcLocationService
    {
        readonly MonoBehaviour owner;
        readonly Func<IGcLocationBackend>? createBackend;
        Coroutine? routine;
        GcLocationSample sample;
        bool hasSample;
        IGcLocationBackend? native;
        internal GcLocationService(MonoBehaviour owner, Func<IGcLocationBackend>? createBackend = null)
        {
            this.owner = owner;
            this.createBackend = createBackend;
        }
        public GcLocationState Status { get; private set; }
        public GcLocationPermission Permission { get; private set; }
        public string ErrorCode { get; private set; } = "";
        public long OperationId { get; private set; }
        public bool IsSupported => createBackend != null || HasDeviceBackend;
        public bool TryGetSample(out GcLocationSample value) { value = sample; return hasSample && Status == GcLocationState.Running; }

        /// <summary>利用操作から呼ぶ。概算許可済みなら再要求しない。中断後は明示的にStartする。</summary>
        public long Start(double timeoutSeconds = 30)
        {
            if (double.IsNaN(timeoutSeconds) || double.IsInfinity(timeoutSeconds) || timeoutSeconds <= 0)
                throw new ArgumentOutOfRangeException(nameof(timeoutSeconds));
            Stop();
            OperationId++;
            ErrorCode = "";
            if (!IsSupported) { Status = GcLocationState.Unsupported; ErrorCode = "GC-LOCATION-UNSUPPORTED"; return OperationId; }
            Status = GcLocationState.RequestingPermission;
            routine = owner.StartCoroutine(Run(timeoutSeconds));
            return OperationId;
        }
        public void Stop()
        {
            if (routine != null) owner.StopCoroutine(routine);
            routine = null;
            CloseNative(); hasSample = false;
            Status = GcLocationState.Stopped;
        }
        internal void Tick()
        {
            if (native == null || (Status != GcLocationState.Waiting && Status != GcLocationState.Running)) return;
            try
            {
                Permission = native.ReadPermission();
                if (!native.Enabled) { Fail(GcLocationState.Disabled, "GC-LOCATION-DISABLED"); return; }
                if (native.IsRestricted) { Fail(GcLocationState.NotGranted, "GC-LOCATION-RESTRICTED"); return; }
                if (Permission != GcLocationPermission.Precise && Permission != GcLocationPermission.Approximate)
                { Fail(GcLocationState.NotGranted, "GC-LOCATION-PERMISSION"); return; }
                if (native.TryRead(out sample)) { hasSample = true; Status = GcLocationState.Running; }
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            catch (AndroidJavaException) { Fail(GcLocationState.Failed, "GC-LOCATION-NATIVE"); }
#else
            catch (Exception) { Fail(GcLocationState.Failed, "GC-LOCATION-NATIVE"); }
#endif
        }
        IEnumerator Run(double timeoutSeconds)
        {
            IGcLocationBackend? impl = null;
            var nativeFailed = false;
            try { impl = createBackend?.Invoke() ?? CreateDeviceBackend(); }
#if UNITY_ANDROID && !UNITY_EDITOR
            catch (AndroidJavaException) { nativeFailed = true; }
#else
            catch (Exception) { nativeFailed = true; }
#endif
            if (nativeFailed) { Fail(GcLocationState.Failed, "GC-LOCATION-NATIVE"); yield break; }
            if (impl == null)
            {
                Status = GcLocationState.Unsupported;
                ErrorCode = "GC-LOCATION-UNSUPPORTED";
                yield break;
            }
            native = impl;
            Permission = impl.ReadPermission();
            if (impl.NeedsPermissionRequest)
            {
                var expired = false;
                yield return impl.RequestPermission(value => expired = value);
                if (Status != GcLocationState.RequestingPermission) yield break;
                Permission = impl.ReadPermission();
                if (expired) { Fail(GcLocationState.TimedOut, "GC-PERMISSION-TIMEOUT"); yield break; }
            }
            var waiting = false;
            try
            {
                if (!impl.Enabled) Fail(GcLocationState.Disabled, "GC-LOCATION-DISABLED");
                else if (impl.IsRestricted) Fail(GcLocationState.NotGranted, "GC-LOCATION-RESTRICTED");
                else if (Permission != GcLocationPermission.Precise && Permission != GcLocationPermission.Approximate)
                    Fail(GcLocationState.NotGranted, "GC-LOCATION-PERMISSION");
                else if (!impl.Start(Permission == GcLocationPermission.Precise)) Fail(GcLocationState.Failed, "GC-LOCATION-NO-PROVIDER");
                else { Status = GcLocationState.Waiting; waiting = true; }
            }
#if UNITY_ANDROID && !UNITY_EDITOR
            catch (AndroidJavaException) { nativeFailed = true; }
#else
            catch (Exception) { nativeFailed = true; }
#endif
            if (nativeFailed) { Fail(GcLocationState.Failed, "GC-LOCATION-NATIVE"); yield break; }
            if (!waiting) yield break;
            var deadline = Time.realtimeSinceStartupAsDouble + timeoutSeconds;
            while (Status == GcLocationState.Waiting && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            if (Status == GcLocationState.Waiting) Fail(GcLocationState.TimedOut, "GC-LOCATION-TIMEOUT");
        }
        static IGcLocationBackend? CreateDeviceBackend()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return new Engine.GcAndroidLocation();
#elif UNITY_IOS && !UNITY_EDITOR
            return new Engine.GcIosLocation();
#else
            return null;
#endif
        }
        void Fail(GcLocationState state, string code) { CloseNative(); hasSample = false; Status = state; ErrorCode = code; }
        void CloseNative()
        {
            var previous = native; native = null;
            if (previous == null) return;
            try { previous.Dispose(); }
#if UNITY_ANDROID && !UNITY_EDITOR
            catch (AndroidJavaException) { ErrorCode = "GC-LOCATION-STOP"; }
#else
            catch (Exception) { ErrorCode = "GC-LOCATION-STOP"; }
#endif
        }
        static bool HasDeviceBackend =>
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            true;
#else
            false;
#endif
    }
}
