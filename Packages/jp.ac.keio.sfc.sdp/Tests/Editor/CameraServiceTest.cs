#nullable enable
using System;
using System.Collections;
using GameCanvas.Engine;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

namespace GameCanvas.Editor.Tests
{
    public sealed class CameraServiceTest
    {
        static GcCameraDevice Device(string name, bool front = false) => new(name, false, Array.Empty<GcResolution>(), front, true);
        sealed class Backend : IGcCameraBackend
        {
            public bool Authorized { get; set; } = true;
            public bool PermissionPending, ThrowOnRead, ThrowOnPlay, Disposed, PermissionDisposed;
            public int Starts, Reads;
            public string? Selected;
            public int2 RequestedSize;
            public bool IsPlaying { get; set; }
            public GcCameraDevice[] Devices = { Device("front", true), Device("rear") };
            public GcCameraFrame? Next;
            public float2? FocusPoint;
            public Texture? Texture => null;
            public IEnumerator RequestPermission()
            {
                try { while (PermissionPending) yield return null; }
                finally { PermissionDisposed = true; }
            }
            public GcCameraDevice[] GetDevices() => Devices;
            public void Play(GcCameraDevice device, int width, int height, int fps)
            {
                if (ThrowOnPlay) throw new InvalidOperationException();
                Starts++; Selected = device.DeviceName; RequestedSize = new int2(width, height); IsPlaying = true;
            }
            public bool TryReadFrame(out GcCameraFrame frame)
            {
                Reads++;
                if (ThrowOnRead) throw new InvalidOperationException();
                frame = Next ?? default;
                bool updated = Next.HasValue; Next = null; return updated;
            }
            public void Focus(float2? point) => FocusPoint = point;
            public void Dispose() { Disposed = true; IsPlaying = false; }
        }
        [Test] public void FirstFrameDefinesRunningAndSize_StopClearsEverything_ThenRestartUsesFreshBackend()
        {
            Backend? current = null;
            var service = new GcCameraService(() => current = new Backend(), () => 0);
            Assert.That(service.Status, Is.EqualTo(GcCameraState.Idle));
            service.Start(width: 1280, height: 720);
            Assert.That(service.Status, Is.EqualTo(GcCameraState.RequestingPermission));
            service.Tick();
            Assert.That(service.Status, Is.EqualTo(GcCameraState.Waiting));
            Assert.That(service.Width, Is.Zero);
            Assert.That(current!.Selected, Is.EqualTo("rear"));
            Assert.That(current.RequestedSize, Is.EqualTo(new int2(1280, 720)));
            current.Next = new GcCameraFrame(640, 480, 90, true); service.Tick();
            Assert.That(service.Status, Is.EqualTo(GcCameraState.Running));
            Assert.That(service.Width, Is.EqualTo(480)); Assert.That(service.Height, Is.EqualTo(640));
            Assert.That(service.Updated, Is.True); Assert.That(service.IsMirrored, Is.True);
            service.Tick(); Assert.That(service.Updated, Is.False); Assert.That(service.Width, Is.EqualTo(480));
            current.Next = new GcCameraFrame(640, 480, 180, false); service.Tick();
            Assert.That(service.Width, Is.EqualTo(640)); Assert.That(service.Rotation, Is.EqualTo(180));
            Assert.That(service.Focus(.25f, .75f), Is.True);
            Assert.That(current.FocusPoint, Is.EqualTo(new float2(.25f, .75f)));
            Assert.That(service.ResetFocus(), Is.True); Assert.That(current.FocusPoint, Is.Null);
            var first = current;
            service.Stop();
            Assert.That(first.Disposed, Is.True); Assert.That(service.Device, Is.Null);
            Assert.That(service.Width, Is.Zero); Assert.That(service.Height, Is.Zero);
            Assert.That(service.Updated, Is.False); Assert.That(service.Rotation, Is.Zero);
            Assert.That(service.Focus(.5f, .5f), Is.False);
            service.Start(GcCameraFacing.Front); service.Tick();
            Assert.That(current, Is.Not.SameAs(first)); Assert.That(current!.Selected, Is.EqualTo("front"));
            Assert.That(service.Status, Is.EqualTo(GcCameraState.Waiting));
            service.Stop();
        }
        [Test] public void StopDuringPermissionWait_DisposesRequest_AndLateGrantCannotRestart()
        {
            var backend = new Backend { Authorized = false, PermissionPending = true };
            var service = new GcCameraService(() => backend, () => 0);
            service.Start(); service.Tick(); service.Stop();
            Assert.That(backend.PermissionDisposed, Is.True); Assert.That(backend.Disposed, Is.True);
            backend.Authorized = true; backend.PermissionPending = false;
            service.Tick(); service.Tick();
            Assert.That(backend.Starts, Is.Zero); Assert.That(service.Status, Is.EqualTo(GcCameraState.Stopped));
        }
        [Test] public void PermissionResultWaitsOneMoreFrame_AndDeniedNeverEnumeratesOrPlays()
        {
            var backend = new Backend { Authorized = false, PermissionPending = true };
            var service = new GcCameraService(() => backend, () => 0);
            service.Start(); service.Tick();
            backend.Authorized = true; backend.PermissionPending = false;
            service.Tick(); Assert.That(backend.Starts, Is.Zero);
            service.Tick(); Assert.That(backend.Starts, Is.EqualTo(1)); service.Stop();
            var denied = new Backend { Authorized = false };
            service = new GcCameraService(() => denied, () => 0);
            service.Start(); service.Tick(); service.Tick();
            Assert.That(service.Status, Is.EqualTo(GcCameraState.NotGranted));
            Assert.That(service.Permission, Is.EqualTo(GcCameraPermission.NotGranted));
            Assert.That(service.Devices.Count, Is.Zero); Assert.That(denied.Starts, Is.Zero); Assert.That(denied.Disposed, Is.True);
        }
        [TestCase(true)] [TestCase(false)] public void PermissionOrInitialFrameTimeoutReleasesBackend(bool permission)
        {
            double time = 0;
            var backend = new Backend { Authorized = !permission, PermissionPending = permission };
            var service = new GcCameraService(() => backend, () => time);
            service.Start(timeoutSeconds: 3); service.Tick(); time = 3; service.Tick();
            Assert.That(service.Status, Is.EqualTo(GcCameraState.TimedOut)); Assert.That(backend.Disposed, Is.True);
            Assert.That(service.Width, Is.Zero); Assert.That(service.ErrorCode, Is.EqualTo("GC-CAMERA-TIMEOUT"));
        }
        [Test] public void PauseCancelsPendingAndRunning_StartWhilePausedCannotActivate_ResumeNeedsStart()
        {
            int created = 0;
            Backend? backend = null;
            var service = new GcCameraService(() => { created++; return backend = new Backend(); }, () => 0);
            service.Start(); service.Tick(); backend!.Next = new GcCameraFrame(640, 480, 0, false); service.Tick();
            service.SetPaused(true);
            Assert.That(backend.Disposed, Is.True); Assert.That(service.Width, Is.Zero);
            service.Start(); Assert.That(created, Is.EqualTo(1));
            service.SetPaused(false); service.Tick(); Assert.That(created, Is.EqualTo(1));
            Assert.That(service.Status, Is.EqualTo(GcCameraState.Stopped));
            service.Start(); Assert.That(created, Is.EqualTo(2)); service.Stop();
        }
        [Test] public void SelectionHasStrictFrontAndRear_AnyFallsBack_StaleDeviceFails()
        {
            var front = Device("only front", true);
            Backend? backend = null;
            var service = new GcCameraService(() => backend = new Backend { Devices = new[] { front } }, () => 0);
            service.Start(GcCameraFacing.Rear); service.Tick(); Assert.That(service.Status, Is.EqualTo(GcCameraState.NoDevice));
            service.Start(); service.Tick(); Assert.That(service.Device, Is.SameAs(front));
            service.Start(front); service.Tick(); Assert.That(service.Device, Is.SameAs(front));
            service.Start(Device("unplugged")); service.Tick(); Assert.That(service.Status, Is.EqualTo(GcCameraState.NoDevice));
            Assert.That(backend!.Disposed, Is.True);
            service = new GcCameraService(() => new Backend { Devices = Array.Empty<GcCameraDevice>() });
            service.Start(); service.Tick(); Assert.That(service.Status, Is.EqualTo(GcCameraState.NoDevice));
        }
        [Test] public void RevokedPermissionAndStoppedStreamInvalidateRunningFrame()
        {
            foreach (bool revoke in new[] { true, false })
            {
                var backend = new Backend(); var service = new GcCameraService(() => backend, () => 0);
                service.Start(); service.Tick(); backend.Next = new GcCameraFrame(640, 480, 0, false); service.Tick();
                if (revoke) backend.Authorized = false; else backend.IsPlaying = false;
                service.Tick(); Assert.That(service.Status, Is.EqualTo(revoke ? GcCameraState.NotGranted : GcCameraState.Failed));
                Assert.That(service.Width, Is.Zero); Assert.That(backend.Disposed, Is.True);
            }
        }
        [Test] public void NativeFailuresAreTerminalAndCleanedUp()
        {
            foreach (bool start in new[] { true, false })
            {
                var backend = new Backend { ThrowOnPlay = start, ThrowOnRead = !start };
                var service = new GcCameraService(() => backend, () => 0);
                service.Start(); service.Tick(); service.Tick();
                Assert.That(service.Status, Is.EqualTo(GcCameraState.Failed)); Assert.That(backend.Disposed, Is.True);
                Assert.That(service.ErrorCode, Is.EqualTo("GC-CAMERA-NATIVE"));
            }
        }
        [Test] public void InvalidArgumentsDoNotInterruptRunningOperation_UnsupportedDoesNotCreateBackend()
        {
            var backend = new Backend(); var service = new GcCameraService(() => backend);
            service.Start(); service.Tick();
            Assert.Throws<ArgumentOutOfRangeException>(() => service.Start(width: 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => service.Start(height: -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => service.Start(fps: 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => service.Start(timeoutSeconds: double.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => service.Start(timeoutSeconds: double.PositiveInfinity));
            Assert.Throws<ArgumentOutOfRangeException>(() => service.Start((GcCameraFacing)99));
            Assert.Throws<ArgumentNullException>(() => service.Start((GcCameraDevice)null!));
            Assert.That(backend.Disposed, Is.False); Assert.That(backend.Starts, Is.EqualTo(1));
            Assert.Throws<ArgumentOutOfRangeException>(() => service.Focus(float.NaN, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => service.Focus(0, float.PositiveInfinity));
            service.Stop();
            service = new GcCameraService(() => throw new Exception("must not create"), supported: false);
            service.Start(); Assert.That(service.Status, Is.EqualTo(GcCameraState.Unsupported));
        }
        [Test] public void WarmPollingDoesNotAllocateOrEnumerateAgain()
        {
            var backend = new Backend(); var service = new GcCameraService(() => backend, () => 0);
            service.Start(); service.Tick(); backend.Next = new GcCameraFrame(640, 480, 0, false); service.Tick();
            var devices = service.Devices;
            for (int i = 0; i < 10; i++) service.Tick();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 1000; i++) { service.Tick(); _ = service.Width; _ = service.Devices[0]; }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.That(allocated, Is.Zero); Assert.That(service.Devices, Is.SameAs(devices)); service.Stop();
        }
        [Test] public void CameraTransformRespectsDestinationBounds_ForEveryAnchorRotationAndMirror()
        {
            foreach (GcAnchor anchor in Enum.GetValues(typeof(GcAnchor)))
            foreach (int rotation in new[] { 0, 90, 180, 270 })
            foreach (bool mirror in new[] { false, true })
            {
                var width = rotation % 180 == 0 ? 640 : 480;
                var height = rotation % 180 == 0 ? 480 : 640;
                var matrix = GcInputCameraEngine.CalcCameraMatrix(new float2(640, 480), rotation, mirror, anchor);
                matrix = GcAffine.FromTRS(new float2(300, 400), 0, new float2(120f / width, 90f / height)).Mul(matrix);
                matrix = matrix.Mul(GcAffine.FromTranslate(GcGraphicsEngine.GetOffset(anchor)));
                var min = new float2(float.PositiveInfinity); var max = new float2(float.NegativeInfinity);
                foreach (var corner in new[] { float2.zero, new float2(1, 0), new float2(0, 1), new float2(1, 1) })
                { var point = matrix.Mul(corner); min = math.min(min, point); max = math.max(max, point); }
                var expected = new float2(300, 400) + GcGraphicsEngine.GetOffset(anchor) * new float2(120, 90);
                Assert.That(math.distance(min, expected), Is.LessThan(.001f), $"{anchor}/{rotation}/{mirror} min");
                Assert.That(math.distance(max - min, new float2(120, 90)), Is.LessThan(.001f), $"{anchor}/{rotation}/{mirror} size");
            }
        }
    }
}
