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
    public sealed class CameraDrawingTest
    {
        [UnityTest] public IEnumerator CameraMatchesTextureDrawingAndProxyStopsItOnPauseAndDisable()
        {
            var go = new GameObject("Camera drawing regression", typeof(Camera), typeof(AudioListener), typeof(TestBehaviourBase));
            GcProxy? gc = null;
            RenderTexture? target = null;
            Texture2D? readback = null;
            var texture = new Texture2D(32, 24, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var pixels = new Color32[32 * 24];
            for (int y = 0; y < 24; y++) for (int x = 0; x < 32; x++)
                pixels[y * 32 + x] = x < 16 ? new Color32(255, 30, 0, 255) : new Color32(0, (byte)(y < 12 ? 255 : 0), 255, 255);
            texture.SetPixels32(pixels); texture.Apply();
            var previous = RenderTexture.active;
            int created = 0;
            CameraDrawingBackend? backend = null;
            try
            {
                yield return null;
                var service = new GcCameraService(() => { created++; return backend = new CameraDrawingBackend(texture); });
                gc = new GcProxy(go.GetComponent<TestBehaviourBase>(), service); gc.OnEnable();
                var context = (GcContext)typeof(GcProxy).GetField("m_Context", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(gc);
                var renderer = context.Graphics;
                var camera = go.GetComponent<Camera>();
                target = new RenderTexture(320, 320, 24); target.Create(); camera.targetTexture = target;
                readback = new Texture2D(320, 320, TextureFormat.RGBA32, false);
                gc.ChangeCanvasSize(320, 320);
                gc.DrawCamera(); gc.DrawCamera(new GcPoint(100, 100)); gc.DrawCamera(new float2(100, 100)); gc.DrawCamera(100, 100); gc.DrawCamera(100, 100, 64, 48);
                Assert.That(created, Is.Zero, "Drawing must not initialize a camera or request permission");
                Color32[] Capture(bool modern, GcAnchor anchor, bool scaled)
                {
                    ((IEngine)renderer).OnBeforeUpdate(DateTimeOffset.Now);
                    gc.ClearScreen(); gc.SetColor(255, 255, 255); gc.SetRectAnchor(anchor);
                    if (modern)
                    {
                        if (scaled) gc.DrawCamera(160, 160, 64, 48, rotation: 30);
                        else gc.DrawCamera(160, 160, rotation: 30);
                    }
                    else
                    {
                        var rect = new GcRect(160, 160, scaled ? 64 : 32, scaled ? 48 : 24) { Rotation = 30 };
                        renderer.DrawTexture(texture, rect);
                    }
                    camera.Render(); RenderTexture.active = target;
                    readback.ReadPixels(new Rect(0, 0, 320, 320), 0, 0); readback.Apply();
                    return readback.GetPixels32();
                }
                service.Start(); gc.OnBeforeUpdate(DateTimeOffset.Now); gc.OnBeforeUpdate(DateTimeOffset.Now);
                Assert.That(service.Status, Is.EqualTo(GcCameraState.Running));
                foreach (GcAnchor anchor in Enum.GetValues(typeof(GcAnchor)))
                foreach (bool scaled in new[] { false, true })
                {
                    Capture(false, anchor, scaled); Capture(true, anchor, scaled);
                    var expected = Capture(false, anchor, scaled);
                    var actual = Capture(true, anchor, scaled);
                    CollectionAssert.AreEqual(expected, actual, $"{anchor}/scaled={scaled}");
                    Assert.That(Array.Exists(actual, p => p.r == 255 && p.g == 30 && p.b == 0), Is.True);
                }
                gc.OnPause();
                Assert.That(backend!.Disposed, Is.True); Assert.That(service.Status, Is.EqualTo(GcCameraState.Stopped));
                gc.OnUnpause(); gc.OnBeforeUpdate(DateTimeOffset.Now);
                Assert.That(created, Is.EqualTo(1)); Assert.That(service.Status, Is.EqualTo(GcCameraState.Stopped));
                service.Start(); gc.OnBeforeUpdate(DateTimeOffset.Now); gc.OnBeforeUpdate(DateTimeOffset.Now);
                Assert.That(created, Is.EqualTo(2)); Assert.That(service.Status, Is.EqualTo(GcCameraState.Running));
                gc.OnDisable();
                Assert.That(backend!.Disposed, Is.True); Assert.That(service.Width, Is.Zero);
                gc.OnEnable(); Assert.That(created, Is.EqualTo(2));
            }
            finally
            {
                RenderTexture.active = previous;
                gc?.OnDisable();
                UnityEngine.Object.DestroyImmediate(go);
                UnityEngine.Object.DestroyImmediate(texture);
                if (target != null) UnityEngine.Object.DestroyImmediate(target);
                if (readback != null) UnityEngine.Object.DestroyImmediate(readback);
            }
        }
    }
    sealed class CameraDrawingBackend : IGcCameraBackend
    {
        readonly Texture texture;
        internal bool Disposed;
        internal CameraDrawingBackend(Texture texture) { this.texture = texture; }
        public bool Authorized => true;
        public IEnumerator RequestPermission() { yield break; }
        public GcCameraDevice[] GetDevices() => new[] { new GcCameraDevice("test camera", false, Array.Empty<GcResolution>(), false, false) };
        public void Play(GcCameraDevice device, int width, int height, int fps) { IsPlaying = true; }
        public bool IsPlaying { get; private set; }
        public bool TryReadFrame(out GcCameraFrame frame) { frame = new GcCameraFrame(32, 24, 0, false); return true; }
        public Texture? Texture => texture;
        public void Focus(float2? point) { }
        public void Dispose() { Disposed = true; IsPlaying = false; }
    }
}
