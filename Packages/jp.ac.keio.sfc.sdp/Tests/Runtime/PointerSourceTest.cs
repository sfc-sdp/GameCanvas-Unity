#nullable enable
using System;
using GameCanvas.Engine;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace GameCanvas.Tests
{
    public sealed class PointerSourceTest : InputTestFixture
    {
        InputSettings.UpdateMode previousMode;
        Touchscreen screen = null!;
        Mouse mouse = null!;
        GcPointerSource source = null!;
        [SetUp] public override void Setup()
        {
            base.Setup();
            previousMode = InputSystem.settings.updateMode;
            InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsManually;
            InputSystem.AddDevice<Keyboard>();
            screen = InputSystem.AddDevice<Touchscreen>(); mouse = InputSystem.AddDevice<Mouse>();
            source = new GcPointerSource(); source.Pending.Clear();
        }
        [TearDown] public override void TearDown()
        {
            source.Dispose(); InputSystem.RemoveDevice(screen); InputSystem.RemoveDevice(mouse);
            InputSystem.settings.updateMode = previousMode;
            base.TearDown();
        }
        void Touch(UnityEngine.InputSystem.TouchPhase phase, float x)
        {
            InputSystem.QueueStateEvent(screen, new TouchState { touchId = 123, phase = phase, position = new Vector2(x, 40) });
            InputSystem.Update();
        }
        void Mouse(bool held, float x)
        {
            InputSystem.QueueStateEvent(mouse, new MouseState { position = new Vector2(x, 50) }.WithButton(MouseButton.Left, held));
            InputSystem.Update();
        }
        [Test] public void RecordsActualHistoricalTouchPositionsBeforeGameFrame()
        {
            Touch(UnityEngine.InputSystem.TouchPhase.Began, 10);
            Touch(UnityEngine.InputSystem.TouchPhase.Moved, 30);
            Touch(UnityEngine.InputSystem.TouchPhase.Ended, 90);
            var records = source.Pending.FindAll(r => r.Device == screen.deviceId);
            Assert.That(records[0].Phase, Is.EqualTo(GcPointerEventPhase.Begin));
            Assert.That(records[0].Screen.x, Is.EqualTo(10));
            Assert.That(records.Exists(r => r.Phase == GcPointerEventPhase.Hold && r.Screen.x == 30));
            Assert.That(records[^1].Phase, Is.EqualTo(GcPointerEventPhase.End));
            Assert.That(records[^1].Screen.x, Is.EqualTo(90));
        }
        [Test] public void MousePressReleaseAndHoverRemainDistinct()
        {
            Mouse(false, 5); Mouse(true, 10); Mouse(false, 90);
            var records = source.Pending.FindAll(r => r.Device == mouse.deviceId);
            Assert.That(records.Exists(r => r.Phase == GcPointerEventPhase.Hover && r.Screen.x == 5));
            Assert.That(records.Exists(r => r.Phase == GcPointerEventPhase.Begin && r.Screen.x == 10));
            Assert.That(records.Exists(r => r.Phase == GcPointerEventPhase.End && r.Screen.x == 90));
        }
        [Test] public void RegisteringAnUnusedMouseDoesNotInventAHoverAtTheOrigin()
        {
            var unused = InputSystem.AddDevice<Mouse>();
            try
            {
                InputSystem.Update();
                Assert.That(source.Pending.Exists(r => r.Device == unused.deviceId), Is.False);
            }
            finally { InputSystem.RemoveDevice(unused); }
        }
        [Test] public void ResetCancelsInsteadOfReleasingAndHotplugIsObserved()
        {
            Mouse(true, 10); source.Pending.Clear(); InputSystem.ResetDevice(mouse);
            Assert.That(source.Pending.Exists(r => r.CancelDevice && r.Device == mouse.deviceId));
            Assert.That(source.Pending.Exists(r => r.Phase == GcPointerEventPhase.End), Is.False);
            var second = InputSystem.AddDevice<Touchscreen>();
            try
            {
                InputSystem.QueueStateEvent(second, new TouchState { touchId = 1, phase = UnityEngine.InputSystem.TouchPhase.Began });
                InputSystem.Update();
                Assert.That(source.Pending.Exists(r => r.Device == second.deviceId && r.Phase == GcPointerEventPhase.Begin));
            }
            finally { InputSystem.RemoveDevice(second); }
        }
        [Test] public void ResumeRequiresAReleaseBeforeNewMousePress()
        {
            Mouse(true, 10); source.SetSuspended(true); source.SetSuspended(false); source.Pending.Clear();
            Mouse(true, 20);
            Assert.That(source.Pending.Exists(r => r.Phase == GcPointerEventPhase.Begin), Is.False);
            Mouse(false, 30); Mouse(true, 40);
            Assert.That(source.Pending.Exists(r => r.Phase == GcPointerEventPhase.Begin && r.Screen.x == 40));
        }
        [Test] public void ResumeWithReleasedMouseAcceptsFirstNewPress()
        {
            source.SetSuspended(true); source.SetSuspended(false); source.Pending.Clear();
            Mouse(true, 40);
            Assert.That(source.Pending.Exists(r => r.Phase == GcPointerEventPhase.Begin && r.Screen.x == 40));
        }
        [Test] public void PublicApiSharesEventsAndCancellationNeverBecomesATap()
        {
            var go = new GameObject("Pointer integration", typeof(Camera), typeof(AudioListener), typeof(TestBehaviourBase));
            try
            {
                var gc = (GcProxy)go.GetComponent<TestBehaviourBase>().GcProxy;
                gc.ChangeCanvasSize(720, 1280);
                Touch(UnityEngine.InputSystem.TouchPhase.Began, 10);
                Touch(UnityEngine.InputSystem.TouchPhase.Ended, 20);
                gc.OnBeforeUpdate(DateTimeOffset.Now);
                Assert.That(gc.Pointer.Down && gc.Pointer.Up && !gc.Pointer.Held);
                gc.ScreenToCanvasPoint(new float2(10, 40), out float2 start);
                gc.ScreenToCanvasPoint(new float2(20, 40), out float2 end);
                Assert.That(gc.Pointer.StartX, Is.EqualTo(start.x).Within(.01));
                Assert.That(gc.Pointer.X, Is.EqualTo(end.x).Within(.01));
                Assert.That(gc.PointerEvents.Count, Is.GreaterThanOrEqualTo(2));
                gc.OnBeforeUpdate(DateTimeOffset.Now);
                Touch(UnityEngine.InputSystem.TouchPhase.Began, 30);
                Touch(UnityEngine.InputSystem.TouchPhase.Canceled, 30);
                gc.OnBeforeUpdate(DateTimeOffset.Now);
                Assert.That(gc.Pointer.Cancelled && !gc.Pointer.Up);
                Assert.That(gc.Taps.Count, Is.Zero);
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }
    }
}
