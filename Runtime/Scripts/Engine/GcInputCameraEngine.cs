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
        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal GcInputCameraEngine(in GcContext context)
        {
            m_Context = context;
            m_DeviceList = new List<GcCameraDevice>();
            m_TextureDict = new Dictionary<string, WebCamTexture>();

            UpdateCameraDevice();
        }

        public int CameraDeviceCount => m_DeviceList.Count;

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
                resolution = new int2(texture.width, texture.height);
                if (!texture.isPlaying)
                {
                    texture.Play();
                    return texture.isPlaying;
                }
            }
            else
            {
                resolution = default;
            }
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
                return new int2(texture.width, texture.height);
            }
            return int2.zero;
        }

        public bool TryGetCameraImage(out GcCameraDevice camera)
        {
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
                resolution = new int2(texture.width, texture.height);
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
                if (callback != null)
                {
                    m_Context.Behaviour.OnFocusOnce += () => callback.Invoke(HasUserAuthorizedPermissionCamera);
                }
                UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Camera);
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
