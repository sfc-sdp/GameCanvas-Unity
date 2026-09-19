#nullable enable
using System;
using System.Collections;
using System.Reflection;
using GameCanvas.Engine;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TestTools;

namespace GameCanvas.Tests
{
    public sealed class DrawingApiTest
    {
        [UnityTest] public IEnumerator StateAnchorsMatchLegacyRenderer()
        {
            var go = new GameObject("Drawing API regression", typeof(Camera), typeof(AudioListener), typeof(TestBehaviourBase));
            RenderTexture? target = null;
            Texture2D? readback = null;
            var previous = RenderTexture.active;
            try
            {
                yield return null;
                var gc = (GcProxy)go.GetComponent<TestBehaviourBase>().GcProxy;
                var context = (GcContext)typeof(GcProxy).GetField("m_Context", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(gc);
                var renderer = context.Graphics;
                var camera = go.GetComponent<Camera>();
                target = new RenderTexture(Screen.width, Screen.height, 24); target.Create(); camera.targetTexture = target;
                readback = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);
                gc.ChangeCanvasSize(720, 1280);
                Assert.That(GcAssets.TryGetImage("BallRed.png", out var image));
                var core = typeof(GcGraphicsEngine).GetMethod("DrawImageCore", BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(GcImage).MakeByRefType(), typeof(GcRect).MakeByRefType() }, null)!;
                var textCore = typeof(GcGraphicsEngine).GetMethod("DrawStringCore", BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(string).MakeByRefType(), typeof(float2).MakeByRefType(), typeof(float) }, null)!;
                Color32[] Capture(bool modern, GcAnchor anchor, int variant = 0)
                {
                    ((IEngine)renderer).OnBeforeUpdate(DateTimeOffset.Now); gc.ClearScreen(); gc.SetColor(255, 255, 255);
                    var rect = GcRect.FromDegrees(350, 400, 120, 90, 30);
                    gc.SetRectAnchor(anchor); gc.SetStringAnchor(anchor);
                    if (modern)
                    {
                        switch (variant)
                        {
                            case 0: gc.DrawImage("BallRed.png", new GcRect(350, 400, 120, 90), rotation: 30); break;
                            case 1: gc.DrawImage(image, 350, 400, 120, 90, rotation: 30); break;
                            default:
                                var original = rect;
                                gc.DrawImage(image, new GcRect(350, 400, 120, 90) { Rotation = 30 });
                                Assert.That(rect, Is.EqualTo(original));
                                break;
                        }
                    }
                    else core.Invoke(renderer, new object[] { image, rect });
                    gc.SetColor(0, 0, 0); gc.SetFontSize(28);
                    const string label = "日本語ABC\n短い行";
                    if (modern)
                    {
                        switch (variant)
                        {
                            case 0: gc.DrawString(label, new GcPoint(350, 700), rotation: 15); break;
                            case 1: gc.DrawString(label, 350, 700, rotation: 15); break;
                            default: gc.DrawString(label, new float2(350, 700), rotation: 15); break;
                        }
                    }
                    else textCore.Invoke(renderer, new object[] { label, new float2(350, 700), 15f });
                    camera.Render(); RenderTexture.active = target;
                    readback.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); readback.Apply();
                    return readback.GetPixels32();
                }
                foreach (GcAnchor anchor in Enum.GetValues(typeof(GcAnchor)))
                {
                    // Warm the dynamic font atlas before comparing the same content.
                    Capture(false, anchor); Capture(true, anchor);
                    var expected = Capture(false, anchor);
                    for (int variant = 0; variant < 3; variant++)
                    {
                        var actual = Capture(true, anchor, variant);
                        int redPixels = 0;
                        foreach (var pixel in actual) if (pixel.r > 150 && pixel.g < 120 && pixel.b < 120) redPixels++;
                        Assert.That(redPixels, Is.GreaterThan(10), "比較画面に赤い画像が描かれていること");
                        CollectionAssert.AreEqual(expected, actual, $"{anchor}, variant={variant}");
                    }
                }
                gc.SetRectAnchor(GcAnchor.MiddleCenter); gc.SetStringAnchor(GcAnchor.LowerRight);
                var outer = gc.CurrentStyle;
                using (gc.StyleScope)
                {
                    gc.SetRectAnchor(GcAnchor.LowerLeft); gc.SetStringAnchor(GcAnchor.UpperCenter);
                    gc.SetColor(12, 34, 56); gc.SetFontSize(16);
                    using (gc.StyleScope)
                    {
                        gc.SetRectAnchor(GcAnchor.LowerRight); gc.SetStringAnchor(GcAnchor.MiddleLeft);
                        gc.DrawImage("BallRed.png", new GcPoint(350, 400));
                        gc.DrawString("内側", 350, 700);
                    }
                    Assert.That(renderer.RectAnchor, Is.EqualTo(GcAnchor.LowerLeft));
                    Assert.That(renderer.StringAnchor, Is.EqualTo(GcAnchor.UpperCenter));
                }
                Assert.That(gc.CurrentStyle, Is.EqualTo(outer));
                LogAssert.Expect(LogType.Warning, "[GameCanvas] GC_ASSET_MISSING: 'anchor-missing.png'. Add this image to Assets/Res and check the asset catalog.");
                gc.DrawImage("anchor-missing.png", new GcRect(300, 400, 90, 120));
                Assert.That(gc.CurrentStyle, Is.EqualTo(outer), "Missing-image diagnostics must restore style");
                gc.DrawImage("BallRed.png", new GcPoint(0, 0)); gc.DrawImage(image, new GcPoint(0, 0)); gc.DrawString("状態の基準点", 0, 0);
                Assert.That(renderer.RectAnchor, Is.EqualTo(GcAnchor.MiddleCenter));
                Assert.That(renderer.StringAnchor, Is.EqualTo(GcAnchor.LowerRight));
                int component = 128; gc.SetColor(component, 64, 32);
                Assert.That(renderer.Color.r, Is.EqualTo(128 / 255f));
                gc.SetColor(GcColor.FromNormalized(.5f, .25f, .125f));
                Assert.That(renderer.Color.r, Is.EqualTo(.5f));
            }
            finally
            {
                RenderTexture.active = previous;
                UnityEngine.Object.DestroyImmediate(go);
                if (target != null) UnityEngine.Object.DestroyImmediate(target);
                if (readback != null) UnityEngine.Object.DestroyImmediate(readback);
            }
        }
    }
}
