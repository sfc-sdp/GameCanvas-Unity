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

    /// <summary>位置情報の新API試作。現時点の実装はAndroid。毎フレームStatusを読む。</summary>
    public sealed class GcLocationService
    {
        readonly MonoBehaviour owner;
        Coroutine? routine;
        GcLocationSample sample;
        bool hasSample;
#if UNITY_ANDROID && !UNITY_EDITOR
        Engine.GcAndroidLocation? native;
#endif
        internal GcLocationService(MonoBehaviour owner) => this.owner = owner;
        public GcLocationState Status { get; private set; }
        public GcLocationPermission Permission { get; private set; }
        public string ErrorCode { get; private set; } = "";
        public long OperationId { get; private set; }
        public bool IsSupported => Application.platform == RuntimePlatform.Android;
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
#if UNITY_ANDROID && !UNITY_EDITOR
            if (native == null || (Status != GcLocationState.Waiting && Status != GcLocationState.Running)) return;
            try
            {
                Permission = ReadPermission();
                if (Permission == GcLocationPermission.NotGranted) { Fail(GcLocationState.NotGranted, "GC-LOCATION-PERMISSION"); return; }
                if (!native.Enabled) { Fail(GcLocationState.Disabled, "GC-LOCATION-DISABLED"); return; }
                if (native.TryRead(out sample)) { hasSample = true; Status = GcLocationState.Running; }
            }
            catch (AndroidJavaException) { Fail(GcLocationState.Failed, "GC-LOCATION-NATIVE"); }
#endif
        }
        IEnumerator Run(double timeoutSeconds)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Permission = ReadPermission();
            if (Permission == GcLocationPermission.NotGranted)
            {
                var expired = false;
                yield return Engine.GcAndroidPermission.Request(new[] { UnityEngine.Android.Permission.CoarseLocation, UnityEngine.Android.Permission.FineLocation }, value => expired = value);
                Permission = ReadPermission();
                if (expired) { Fail(GcLocationState.TimedOut, "GC-PERMISSION-TIMEOUT"); yield break; }
            }
            if (Permission == GcLocationPermission.NotGranted) { Fail(GcLocationState.NotGranted, "GC-LOCATION-PERMISSION"); yield break; }
            try
            {
                native = new Engine.GcAndroidLocation();
                if (!native.Enabled) { Fail(GcLocationState.Disabled, "GC-LOCATION-DISABLED"); }
                else if (!native.Start(Permission == GcLocationPermission.Precise)) { Fail(GcLocationState.Failed, "GC-LOCATION-NO-PROVIDER"); }
                else { Status = GcLocationState.Waiting; }
            }
            catch (AndroidJavaException) { Fail(GcLocationState.Failed, "GC-LOCATION-NATIVE"); }
            var deadline = Time.realtimeSinceStartupAsDouble + timeoutSeconds;
            while (Status == GcLocationState.Waiting && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            if (Status == GcLocationState.Waiting) Fail(GcLocationState.TimedOut, "GC-LOCATION-TIMEOUT");
#else
            Status = GcLocationState.Unsupported;
            yield break;
#endif
        }
#if UNITY_ANDROID && !UNITY_EDITOR
        static GcLocationPermission ReadPermission() => UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.FineLocation)
            ? GcLocationPermission.Precise : UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.CoarseLocation)
            ? GcLocationPermission.Approximate : GcLocationPermission.NotGranted;
#endif
        void Fail(GcLocationState state, string code) { CloseNative(); hasSample = false; Status = state; ErrorCode = code; }
        void CloseNative()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            var previous = native; native = null;
            if (previous != null) { try { previous.Dispose(); } catch (AndroidJavaException) { ErrorCode = "GC-LOCATION-STOP"; } }
#endif
        }
    }
}
