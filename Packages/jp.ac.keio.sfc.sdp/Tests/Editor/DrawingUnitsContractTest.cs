#nullable enable
using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Unity.Mathematics;

namespace GameCanvas.Editor.Tests
{
    public class DrawingUnitsContractTest
    {
        [Test] public void RectRotationIsDegreesWithoutReinterpretingOldConstructorArguments()
        {
            Assert.That(typeof(GcRect).GetField("Radian"), Is.Null);
            Assert.That(typeof(GcRect).Assembly.GetType("GameCanvas.GcRectExtensions"), Is.Null);
            foreach (var constructor in typeof(GcRect).GetConstructors())
                foreach (var parameter in constructor.GetParameters())
                    Assert.That(parameter.Name, Is.Not.EqualTo("radian"));
            Assert.That(typeof(GcRect).GetConstructors().Any(c => c.GetParameters().Length == 5), Is.False);
            var rect = new GcRect(10, 20, 30, 40) { Rotation = 90 };
            Assert.That(rect, Is.EqualTo(GcRect.FromDegrees(10, 20, 30, 40, 90)));
            Assert.That(rect.Rotation, Is.EqualTo(90).Within(.00001));
            foreach (var type in new[] { typeof(GcProxy), typeof(IGraphics), typeof(IGraphicsEx), typeof(INetworkEx), typeof(IInputCameraEx) })
                foreach (var method in type.GetMethods())
                    if (method.Name.StartsWith("Draw") || method.Name.StartsWith("Fill") || method.Name == "RotateCoordinate")
                        foreach (var parameter in method.GetParameters())
                            Assert.That(parameter.Name, Is.Not.EqualTo("degree"), type.Name + "." + method.Name);
        }

    }
}
