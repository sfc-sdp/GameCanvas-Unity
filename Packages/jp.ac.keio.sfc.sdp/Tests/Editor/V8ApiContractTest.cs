#nullable enable
using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace GameCanvas.Editor.Tests
{
    public class V8ApiContractTest
    {
        [Test] public void DeprecatedDeclarationsAndAmbiguousColorUnitsAreRemoved()
        {
            foreach (var type in typeof(GcProxy).Assembly.GetExportedTypes())
            {
                Assert.That(type.IsDefined(typeof(ObsoleteAttribute), false), Is.False, type.FullName);
                foreach (var member in type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                    Assert.That(member.IsDefined(typeof(ObsoleteAttribute), false), Is.False, type.FullName + "." + member.Name);
            }
            foreach (var method in typeof(GcProxy).GetMethods())
                if (method.Name == "SetColor")
                {
                    var parameters = method.GetParameters();
                    if (parameters.Length >= 3) Assert.That(parameters[0].ParameterType, Is.EqualTo(typeof(int)));
                }
        }
        [Test] public void DrawingAnchorsBelongToStyleRatherThanCallArguments()
        {
            foreach (var type in new[] { typeof(GcProxy), typeof(IGraphics), typeof(IGraphicsEx) })
                foreach (var method in type.GetMethods())
                    if (method.Name == "DrawImage" || method.Name == "DrawString")
                        foreach (var parameter in method.GetParameters())
                            Assert.That(parameter.Name, Is.Not.EqualTo("anchor"), type.Name + "." + method.Name);
        }
        [Test] public void DrawingACameraDoesNotStartItByDefault()
        {
            foreach (var type in new[] { typeof(GcProxy), typeof(IInputCamera) })
                foreach (var method in type.GetMethods())
                    if (method.Name == "DrawCamera")
                        foreach (var parameter in method.GetParameters())
                            Assert.That(parameter.Name, Is.Not.EqualTo("autoPlay"));
        }
        [Test] public void ColorsUseExplicitUnitsAndClampAtTheirBoundaries()
        {
            Assert.That(new GcColor(128, 0, 255).R, Is.EqualTo(128 / 255f));
            Assert.That(new GcColor(-1, 300, 0).G, Is.EqualTo(1));
            Assert.That(GcColor.FromNormalized(.5f, .1f, 3).B, Is.EqualTo(1));
            Assert.Throws<ArgumentOutOfRangeException>(() => GcColor.FromNormalized(float.NaN, 0, 0));
        }
        [Test] public void WarmImagePathsAllocateNoManagedMemoryAndResetInvalidatesThem()
        {
            GcAssets.Reset(); Assert.That(GcAssets.TryGetImage("BlueSky.png", out var image));
            for (int i = 0; i < 10; i++) GcAssets.TryGetImage("BlueSky.png", out _);
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 1000; i++) GcAssets.TryGetImage("BlueSky.png", out _);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.That(allocated, Is.Zero);
            GcAssets.Reset(); Assert.That(GcAssets.TryGetImage("BlueSky.png", out var again)); Assert.That(again, Is.EqualTo(image));
        }
        [Test] public void MissingImagesAreDiagnosedOnceUntilCatalogReset()
        {
            GcAssets.Reset();
            const string message = "[GameCanvas] GC_ASSET_MISSING: 'absent.png'. Add this image to Assets/Res and check the asset catalog.";
            LogAssert.Expect(LogType.Warning, message);
            GcAssets.ReportMissingImage("absent.png"); GcAssets.ReportMissingImage("absent.png");
            LogAssert.NoUnexpectedReceived(); GcAssets.Reset();
            LogAssert.Expect(LogType.Warning, message); GcAssets.ReportMissingImage("absent.png"); GcAssets.Reset();
        }
    }
}
