#nullable enable
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GameCanvas.Tests
{
    public class LocationLifecycleTest
    {
        [UnityTest]
        public IEnumerator InjectedBackend_StopClearsSampleAndRestartDoesNotReuseIt()
        {
            var obj = new GameObject("location lifecycle test");
            FakeLocationBackend? current = null;
            try
            {
                var first = new GcLocationSample(35.123456789123, 139.987654321987, 5, 1789776000.125, true);
                var second = new GcLocationSample(34.5, 135.5, 12.5, 1789776001.5, true);
                var service = new GcLocationService(obj.AddComponent<LocationLifecycleTestHost>(), () => current = new FakeLocationBackend
                {
                    Permission = GcLocationPermission.Precise
                });
                var operation = service.Start(2);
                yield return null;
                Assert.That(service.Status, Is.EqualTo(GcLocationState.Waiting));
                Assert.That(service.TryGetSample(out _), Is.False);
                current!.Sample = first;
                service.Tick();
                Assert.That(service.Status, Is.EqualTo(GcLocationState.Running));
                Assert.That(service.TryGetSample(out var got), Is.True);
                Assert.That(got.Latitude, Is.EqualTo(first.Latitude));
                Assert.That(got.UnixTimeSeconds, Is.EqualTo(first.UnixTimeSeconds));
                Assert.That(got.IsMock, Is.True);
                service.Stop();
                Assert.That(service.Status, Is.EqualTo(GcLocationState.Stopped));
                Assert.That(service.TryGetSample(out _), Is.False);
                Assert.That(current.Disposed, Is.True);

                Assert.That(service.Start(2), Is.GreaterThan(operation));
                yield return null;
                service.Tick();
                Assert.That(service.Status, Is.EqualTo(GcLocationState.Waiting));
                Assert.That(service.TryGetSample(out _), Is.False);
                current!.Sample = second;
                service.Tick();
                Assert.That(service.TryGetSample(out got), Is.True);
                Assert.That(got.Latitude, Is.EqualTo(second.Latitude));
                Assert.That(got.UnixTimeSeconds, Is.EqualTo(second.UnixTimeSeconds));
            }
            finally { UnityEngine.Object.DestroyImmediate(obj); }
        }

        [UnityTest]
        public IEnumerator InjectedBackend_StopCancelsPermissionWait()
        {
            var obj = new GameObject("location permission cancel test");
            FakeLocationBackend? current = null;
            try
            {
                var service = new GcLocationService(obj.AddComponent<LocationLifecycleTestHost>(), () => current = new FakeLocationBackend
                {
                    Permission = GcLocationPermission.Unknown,
                    NeedsPermissionRequest = true,
                    HoldPermission = true
                });
                service.Start(2);
                yield return null;
                Assert.That(service.Status, Is.EqualTo(GcLocationState.RequestingPermission));
                service.Stop();
                Assert.That(service.Status, Is.EqualTo(GcLocationState.Stopped));
                Assert.That(service.TryGetSample(out _), Is.False);
                Assert.That(current!.Disposed, Is.True);
            }
            finally { UnityEngine.Object.DestroyImmediate(obj); }
        }

        [UnityTest]
        public IEnumerator InjectedBackend_TimeoutDisabledDeniedRestricted()
        {
            var obj = new GameObject("location contract test");
            try
            {
                var host = obj.AddComponent<LocationLifecycleTestHost>();
                var waiting = new FakeLocationBackend { Permission = GcLocationPermission.Precise };
                var service = new GcLocationService(host, () => waiting);
                service.Start(0.05);
                var deadline = Time.realtimeSinceStartupAsDouble + 2;
                while (service.Status == GcLocationState.RequestingPermission || service.Status == GcLocationState.Waiting)
                {
                    if (Time.realtimeSinceStartupAsDouble > deadline) break;
                    yield return null;
                }
                Assert.That(service.Status, Is.EqualTo(GcLocationState.TimedOut));
                Assert.That(service.ErrorCode, Is.EqualTo("GC-LOCATION-TIMEOUT"));
                Assert.That(service.TryGetSample(out _), Is.False);
                Assert.That(waiting.Disposed, Is.True);

                var disabled = new FakeLocationBackend { Permission = GcLocationPermission.Precise, Enabled = false };
                service = new GcLocationService(host, () => disabled);
                service.Start(1);
                yield return null;
                Assert.That(service.Status, Is.EqualTo(GcLocationState.Disabled));
                Assert.That(service.ErrorCode, Is.EqualTo("GC-LOCATION-DISABLED"));

                var denied = new FakeLocationBackend { Permission = GcLocationPermission.NotGranted };
                service = new GcLocationService(host, () => denied);
                service.Start(1);
                yield return null;
                Assert.That(service.Status, Is.EqualTo(GcLocationState.NotGranted));
                Assert.That(service.ErrorCode, Is.EqualTo("GC-LOCATION-PERMISSION"));

                var disabledDenied = new FakeLocationBackend { Permission = GcLocationPermission.NotGranted, Enabled = false };
                service = new GcLocationService(host, () => disabledDenied);
                service.Start(1);
                yield return null;
                Assert.That(service.Status, Is.EqualTo(GcLocationState.Disabled));
                Assert.That(service.ErrorCode, Is.EqualTo("GC-LOCATION-DISABLED"));

                var restricted = new FakeLocationBackend { Permission = GcLocationPermission.NotGranted, Restricted = true };
                service = new GcLocationService(host, () => restricted);
                service.Start(1);
                yield return null;
                Assert.That(service.Status, Is.EqualTo(GcLocationState.NotGranted));
                Assert.That(service.ErrorCode, Is.EqualTo("GC-LOCATION-RESTRICTED"));
            }
            finally { UnityEngine.Object.DestroyImmediate(obj); }
        }
    }

    public sealed class LocationLifecycleTestHost : MonoBehaviour { }

    sealed class FakeLocationBackend : IGcLocationBackend
    {
        public bool Enabled { get; set; } = true;
        public bool NeedsPermissionRequest { get; set; }
        public bool Restricted { get; set; }
        public bool HoldPermission { get; set; }
        public bool PermissionTimedOut { get; set; }
        public bool StartFails { get; set; }
        public bool Disposed { get; private set; }
        public GcLocationPermission Permission { get; set; } = GcLocationPermission.Precise;
        public GcLocationSample? Sample;
        public bool IsRestricted => Restricted;
        public GcLocationPermission ReadPermission() => Permission;
        public IEnumerator RequestPermission(System.Action<bool> timedOut)
        {
            while (HoldPermission) yield return null;
            timedOut(PermissionTimedOut);
        }
        public bool Start(bool precise) => !Disposed && !StartFails;
        public bool TryRead(out GcLocationSample sample)
        {
            if (Disposed || Sample is not { } value) { sample = default; return false; }
            sample = value; return true;
        }
        public void Dispose()
        {
            Disposed = true;
            Sample = null;
            HoldPermission = false;
        }
    }
}
