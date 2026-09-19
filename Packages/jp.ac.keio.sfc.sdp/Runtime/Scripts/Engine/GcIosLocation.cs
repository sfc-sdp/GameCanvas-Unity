#nullable enable
using System;
using System.Collections;
using UnityEngine;

namespace GameCanvas.Engine
{
    // CLAuthorizationStatus / CLAccuracyAuthorization の数値を公開契約へ写す。Editorテストからも使う。
    internal static class GcIosLocationMap
    {
        internal const int AuthNotDetermined = 0;
        internal const int AuthRestricted = 1;
        internal const int AuthDenied = 2;
        internal const int AuthAlways = 3;
        internal const int AuthWhenInUse = 4;
        internal const int AccuracyFull = 0;
        internal const int AccuracyReduced = 1;

        internal static GcLocationPermission ToPermission(int authorization, int accuracy)
        {
            if (authorization != AuthAlways && authorization != AuthWhenInUse)
                return authorization == AuthNotDetermined ? GcLocationPermission.Unknown : GcLocationPermission.NotGranted;
            return accuracy == AccuracyReduced ? GcLocationPermission.Approximate : GcLocationPermission.Precise;
        }

        internal static bool NeedsRequest(int authorization) => authorization == AuthNotDetermined;
        internal static bool IsRestricted(int authorization) => authorization == AuthRestricted;
    }

#if UNITY_IOS && !UNITY_EDITOR
    // CoreLocation の double を Unity Input.location の float を経由せずに受け取る。
    internal sealed class GcIosLocation : IGcLocationBackend
    {
        IntPtr handle;
        public GcIosLocation() => handle = GcLocationCreate();
        public bool Enabled => handle != IntPtr.Zero && GcLocationIsEnabled(handle) != 0;
        public bool NeedsPermissionRequest => handle != IntPtr.Zero && GcIosLocationMap.NeedsRequest(GcLocationAuthorization(handle));
        public bool IsRestricted => handle != IntPtr.Zero && GcIosLocationMap.IsRestricted(GcLocationAuthorization(handle));
        public GcLocationPermission ReadPermission()
            => handle == IntPtr.Zero ? GcLocationPermission.NotGranted
                : GcIosLocationMap.ToPermission(GcLocationAuthorization(handle), GcLocationAccuracy(handle));
        public IEnumerator RequestPermission(Action<bool> timedOut)
        {
            if (handle == IntPtr.Zero) { timedOut(true); yield break; }
            GcLocationRequestWhenInUse(handle);
            var deadline = Time.realtimeSinceStartupAsDouble + 30;
            while (NeedsPermissionRequest && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            timedOut(NeedsPermissionRequest);
        }
        public bool Start(bool precise) => handle != IntPtr.Zero && GcLocationStart(handle, precise ? 1 : 0) != 0;
        public bool TryRead(out GcLocationSample sample)
        {
            sample = default;
            if (handle == IntPtr.Zero) return false;
            if (GcLocationTryRead(handle, out var latitude, out var longitude, out var accuracyMeters, out var unixTimeSeconds, out var isMock) == 0)
                return false;
            sample = new GcLocationSample(latitude, longitude, accuracyMeters, unixTimeSeconds, isMock != 0);
            return true;
        }
        public void Dispose()
        {
            var previous = handle;
            handle = IntPtr.Zero;
            if (previous == IntPtr.Zero) return;
            GcLocationStop(previous);
            GcLocationDestroy(previous);
        }

        [System.Runtime.InteropServices.DllImport("__Internal")] static extern IntPtr GcLocationCreate();
        [System.Runtime.InteropServices.DllImport("__Internal")] static extern void GcLocationDestroy(IntPtr handle);
        [System.Runtime.InteropServices.DllImport("__Internal")] static extern int GcLocationIsEnabled(IntPtr handle);
        [System.Runtime.InteropServices.DllImport("__Internal")] static extern int GcLocationAuthorization(IntPtr handle);
        [System.Runtime.InteropServices.DllImport("__Internal")] static extern int GcLocationAccuracy(IntPtr handle);
        [System.Runtime.InteropServices.DllImport("__Internal")] static extern void GcLocationRequestWhenInUse(IntPtr handle);
        [System.Runtime.InteropServices.DllImport("__Internal")] static extern int GcLocationStart(IntPtr handle, int precise);
        [System.Runtime.InteropServices.DllImport("__Internal")] static extern void GcLocationStop(IntPtr handle);
        [System.Runtime.InteropServices.DllImport("__Internal")] static extern int GcLocationTryRead(IntPtr handle, out double latitude, out double longitude, out double accuracyMeters, out double unixTimeSeconds, out int isMock);
    }
#endif
}
