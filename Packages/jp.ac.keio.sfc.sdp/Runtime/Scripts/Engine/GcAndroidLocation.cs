#nullable enable
#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameCanvas.Engine
{
    // Androidのdouble座標をfloatへ変換せずに受け取る。呼び出し元が権限と期限を管理する。
    internal sealed class GcAndroidLocation : IGcLocationBackend
    {
        readonly AndroidJavaObject manager;
        readonly Listener listener = new();
        public GcAndroidLocation()
        {
            using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
            manager = activity.Call<AndroidJavaObject>("getSystemService", "location");
        }
        public bool Enabled
        {
            get
            {
                using var version = new AndroidJavaClass("android.os.Build$VERSION");
                if (version.GetStatic<int>("SDK_INT") >= 28) return manager.Call<bool>("isLocationEnabled");
                return manager.Call<bool>("isProviderEnabled", "gps") || manager.Call<bool>("isProviderEnabled", "network");
            }
        }
        public bool Start(bool precise)
        {
            using var looperClass = new AndroidJavaClass("android.os.Looper");
            using var looper = looperClass.CallStatic<AndroidJavaObject>("getMainLooper");
            var enabled = new System.Collections.Generic.HashSet<string>();
            using var providers = manager.Call<AndroidJavaObject>("getProviders", true);
            for (var i = 0; i < providers.Call<int>("size"); i++)
            {
                using var provider = providers.Call<AndroidJavaObject>("get", i);
                enabled.Add(provider.Call<string>("toString"));
            }
            // fusedがある端末ではOSの統合測位を使う。Google Play Servicesへの依存は追加しない。
            var candidates = enabled.Contains("fused") ? new[] { "fused" }
                : precise ? new[] { "network", "gps" } : new[] { "network" };
            var started = false;
            foreach (var provider in candidates)
            {
                if (!enabled.Contains(provider)) continue;
                manager.Call("requestLocationUpdates", provider, 1000L, 1f, listener, looper);
                started = true;
            }
            return started;
        }
        public bool NeedsPermissionRequest => ReadPermission() == GcLocationPermission.NotGranted;
        public bool IsRestricted => false;
        public GcLocationPermission ReadPermission() => UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.FineLocation)
            ? GcLocationPermission.Precise : UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.CoarseLocation)
            ? GcLocationPermission.Approximate : GcLocationPermission.NotGranted;
        public IEnumerator RequestPermission(Action<bool> timedOut)
            => GcAndroidPermission.Request(new[] { UnityEngine.Android.Permission.CoarseLocation, UnityEngine.Android.Permission.FineLocation }, timedOut);
        public bool TryRead(out GcLocationSample sample) => listener.TryRead(out sample);
        public void Dispose()
        {
            listener.Close();
            try { manager.Call("removeUpdates", listener); }
            finally { manager.Dispose(); }
        }
        [Preserve]
        sealed class Listener : AndroidJavaProxy
        {
            readonly object gate = new();
            GcLocationSample last;
            bool available, closed;
            public Listener() : base("android.location.LocationListener") { }
            public bool TryRead(out GcLocationSample sample)
            {
                lock (gate) { sample = last; return available && !closed; }
            }
            public void Close() { lock (gate) { closed = true; available = false; } }
            [Preserve] public void onLocationChanged(AndroidJavaObject location)
            {
                // Android 12以降のList<Location>通知と、従来のLocation通知を両方扱う。
                using var type = new AndroidJavaClass("android.location.Location");
                if (AndroidJNI.IsInstanceOf(location.GetRawObject(), type.GetRawClass())) Read(location);
                else
                {
                    var count = location.Call<int>("size");
                    if (count == 0) return;
                    using var latest = location.Call<AndroidJavaObject>("get", count - 1);
                    Read(latest);
                }
            }
            void Read(AndroidJavaObject location)
            {
                var sample = new GcLocationSample(location.Call<double>("getLatitude"), location.Call<double>("getLongitude"),
                    location.Call<float>("getAccuracy"), location.Call<long>("getTime") / 1000.0,
                    location.Call<bool>("isFromMockProvider"));
                lock (gate)
                {
                    if (closed) return;
                    last = sample; available = true;
                }
            }
            [Preserve] public void onProviderEnabled(string provider) { }
            [Preserve] public void onProviderDisabled(string provider) { }
            [Preserve] public void onStatusChanged(string provider, int status, AndroidJavaObject extras) { }
        }
    }
}
#endif
