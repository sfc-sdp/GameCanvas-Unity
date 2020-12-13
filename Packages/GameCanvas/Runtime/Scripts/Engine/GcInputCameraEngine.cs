/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using Coroutine = System.Collections.IEnumerator;

namespace GameCanvas.Engine
{
    sealed class GcInputCameraEngine : IInputCamera, IEngine
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        readonly GcContext m_Context;
        readonly List<GcCameraDevice> m_DeviceList;
        readonly Dictionary<string, WebCamTexture> m_TextureDict;
        ReadOnlyCollection<GcCameraDevice> m_DeviceListReadOnly;
        bool m_IsDeviceListInitialized;
        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        public int CameraDeviceCount
        {
            get
            {
                InitCameraDevice();
                return m_DeviceList.Count;
            }
        }

        public bool HasUserAuthorizedPermissionCamera
#if UNITY_EDITOR
            => true;
#elif UNITY_ANDROID
            =>  UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera);
#elif UNITY_IOS
            =>  Application.HasUserAuthorization(UserAuthorization.WebCam);
#endif // UNITY_EDITOR

        public bool DidUpdateCameraImageThisFrame(in GcCameraDevice camera)
        {
            if (m_TextureDict.TryGetValue(camera.DeviceName, out var texture))
            {
                return texture.didUpdateThisFrame;
            }
            return false;
        }

        public void FocusCameraImage(in GcCameraDevice camera, in float2? uv)
        {
            if (m_TextureDict.TryGetValue(camera.DeviceName, out var texture))
            {
                texture.autoFocusPoint = uv;
            }
        }

        public WebCamTexture GetOrCreateCameraTexture(in GcCameraDevice camera, in GcResolution request)
        {
            if (string.IsNullOrEmpty(camera.DeviceName)) return null;

            if (m_TextureDict.TryGetValue(camera.DeviceName, out var texture))
            {
                return texture;
            }

            if (!HasUserAuthorizedPermissionCamera)
            {
                return null;
            }

            texture = new WebCamTexture(camera.DeviceName, request.Size.x, request.Size.y, request.RefreshRate);
            if (texture != null)
            {
                m_TextureDict.Add(camera.DeviceName, texture);
            }
            return texture;
        }

        public bool IsFlippedCameraImage(in GcCameraDevice camera)
        {
            if (m_TextureDict.TryGetValue(camera.DeviceName, out var texture))
            {
                return texture.videoVerticallyMirrored;
            }
            return false;
        }

        public bool IsPlayingCameraImage(in GcCameraDevice camera)
        {
            if (m_TextureDict.TryGetValue(camera.DeviceName, out var texture))
            {
                return texture.isPlaying;
            }
            return false;
        }

        public bool PauseCameraImage(in GcCameraDevice camera)
        {
            if (m_TextureDict.TryGetValue(camera.DeviceName, out var texture))
            {
                if (texture.isPlaying)
                {
                    texture.Pause();
                    return !texture.isPlaying;
                }
            }
            return false;
        }

        public bool PlayCameraImage(in GcCameraDevice camera, in GcResolution request, out int2 resolution)
        {
            if (string.IsNullOrEmpty(camera.DeviceName))
            {
                resolution = default;
                return false;
            }

            var texture = GetOrCreateCameraTexture(camera, request);
            if (texture != null)
            {
                if (!texture.isPlaying)
                {
                    texture.Play();
                }
                resolution = GetRotatedCameraSize(texture);
                return texture.isPlaying;
            }
            resolution = default;
            return false;
        }

        public void RequestUserAuthorizedPermissionCameraAsync(in System.Action<bool> callback)
        {
            var coroutine = RequestUserAuthorizedPermissionCoroutine(callback);
            m_Context.Behaviour.StartCoroutine(coroutine);
        }

        public void StopCameraImage(in GcCameraDevice camera)
        {
            if (m_TextureDict.TryGetValue(camera.DeviceName, out var texture))
            {
                texture.Stop();
            }
        }

        public int2 TryChangeCameraImageResolution(in GcCameraDevice camera, in GcResolution request)
        {
            if (string.IsNullOrEmpty(camera.DeviceName)) return int2.zero;

            if (m_TextureDict.TryGetValue(camera.DeviceName, out var texture))
            {
                m_TextureDict.Remove(camera.DeviceName);
                texture.Stop();
                Object.Destroy(texture);
            }

            texture = GetOrCreateCameraTexture(camera, request);
            if (texture != null)
            {
                return GetRotatedCameraSize(texture);
            }
            return int2.zero;
        }

        public bool TryGetCameraImage(out GcCameraDevice camera)
        {
            InitCameraDevice();
            if (m_DeviceList.Count > 0)
            {
                camera = m_DeviceList[0];
                return true;
            }
            camera = null;
            return false;
        }

        public bool TryGetCameraImage(in string deviceName, out GcCameraDevice camera)
        {
            InitCameraDevice();
            for (var i = 0; i < m_DeviceList.Count; i++)
            {
                camera = m_DeviceList[i];
                if (camera.DeviceName == deviceName) return true;
            }
            camera = null;
            return false;
        }

        public bool TryGetCameraImageAll(out ReadOnlyCollection<GcCameraDevice> array)
        {
            InitCameraDevice();
            if (m_DeviceListReadOnly != null)
            {
                array = m_DeviceListReadOnly;
                return true;
            }
            array = null;
            return false;
        }

        public bool TryGetCameraImageRotation(in GcCameraDevice camera, out float degree)
        {
            if (m_TextureDict.TryGetValue(camera.DeviceName, out var texture))
            {
                degree = Mathf.Repeat(-texture.videoRotationAngle, 360f);
                return true;
            }
            degree = default;
            return false;
        }

        public bool TryGetCameraImageSize(in GcCameraDevice camera, out int2 resolution)
        {
            if (m_TextureDict.TryGetValue(camera.DeviceName, out var texture))
            {
                resolution = GetRotatedCameraSize(texture);
                return true;
            }
            resolution = default;
            return false;
        }

        public int UpdateCameraDevice()
        {
            foreach (var tex in m_TextureDict.Values)
            {
                tex.Stop();
                Object.Destroy(tex);
            }
            m_TextureDict.Clear();

            m_DeviceListReadOnly = null;
            m_DeviceList.Clear();

            foreach (var device in WebCamTexture.devices)
            {
                var colorName = device.name;
                var depthName = device.depthCameraName;
                var isFront = device.isFrontFacing;

                var resArray = device.availableResolutions;
                var gcResArray = new GcResolution[resArray?.Length ?? 0];
                for (var i = 0; i < gcResArray.Length; i++)
                {
                    gcResArray[i] = (GcResolution)resArray[i];
                }

                if (colorName != depthName)
                {
                    m_DeviceList.Add(new GcCameraDevice(colorName, false, gcResArray, isFront, device.isAutoFocusPointSupported));
                }
                if (!string.IsNullOrEmpty(depthName))
                {
                    m_DeviceList.Add(new GcCameraDevice(depthName, true, gcResArray, isFront, false));
                }
            }

            if (m_DeviceList.Count > 0)
            {
                m_DeviceListReadOnly = m_DeviceList.AsReadOnly();
            }
            return m_DeviceList.Count;
        }
        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal GcInputCameraEngine(in GcContext context)
        {
            m_Context = context;
            m_DeviceList = new List<GcCameraDevice>();
            m_TextureDict = new Dictionary<string, WebCamTexture>();
        }

        void System.IDisposable.Dispose()
        {
            if (m_TextureDict != null)
            {
                foreach (var tex in m_TextureDict.Values)
                {
                    if (tex)
                    {
                        tex.Stop();
                        Object.Destroy(tex);
                    }
                }
                m_TextureDict.Clear();
            }

            m_DeviceListReadOnly = null;
            m_DeviceList.Clear();
        }

        void IEngine.OnAfterDraw() { }

        void IEngine.OnBeforeUpdate(in System.DateTimeOffset now) { }

        internal float2x3 CalcCameraMatrix(in WebCamTexture tex, in GcAnchor anchor)
        {
            var size = new float2(tex.width, tex.height);
            var mtx = GcAffine.FromScale(size);
            var offset = GcGraphicsEngine.GetOffset(anchor) - GcGraphicsEngine.GetOffset(GcAnchor.MiddleCenter);

            if (tex.videoVerticallyMirrored)
            {
                var t = size * offset;
                mtx = GcAffine.FromTranslate(t)
                    .Mul(GcAffine.FromScale(new float2(1f, -1f)))
                    .Mul(GcAffine.FromTranslate(-t))
                    .Mul(mtx);
            }

            var deg = GcMath.Repeat(tex.videoRotationAngle, 360f);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int2 GetRotatedCameraSize(in WebCamTexture texture)
        {
            var deg = Mathf.Repeat(-texture.videoRotationAngle, 360f);
            return (GcMath.AlmostSame(deg, 90f) || GcMath.AlmostSame(deg, 270f))
                ? new int2(texture.height, texture.width)
                : new int2(texture.width, texture.height);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void InitCameraDevice()
        {
            if (m_IsDeviceListInitialized) return;

            UpdateCameraDevice();
            m_IsDeviceListInitialized = true;
        }

        /// <remarks><see href="https://qiita.com/utibenkei/items/65b56c13f43ce5809561">参考記事</see></remarks>
        private Coroutine RequestUserAuthorizedPermissionCoroutine(System.Action<bool> callback)
        {
#if UNITY_EDITOR
            yield return null;
            callback?.Invoke(true);
#elif UNITY_ANDROID
            if (HasUserAuthorizedPermissionCamera)
            {
                yield return null;
                callback?.Invoke(true);
            }
            else
            {
                var onFocus = false;
                m_Context.Behaviour.OnFocusOnce += () => onFocus = true;
                UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Camera);
                while (!onFocus) yield return null;
                yield return null;
                callback?.Invoke(HasUserAuthorizedPermissionCamera);
            }
#elif UNITY_IOS
            if (HasUserAuthorizedPermissionCamera)
            {
                yield return null;
                callback?.Invoke(true);
            }
            else
            {
                var finish = false;
                System.Action cbInternal = () =>
                {
                    finish = true;
                    callback?.Invoke(HasUserAuthorizedPermissionCamera);
                };
                m_Context.Behaviour.OnFocusOnce += cbInternal;
                yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);

                // タイムアウト処理
                yield return new WaitForSecondsRealtime(0.5f);
                if (!finish)
                {
                    m_Context.Behaviour.OnFocusOnce -= cbInternal;
                    callback?.Invoke(HasUserAuthorizedPermissionCamera);
                }
            }
#endif // UNITY_EDITOR
        }
        #endregion
    }
}
