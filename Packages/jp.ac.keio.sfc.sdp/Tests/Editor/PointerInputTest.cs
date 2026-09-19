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
    public sealed class PointerInputTest
    {
        static void Feed(GcPointerTracker tracker, GcPointerEventPhase phase, float x, float y,
            int device = 1, int contact = 1, double time = 1, GcPointerType kind = GcPointerType.Touch)
        {
            tracker.Accept(new GcPointerRecord(device, contact, kind, phase, new float2(x, y), time),
                new GcPoint(x, y), x >= 0 && y >= 0 && x < 720 && y < 1280);
        }

        [Test]
        public void QuickDragKeepsStartFinalPositionAndBothTransitions()
        {
            var t = new GcPointerTracker(); t.BeginFrame(1, 1.3);
            Feed(t, GcPointerEventPhase.Begin, 20, 30, time: 1);
            Feed(t, GcPointerEventPhase.Hold, 50, 60, time: 1.1);
            Feed(t, GcPointerEventPhase.End, 80, 90, time: 1.2); t.EndFrame();
            var p = t.Pointer;
            Assert.That(p.Down && p.Up && !p.Held && !p.Cancelled && p.Present);
            Assert.That(p.StartX, Is.EqualTo(20)); Assert.That(p.X, Is.EqualTo(80));
            Assert.That(p.Delta.X, Is.EqualTo(60)); Assert.That(p.Duration, Is.EqualTo(.2).Within(.0001));
            Assert.That(t.Events.Count, Is.EqualTo(3));
            Assert.That(t.Events[0].X, Is.EqualTo(20)); Assert.That(t.Events[2].X, Is.EqualTo(80));
            t.BeginFrame(2, 2); t.EndFrame();
            Assert.That(t.Pointers.Count, Is.Zero); Assert.That(t.Pointer.Present, Is.False);
            Assert.That(p.Up, Is.True, "値として保存した状態は次フレームでも変わらない");
        }

        [Test]
        public void FirstFrameIsHeldAndIdleFramesClearTransitions()
        {
            var t = new GcPointerTracker(); t.BeginFrame(1, 1);
            Feed(t, GcPointerEventPhase.Begin, 20, 30); t.EndFrame();
            int id = t.Pointer.Id; Assert.That(t.Pointer.Down && t.Pointer.Held);
            t.BeginFrame(2, 2); t.EndFrame();
            Assert.That(t.Pointer.Id, Is.EqualTo(id)); Assert.That(t.Pointer.Held && !t.Pointer.Down);
            Assert.That(t.Pointer.Delta.X, Is.Zero); Assert.That(t.Pointer.Duration, Is.EqualTo(1));
        }

        [Test]
        public void DeviceIdsDoNotCollideAndPrimaryDoesNotJumpToRemainingFinger()
        {
            var t = new GcPointerTracker(); t.BeginFrame(1, 1);
            Feed(t, GcPointerEventPhase.Begin, 20, 30, device: 1);
            Feed(t, GcPointerEventPhase.Begin, 50, 60, device: 2); t.EndFrame();
            int first = t.Pointer.Id, second = t.Pointers[1].Id;
            Assert.That(first, Is.Not.EqualTo(second));
            t.BeginFrame(2, 2); Feed(t, GcPointerEventPhase.End, 25, 30, device: 1, time: 2); t.EndFrame();
            Assert.That(t.Pointer.Id, Is.EqualTo(first)); Assert.That(t.Pointer.Up);
            t.BeginFrame(3, 3); t.EndFrame();
            Assert.That(t.Pointers[0].Id, Is.EqualTo(second)); Assert.That(t.Pointers[0].Held);
            Assert.That(t.Pointer.Present, Is.False);
            t.BeginFrame(4, 4);
            Feed(t, GcPointerEventPhase.Begin, 80, 90, device: 3, time: 4); t.EndFrame();
            Assert.That(t.Pointer.Id, Is.Not.EqualTo(second)); Assert.That(t.Pointer.Down);
        }

        [Test]
        public void CancelDoesNotBecomeUpAndEndsCapture()
        {
            var t = new GcPointerTracker(); t.BeginFrame(1, 1);
            Feed(t, GcPointerEventPhase.Begin, 20, 30);
            Feed(t, GcPointerEventPhase.Cancelled, 30, 40); t.EndFrame();
            Assert.That(t.Pointer.Cancelled && !t.Pointer.Up && !t.Pointer.Held);
            Assert.That(t.Events[1].Phase, Is.EqualTo(GcPointerEventPhase.Cancelled));
            Assert.That(UnityEngine.InputSystem.TouchPhase.Canceled.ToGcPointerPhase(), Is.EqualTo(GcPointerEventPhase.Cancelled));
            t.BeginFrame(2, 2); t.EndFrame(); Assert.That(t.Pointers.Count, Is.Zero);
        }

        [Test]
        public void ContactNumberReuseInOneFrameGetsNewIdAndPreservesOrder()
        {
            var t = new GcPointerTracker(); t.BeginFrame(1, 1);
            Feed(t, GcPointerEventPhase.Begin, 10, 10);
            Feed(t, GcPointerEventPhase.End, 20, 20);
            Feed(t, GcPointerEventPhase.Begin, 30, 30); t.EndFrame();
            Assert.That(t.Pointers.Count, Is.EqualTo(2));
            Assert.That(t.Pointers[0].Id, Is.Not.EqualTo(t.Pointers[1].Id));
            Assert.That(t.Events[0].Id, Is.EqualTo(t.Events[1].Id));
            Assert.That(t.Events[2].Id, Is.EqualTo(t.Pointers[1].Id));
        }

        [Test]
        public void LostFocusCancelsEveryHeldPointerWithoutInventingPressOnResume()
        {
            var t = new GcPointerTracker(); t.BeginFrame(1, 1);
            Feed(t, GcPointerEventPhase.Begin, 20, 30);
            Feed(t, GcPointerEventPhase.Begin, 50, 60, contact: 2);
            t.Accept(new GcPointerRecord(-1, 0, GcPointerType.Unknown, GcPointerEventPhase.Cancelled,
                default, 1, false, true), default, false); t.EndFrame();
            Assert.That(t.Pointers[0].Cancelled && t.Pointers[1].Cancelled);
            t.BeginFrame(2, 2); Feed(t, GcPointerEventPhase.Hold, 50, 60, contact: 2); t.EndFrame();
            Assert.That(t.Pointers.Count, Is.Zero);
        }

        [Test]
        public void HoverHasPositionButNoPressAndOutsideCoordinatesRemainUsable()
        {
            var t = new GcPointerTracker(); t.BeginFrame(1, 1);
            Feed(t, GcPointerEventPhase.Hover, -10, 30, kind: GcPointerType.Mouse); t.EndFrame();
            Assert.That(t.Pointer.Present && !t.Pointer.Inside && !t.Pointer.Held && !t.Pointer.Down);
            Assert.That(t.Pointer.X, Is.EqualTo(-10));
        }

        [Test]
        public void RepeatedMouseClicksDoNotAccumulateHoverEntries()
        {
            var t = new GcPointerTracker();
            for (int frame = 0; frame < 20; frame++)
            {
                t.BeginFrame(frame, frame);
                Feed(t, GcPointerEventPhase.Begin, 10, 10, kind: GcPointerType.Mouse);
                Feed(t, GcPointerEventPhase.End, 20, 20, kind: GcPointerType.Mouse);
                Feed(t, GcPointerEventPhase.Hover, 30, 30, kind: GcPointerType.Mouse); t.EndFrame();
                Assert.That(t.Pointers.Count, Is.LessThanOrEqualTo(2));
            }
        }

        [Test]
        public void CountIndexAndForeachAgreeWithoutPerFrameManagedAllocation()
        {
            var t = new GcPointerTracker(); t.BeginFrame(0, 0);
            Feed(t, GcPointerEventPhase.Begin, 20, 30, time: 0); t.EndFrame();
            int sum = 0;
            for (int frame = 0; frame < 10; frame++)
            { t.BeginFrame(frame, frame); t.EndFrame(); foreach (var p in t.Pointers) sum += p.Id; }
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int frame = 10; frame < 1010; frame++)
            {
                t.BeginFrame(frame, frame); t.EndFrame();
                for (int i = 0; i < t.Pointers.Count; i++) sum += t.Pointers[i].Id;
                foreach (var p in t.Pointers) sum -= p.Id;
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.That(allocated, Is.Zero); Assert.That(sum, Is.EqualTo(10));
            Assert.Throws<ArgumentOutOfRangeException>(() => { var p = t.Pointers[1]; });
            Assert.That(default(GcReadOnlyList<GcPointer>).Count, Is.Zero);
        }
        [Test]
        public void TapsKeepStartPositionAndOrderAndClearNextFrame()
        {
            var t = new GcPointerTracker(); t.BeginFrame(1, 1.1);
            Feed(t, GcPointerEventPhase.Begin, 10, 20, time: 1);
            Feed(t, GcPointerEventPhase.End, 12, 22, time: 1.05);
            Feed(t, GcPointerEventPhase.Begin, 50, 60, time: 1.06);
            Feed(t, GcPointerEventPhase.End, 51, 60, time: 1.1); t.EndFrame();
            Assert.That(t.Taps.Count, Is.EqualTo(2));
            Assert.That(t.Taps[0], Is.EqualTo(new GcPoint(10, 20)));
            Assert.That(t.Taps[1], Is.EqualTo(new GcPoint(50, 60)));
            t.BeginFrame(2, 2); t.EndFrame(); Assert.That(t.Taps.Count, Is.Zero);
        }
        [TestCase(GcPointerEventPhase.Cancelled, 1.05, 0)]
        [TestCase(GcPointerEventPhase.End, 1.5, 0)]
        [TestCase(GcPointerEventPhase.End, 1.05, 20)]
        public void InterruptedLongOrWanderingContactIsNotATap(GcPointerEventPhase end, double time, float excursion)
        {
            var t = new GcPointerTracker(); t.BeginFrame(1, time);
            Feed(t, GcPointerEventPhase.Begin, 10, 10, time: 1);
            Feed(t, GcPointerEventPhase.Hold, 10 + excursion, 10, time: 1.01);
            Feed(t, end, 10, 10, time: time); t.EndFrame();
            Assert.That(t.Taps.Count, Is.Zero);
        }
        [Test]
        public void TapThresholdsAreInclusiveAndConfigurable()
        {
            var t = new GcPointerTracker { TapSettings = new GcTapSettings(10, .5f) };
            t.BeginFrame(1, 1.5);
            Feed(t, GcPointerEventPhase.Begin, 0, 0, time: 1);
            Feed(t, GcPointerEventPhase.End, 10, 0, time: 1.5); t.EndFrame();
            Assert.That(t.Taps.Count, Is.EqualTo(1));
        }
        [Test]
        public void PointerEventsAndDurationKeepPrecisionAfterLongUptime()
        {
            var t = new GcPointerTracker(); t.BeginFrame(1, 86400.01);
            Feed(t, GcPointerEventPhase.Begin, 10, 10, time: 86400.001);
            Feed(t, GcPointerEventPhase.End, 10, 10, time: 86400.003); t.EndFrame();
            Assert.That(t.Pointer.Duration, Is.EqualTo(.002).Within(1e-9));
            Assert.That(t.Events[1].Time - t.Events[0].Time, Is.EqualTo(.002).Within(1e-9));
        }
    }

}
