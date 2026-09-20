#nullable enable
using System;
using System.Linq;
using System.Reflection;
using GameCanvas;
using NUnit.Framework;

namespace GameCanvas.Editor.Tests
{
    public sealed class AccelerationServiceTest
    {
        [Test] public void UnsupportedWithoutASensor_InvalidRateDoesNotChangeState_StopClears()
        {
            var service = new GcAccelerationService();
            Assert.That(service.Status, Is.EqualTo(GcAccelerationState.Idle));
            Assert.That(service.HasValue, Is.False);
            Assert.Throws<ArgumentOutOfRangeException>(() => service.Start(0));
            Assert.That(service.Status, Is.EqualTo(GcAccelerationState.Idle));
            service.Start();
            Assert.That(service.Status, Is.EqualTo(GcAccelerationState.Unsupported));
            Assert.That(service.HasValue, Is.False);
            Assert.That(service.Events.Count, Is.Zero);
            var unsupported = service.Status;
            Assert.Throws<ArgumentOutOfRangeException>(() => service.Start(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => service.Start(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => service.Start(float.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => service.Start(float.PositiveInfinity));
            Assert.Throws<ArgumentOutOfRangeException>(() => service.Start(float.NegativeInfinity));
            Assert.That(service.Status, Is.EqualTo(unsupported));
            service.Stop();
            Assert.That(service.Status, Is.EqualTo(GcAccelerationState.Stopped));
            Assert.That(service.HasValue, Is.False);
            Assert.That(service.X, Is.Zero);
            Assert.That(service.Last.Time, Is.Zero);
        }

        [Test] public void PauseStopsAndStartWhilePausedDoesNotActivate_ResumeNeedsStart()
        {
            var service = new GcAccelerationService();
            service.Start();
            Assert.That(service.Status, Is.EqualTo(GcAccelerationState.Unsupported));
            service.SetPaused(true);
            Assert.That(service.Status, Is.EqualTo(GcAccelerationState.Stopped));
            service.Start();
            Assert.That(service.Status, Is.EqualTo(GcAccelerationState.Stopped));
            service.SetPaused(false);
            service.Tick();
            Assert.That(service.Status, Is.EqualTo(GcAccelerationState.Stopped));
            Assert.That(service.HasValue, Is.False);
            service.Start();
            Assert.That(service.Status, Is.EqualTo(GcAccelerationState.Unsupported));
            service.Stop();
        }

        [Test] public void SampleKeepsCanvasFlipAndDoubleTime()
        {
            var sample = GcAccelerationSample.FromRaw(0.25f, 0.5f, 0.75f, 86400.125, 0.002);
            Assert.That(sample.X, Is.EqualTo(0.25f));
            Assert.That(sample.Y, Is.EqualTo(-0.5f));
            Assert.That(sample.Z, Is.EqualTo(-0.75f));
            Assert.That(sample.RawX, Is.EqualTo(0.25f));
            Assert.That(sample.RawY, Is.EqualTo(0.5f));
            Assert.That(sample.RawZ, Is.EqualTo(0.75f));
            Assert.That(sample.Time, Is.EqualTo(86400.125));
            Assert.That(sample.DeltaTime, Is.EqualTo(0.002));
            foreach (var name in new[] { "X", "Y", "Z", "RawX", "RawY", "RawZ", "Time", "DeltaTime" })
                Assert.That(typeof(GcAccelerationSample).GetProperty(name), Is.Not.Null);
            Assert.That(typeof(GcAccelerationSample).GetProperty("Time")!.PropertyType, Is.EqualTo(typeof(double)));
            Assert.That(typeof(GcAccelerationSample).GetProperty("DeltaTime")!.PropertyType, Is.EqualTo(typeof(double)));
            Assert.That(Enum.GetNames(typeof(GcAccelerationState)),
                Is.EquivalentTo(new[] { "Idle", "Waiting", "Running", "Unsupported", "Stopped", "Failed" }));
        }

        [Test] public void LegacyAccelerationSurfaceIsRemoved()
        {
            var names = typeof(GcProxy).GetMembers().Select(m => m.Name).ToArray();
            foreach (var name in new[] { "AccelerationEventCount", "AccelerationEvents", "AccelerometerSamplingRate",
                "DidUpdateAccelerationThisFrame", "IsAccelerometerEnabled", "IsAccelerometerSupported",
                "LastAccelerationEvent", "TryGetAccelerationEvent", "TryGetAccelerationEventAll" })
                Assert.That(names, Does.Not.Contain(name));
            var assembly = typeof(GcProxy).Assembly;
            foreach (var name in new[] { "GcAccelerationEvent", "IInputAcceleration", "IInputAccelerationEx" })
                Assert.That(assembly.GetType("GameCanvas." + name), Is.Null);
            Assert.That(typeof(IGameCanvas).GetProperty("Acceleration")!.PropertyType, Is.EqualTo(typeof(GcAccelerationService)));
            Assert.That(typeof(GcAccelerationSample).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => p.PropertyType).All(t => t == typeof(float) || t == typeof(double)));
        }
    }
}
