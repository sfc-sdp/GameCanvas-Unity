#nullable enable
using System;
using GameCanvas;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameCanvas.Tests
{
    public sealed class AccelerationInputTest : InputTestFixture
    {
        GameObject go = null!;
        GcProxy gc = null!;
        Accelerometer sensor = null!;

        [SetUp] public override void Setup()
        {
            base.Setup();
            InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsManually;
            InputSystem.settings.compensateForScreenOrientation = false;
            sensor = InputSystem.AddDevice<Accelerometer>();
            go = new GameObject("Acceleration test", typeof(Camera), typeof(AudioListener), typeof(TestBehaviourBase));
            gc = (GcProxy)go.GetComponent<TestBehaviourBase>().GcProxy;
        }

        [TearDown] public override void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(go);
            base.TearDown();
        }

        void Frame() => gc.OnBeforeUpdate(DateTimeOffset.Now);

        void Push(Vector3 value, double time = -1)
        {
            sensor.acceleration.QueueValueChange(value, time);
            InputSystem.Update();
        }

        [Test] public void TwoHistoricalRecordsKeepDistinctValuesAndCanvasAxes()
        {
            gc.Acceleration.Start();
            InputSystem.Update();
            Frame();
            Push(new Vector3(0.25f, 0.5f, 0.75f), 86400.001);
            Push(new Vector3(-0.1f, 0.2f, -0.3f), 86400.003);
            Frame();
            var events = gc.Acceleration.Events;
            Assert.That(events.Count, Is.GreaterThanOrEqualTo(2));
            var first = events[events.Count - 2];
            var second = events[events.Count - 1];
            Assert.That(first.RawX, Is.EqualTo(0.25f));
            Assert.That(first.RawY, Is.EqualTo(0.5f));
            Assert.That(first.RawZ, Is.EqualTo(0.75f));
            Assert.That(first.X, Is.EqualTo(0.25f));
            Assert.That(first.Y, Is.EqualTo(-0.5f));
            Assert.That(first.Z, Is.EqualTo(-0.75f));
            Assert.That(second.RawX, Is.EqualTo(-0.1f));
            Assert.That(second.RawY, Is.EqualTo(0.2f));
            Assert.That(second.RawZ, Is.EqualTo(-0.3f));
            Assert.That(second.X, Is.EqualTo(-0.1f));
            Assert.That(second.Y, Is.EqualTo(-0.2f));
            Assert.That(second.Z, Is.EqualTo(0.3f));
            Assert.That(first.RawX, Is.Not.EqualTo(second.RawX));
            Assert.That(first.RawY, Is.Not.EqualTo(second.RawY));
            Assert.That(gc.Acceleration.HasValue, Is.True);
            Assert.That(gc.Acceleration.Updated, Is.True);
            Assert.That(gc.Acceleration.Status, Is.EqualTo(GcAccelerationState.Running));
            Assert.That(gc.Acceleration.Last.RawX, Is.EqualTo(-0.1f));
            Assert.That(gc.Acceleration.X, Is.EqualTo(-0.1f));
        }

        [Test] public void FirstDeltaTimeIsZero_LaterTimesKeepPrecisionAfterADay()
        {
            gc.Acceleration.Start();
            Push(new Vector3(1, 0, 0), 10.5);
            Frame();
            Assert.That(gc.Acceleration.Events.Count, Is.GreaterThanOrEqualTo(1));
            Assert.That(gc.Acceleration.Events[0].DeltaTime, Is.EqualTo(0));
            Frame();
            Push(new Vector3(0, 1, 0), 86400.001);
            Push(new Vector3(0, 0, 1), 86400.003);
            Frame();
            var events = gc.Acceleration.Events;
            Assert.That(events.Count, Is.GreaterThanOrEqualTo(2));
            var first = events[events.Count - 2];
            var second = events[events.Count - 1];
            Assert.That(first.Time, Is.EqualTo(86400.001).Within(1e-9));
            Assert.That(second.Time, Is.EqualTo(86400.003).Within(1e-9));
            Assert.That(second.Time - first.Time, Is.EqualTo(0.002).Within(1e-9));
            Assert.That(second.DeltaTime, Is.EqualTo(0.002).Within(1e-9));
            Assert.That(gc.Acceleration.Time, Is.EqualTo(86400.003).Within(1e-9));
            Assert.That(gc.Acceleration.DeltaTime, Is.EqualTo(0.002).Within(1e-9));
        }

        [Test] public void StopAndPauseClearValues_PauseDoesNotAutoResume()
        {
            gc.Acceleration.Start();
            Push(new Vector3(0.4f, 0, 0));
            Frame();
            Assert.That(gc.Acceleration.HasValue, Is.True);
            gc.Acceleration.Stop();
            Assert.That(gc.Acceleration.Status, Is.EqualTo(GcAccelerationState.Stopped));
            Assert.That(gc.Acceleration.HasValue, Is.False);
            Assert.That(gc.Acceleration.Events.Count, Is.Zero);
            Assert.That(gc.Acceleration.X, Is.Zero);
            gc.Acceleration.Start();
            Push(new Vector3(0.8f, 0, 0));
            Frame();
            Assert.That(gc.Acceleration.HasValue, Is.True);
            gc.OnPause();
            Assert.That(gc.Acceleration.Status, Is.EqualTo(GcAccelerationState.Stopped));
            Assert.That(gc.Acceleration.HasValue, Is.False);
            gc.OnUnpause();
            Push(new Vector3(0.9f, 0, 0));
            Frame();
            Assert.That(gc.Acceleration.Status, Is.EqualTo(GcAccelerationState.Stopped));
            Assert.That(gc.Acceleration.HasValue, Is.False);
            gc.Acceleration.Start();
            Push(new Vector3(0.3f, 0, 0));
            Frame();
            Assert.That(gc.Acceleration.HasValue, Is.True);
            Assert.That(gc.Acceleration.X, Is.EqualTo(0.3f));
            gc.Acceleration.Stop();
        }

        [Test] public void DisconnectFailsAndClears_UnsupportedHasNoSamples()
        {
            gc.Acceleration.Start();
            Push(new Vector3(0.2f, 0, 0));
            Frame();
            Assert.That(gc.Acceleration.HasValue, Is.True);
            InputSystem.RemoveDevice(sensor);
            Frame();
            Assert.That(gc.Acceleration.Status, Is.EqualTo(GcAccelerationState.Failed));
            Assert.That(gc.Acceleration.HasValue, Is.False);
            Assert.That(gc.Acceleration.Events.Count, Is.Zero);
            sensor = InputSystem.AddDevice<Accelerometer>();
            gc.Acceleration.Start();
            Push(new Vector3(0.6f, 0, 0));
            Frame();
            Assert.That(gc.Acceleration.Status, Is.EqualTo(GcAccelerationState.Running));
            Assert.That(gc.Acceleration.X, Is.EqualTo(0.6f));
            gc.Acceleration.Stop();
            InputSystem.RemoveDevice(sensor);

            UnityEngine.Object.DestroyImmediate(go);
            go = new GameObject("No accelerometer", typeof(Camera), typeof(AudioListener), typeof(TestBehaviourBase));
            gc = (GcProxy)go.GetComponent<TestBehaviourBase>().GcProxy;
            Assert.That(gc.Acceleration.IsSupported, Is.False);
            gc.Acceleration.Start();
            Frame();
            Assert.That(gc.Acceleration.Status, Is.EqualTo(GcAccelerationState.Unsupported));
            Assert.That(gc.Acceleration.HasValue, Is.False);
            Assert.That(gc.Acceleration.Events.Count, Is.Zero);
        }

        [Test] public void WarmPollingDoesNotAllocate()
        {
            gc.Acceleration.Start();
            for (int i = 0; i < 20; i++)
            {
                Push(new Vector3(0.1f, 0, 0));
                gc.Acceleration.Tick();
            }
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 1000; i++) gc.Acceleration.Tick();
            long bytes = GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.That(bytes, Is.Zero);
            Assert.That(gc.Acceleration.Status, Is.EqualTo(GcAccelerationState.Running));
            gc.Acceleration.Stop();
        }
    }
}
