#nullable enable
using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Unity.Mathematics;

namespace GameCanvas.Editor.Tests
{
    public class InputApiContractTest
    {
        [Test] public void LegacyInputSurfaceAndTracesAreRemoved()
        {
            var names = typeof(GcProxy).GetMembers().Select(m => m.Name).ToArray();
            foreach (var name in new[] { "IsKeyDown", "IsKeyHold", "IsKeyPress", "IsKeyUp", "IsAnyKey",
                "IsAnyKeyDown", "IsAnyKeyHold", "IsAnyKeyPress", "IsAnyKeyUp", "KeyEscape",
                "KeyDownCount", "KeyHoldCount", "KeyPressCount", "KeyUpCount", "GetKeyPressDuration",
                "GetKeyPressFrameCount", "TryGetKeyEvent", "TryGetKeyEventAll", "TryGetKeyTrace", "TryGetKeyTraceAll",
                "PointerCount", "PointerBeginCount", "PointerEndCount", "PointerTapCount", "LastPointerEvent",
                "LastPointerFrame", "LastPointerPoint", "LastPointerTime", "LastPointerX", "LastPointerY",
                "TryGetPointerEvent", "TryGetPointerEventAll", "TryGetPointerTrace", "TryGetPointerTraceAll",
                "TryGetPointerTapPoint", "TryGetPointerTapPointAll", "IsTapped", "IsTouchBegan", "IsTouched", "IsTouchEnded" })
                Assert.That(names, Does.Not.Contain(name));
            var assembly = typeof(GcProxy).Assembly;
            foreach (var name in new[] { "GcKeyTrace", "GcPointerTrace", "IInputKeyEx", "IInputPointerEx" })
                Assert.That(assembly.GetType("GameCanvas." + name), Is.Null);
            Assert.That(typeof(GcKeyEvent).GetField("Key")!.FieldType, Is.EqualTo(typeof(GcKey)));
            Assert.That(Enum.GetNames(typeof(GcKeyEventPhase)), Is.EquivalentTo(new[] { "Down", "Up", "Cancelled" }));
        }


        [Test] public void DurationsAndInputTimesUseDoubleSeconds()
        {
            Assert.That(typeof(GcKeyState).GetProperty("Duration")!.PropertyType, Is.EqualTo(typeof(double)));
            Assert.That(typeof(GcPointer).GetProperty("Duration")!.PropertyType, Is.EqualTo(typeof(double)));
            Assert.That(typeof(GcKeyEvent).GetField("Time")!.FieldType, Is.EqualTo(typeof(double)));
            Assert.That(typeof(GcPointerEvent).GetField("Time")!.FieldType, Is.EqualTo(typeof(double)));
            Assert.That(typeof(ITime).GetProperty("TimeSinceStartup")!.PropertyType, Is.EqualTo(typeof(double)));
        }

    }
}
