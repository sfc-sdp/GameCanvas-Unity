/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2026 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
#nullable enable
using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace GameCanvas.Engine
{
    // One native stream per service. Mobile Unity does not support simultaneous WebCamTextures.
    internal sealed class GcInputCameraEngine : IGcCameraBackend
    {
        WebCamTexture? texture;
        public bool Authorized =>
#if UNITY_ANDROID && !UNITY_EDITOR
            UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera);
#elif UNITY_IOS || UNITY_WEBGL || UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            Application.HasUserAuthorization(UserAuthorization.WebCam);
#else
            true;
#endif
        public Texture? Texture => texture;
        public bool IsPlaying => texture != null && texture.isPlaying;
        public IEnumerator RequestPermission()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return GcAndroidPermission.Request(new[] { UnityEngine.Android.Permission.Camera });
#else
            return RequestUnityPermission();
#endif
        }
        static IEnumerator RequestUnityPermission()
        {
            var request = Application.RequestUserAuthorization(UserAuthorization.WebCam);
            while (!request.isDone) yield return null;
        }
        public GcCameraDevice[] GetDevices()
        {
            var devices = WebCamTexture.devices;
            var result = new GcCameraDevice[devices.Length];
            var count = 0;
            for (var i = 0; i < devices.Length; i++)
            {
                var device = devices[i];
                if (string.IsNullOrEmpty(device.name) || device.name == device.depthCameraName) continue;
                var resolutions = device.availableResolutions;
                var values = new GcResolution[resolutions?.Length ?? 0];
                for (var j = 0; j < values.Length; j++) values[j] = (GcResolution)resolutions![j];
                result[count++] = new GcCameraDevice(device.name, false, values, device.isFrontFacing, device.isAutoFocusPointSupported);
            }
            if (count != result.Length) Array.Resize(ref result, count);
            return result;
        }
        public void Play(GcCameraDevice device, int width, int height, int fps)
        {
            texture = new WebCamTexture(device.DeviceName, width, height, fps);
            texture.Play();
        }
        public bool TryReadFrame(out GcCameraFrame frame)
        {
            frame = default;
            if (texture == null || !texture.didUpdateThisFrame || texture.width <= 16 || texture.height <= 16) return false;
            frame = new GcCameraFrame(texture.width, texture.height, texture.videoRotationAngle, texture.videoVerticallyMirrored);
            return true;
        }
        public void Focus(float2? point) { if (texture != null) texture.autoFocusPoint = point; }
        public void Dispose()
        {
            var previous = texture;
            texture = null;
            if (previous == null) return;
            try { previous.Stop(); }
            finally { UnityEngine.Object.Destroy(previous); }
        }

        internal static float2x3 CalcCameraMatrix(float2 size, float rotation, bool mirrored, GcAnchor anchor)
        {
            var mtx = GcAffine.FromScale(size);
            var offset = GcGraphicsEngine.GetOffset(anchor) - GcGraphicsEngine.GetOffset(GcAnchor.MiddleCenter);

            if (mirrored)
            {
                var t = size * offset;
                mtx = GcAffine.FromTranslate(t)
                    .Mul(GcAffine.FromScale(new float2(1f, -1f)))
                    .Mul(GcAffine.FromTranslate(-t))
                    .Mul(mtx);
            }

            var deg = GcMath.Repeat(rotation, 360f);
            if (GcMath.AlmostSame(deg, 90f) || GcMath.AlmostSame(deg, 270f))
            {
                mtx = GcAffine.FromTranslate(new float2(size.y, size.x) * offset)
                    .Mul(GcAffine.FromRotate(math.radians(deg)))
                    .Mul(GcAffine.FromTranslate(size * -offset))
                    .Mul(mtx);
            }
            else if (GcMath.AlmostSame(deg, 180f))
            {
                var t = size * offset;
                mtx = GcAffine.FromTranslate(t)
                    .Mul(GcAffine.FromRotate(math.radians(deg)))
                    .Mul(GcAffine.FromTranslate(-t))
                    .Mul(mtx);
            }

            return mtx;
        }

    }
}
