#nullable enable
using System;
using GameCanvas;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace GameCanvas.Tests
{
    public sealed class KeyboardInputTest : InputTestFixture
    {
        GameObject go = null!;
        GcProxy gc = null!;
        Keyboard keyboard = null!;
        [SetUp] public override void Setup()
        {
            base.Setup();
            InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsManually;
            keyboard = InputSystem.AddDevice<Keyboard>();
            go = new GameObject("Keyboard test", typeof(Camera), typeof(AudioListener), typeof(TestBehaviourBase));
            gc = (GcProxy)go.GetComponent<TestBehaviourBase>().GcProxy;
        }
        [TearDown] public override void TearDown()
        { UnityEngine.Object.DestroyImmediate(go); base.TearDown(); }
        void Send(Keyboard device, params Key[] keys)
        { InputSystem.QueueStateEvent(device, new KeyboardState(keys)); InputSystem.Update(); }
        void Frame() => gc.OnBeforeUpdate(DateTimeOffset.Now);
        [Test] public void QuickPressReleasePreservesBothEdgesAndOrder()
        {
            Send(keyboard, Key.Space); Send(keyboard); Frame();
            var key = gc.Key(GcKey.Space);
            Assert.That(key.Down && key.Up && !key.Held && !key.Cancelled);
            var events = gc.KeyEvents;
            Assert.That(events.Count, Is.EqualTo(2));
            Assert.That(events[0].Phase, Is.EqualTo(GcKeyEventPhase.Down));
            Assert.That(events[1].Phase, Is.EqualTo(GcKeyEventPhase.Up));
            Frame(); Assert.That(gc.Key(GcKey.Space), Is.EqualTo(default(GcKeyState)));
        }
        [Test] public void HeldIncludesDownAndDurationNeverGoesNegative()
        {
            Send(keyboard, Key.A); Frame(); Assert.That(gc.Key(GcKey.A).Down && gc.Key(GcKey.A).Held);
            Frame(); Assert.That(gc.Key(GcKey.A).Held && !gc.Key(GcKey.A).Down);
            Assert.That(gc.Key(GcKey.A).Duration, Is.GreaterThanOrEqualTo(0));
            Assert.That(gc.KeyEvents.Count, Is.Zero, "Holding does not create a new input change");
            Send(keyboard); Frame(); Assert.That(gc.Key(GcKey.A).Up && !gc.Key(GcKey.A).Held);
        }
        [Test] public void RepeatedClickInOneFrameDoesNotDuplicateDictionaryKeys()
        {
            Send(keyboard, Key.Space); Send(keyboard); Send(keyboard, Key.Space); Frame();
            Assert.That(gc.Key(GcKey.Space).Down && gc.Key(GcKey.Space).Up && gc.Key(GcKey.Space).Held);
        }
        [Test] public void MultipleKeyboardsHoldUntilLastRelease()
        {
            var second = InputSystem.AddDevice<Keyboard>();
            Send(keyboard, Key.A); Send(second, Key.A); Frame();
            Send(keyboard); Frame(); Assert.That(gc.Key(GcKey.A).Held && !gc.Key(GcKey.A).Up);
            Send(second); Frame(); Assert.That(gc.Key(GcKey.A).Up);
        }
        [Test] public void ResetAndRemovalCancelRatherThanRelease()
        {
            Send(keyboard, Key.Space); Frame(); InputSystem.ResetDevice(keyboard); Frame();
            Assert.That(gc.Key(GcKey.Space).Cancelled && !gc.Key(GcKey.Space).Up && !gc.Key(GcKey.Space).Held);
            var events = gc.KeyEvents;
            Assert.That(events.Count, Is.EqualTo(1));
            Assert.That(events[0].Key, Is.EqualTo(GcKey.Space));
            Assert.That(events[0].Phase, Is.EqualTo(GcKeyEventPhase.Cancelled));
            Frame();
            Assert.That(gc.KeyEvents.Count, Is.Zero);
            Send(keyboard, Key.Space); Frame();
            InputSystem.RemoveDevice(keyboard); Frame();
            Assert.That(gc.Key(GcKey.Space).Cancelled && !gc.Key(GcKey.Space).Up);
        }
        [Test] public void FocusAndPauseRequireFreshPressAndDoNotOverrideEachOther()
        {
            Send(keyboard, Key.Space); Frame(); gc.OnFocus(false); Frame();
            Assert.That(gc.Key(GcKey.Space).Cancelled);
            gc.OnPause(); gc.OnFocus(true); Send(keyboard); Send(keyboard, Key.A); Frame();
            Assert.That(gc.Key(GcKey.A).Down, Is.False);
            gc.OnUnpause(); Send(keyboard); Send(keyboard, Key.A); Frame();
            Assert.That(gc.Key(GcKey.A).Down);
        }
        [Test] public void NoKeyboardThenHotplugWorks()
        {
            UnityEngine.Object.DestroyImmediate(go);
            InputSystem.RemoveDevice(keyboard);
            go = new GameObject("No keyboard", typeof(Camera), typeof(AudioListener), typeof(TestBehaviourBase));
            gc = (GcProxy)go.GetComponent<TestBehaviourBase>().GcProxy;
            Frame();
            var second = InputSystem.AddDevice<Keyboard>(); Send(second, Key.Enter); Frame();
            Assert.That(gc.Key(GcKey.Enter).Down);
            Assert.That(gc.Key((GcKey)(-1)), Is.EqualTo(default(GcKeyState)));
        }
        [Test] public void ShortKeyPressKeepsPrecisionAfterLongUptime()
        {
            currentTime = 86400.01;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Space), time: 86400.001);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(), time: 86400.003);
            InputSystem.Update(); Frame();
            Assert.That(gc.Key(GcKey.Space).Duration, Is.EqualTo(.002).Within(1e-9));
            Assert.That(gc.KeyEvents[1].Time - gc.KeyEvents[0].Time, Is.EqualTo(.002).Within(1e-9));
            Assert.That(gc.KeyEvents[0].Key, Is.EqualTo(GcKey.Space));
        }
        [Test] public void InputEnginesReuseCollectionsAfterWarmup()
        {
            var context = (GcContext)typeof(GcProxy).GetField("m_Context", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(gc);
            var keys = (IEngine)context.InputKey;
            var pointers = (IEngine)context.InputPointer;
            var now = DateTimeOffset.Now;
            Send(keyboard, Key.A);
            for (int i = 0; i < 20; i++) { keys.OnBeforeUpdate(now); pointers.OnBeforeUpdate(now); }
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 1000; i++) { keys.OnBeforeUpdate(now); pointers.OnBeforeUpdate(now); }
            long bytes = GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.That(bytes, Is.Zero);
            Assert.That(gc.Key(GcKey.A).Held);
        }
    }
}
