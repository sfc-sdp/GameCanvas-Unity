#nullable enable
using NUnit.Framework;
using UnityEngine;

namespace GameCanvas.Editor.Tests
{
    public class LocationServiceTest
    {
        [Test]
        public void UnsupportedPlatform_CompletesWithoutRequestAndCanRestart()
        {
            var obj = new GameObject("location service test");
            try
            {
                var service = new GcLocationService(obj.AddComponent<LocationTestHost>());
                Assert.That(service.Status, Is.EqualTo(GcLocationState.Idle));
                var first = service.Start();
                Assert.That(service.Status, Is.EqualTo(GcLocationState.Unsupported));
                Assert.That(service.ErrorCode, Is.EqualTo("GC-LOCATION-UNSUPPORTED"));
                Assert.That(service.TryGetSample(out _), Is.False);
                service.Stop();
                Assert.That(service.Status, Is.EqualTo(GcLocationState.Stopped));
                Assert.That(service.Start(), Is.GreaterThan(first));
                Assert.Throws<System.ArgumentOutOfRangeException>(() => service.Start(double.NaN));
                Assert.Throws<System.ArgumentOutOfRangeException>(() => service.Start(0));
            }
            finally { Object.DestroyImmediate(obj); }
        }
        [Test]
        public void Sample_PreservesNativePrecisionAndMilliseconds()
        {
            var value = new GcLocationSample(35.123456789123, 139.987654321987, 12.5, 1789776000.125, true);
            Assert.That(value.Latitude, Is.EqualTo(35.123456789123));
            Assert.That(value.Longitude, Is.EqualTo(139.987654321987));
            Assert.That(value.UnixTimeSeconds, Is.EqualTo(1789776000.125));
            Assert.That(value.IsMock, Is.True);
        }
    }
    public sealed class LocationTestHost : MonoBehaviour { }
}
