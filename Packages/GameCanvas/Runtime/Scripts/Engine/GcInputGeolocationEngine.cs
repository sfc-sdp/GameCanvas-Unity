/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using UnityEngine;
using Coroutine = System.Collections.IEnumerator;

namespace GameCanvas.Engine
{
    sealed class GcInputGeolocationEngine : IInputGeolocation, IEngine
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

#pragma warning disable IDE0032
        readonly GcContext m_Context;
        readonly LocationService m_Service;

        bool m_DidUpdateThisFrame;
        GcGeolocationEvent m_LastEvent;
        LocationServiceStatus m_Status;
#pragma warning restore IDE0032
        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal GcInputGeolocationEngine(in GcContext context)
        {
            m_Context = context;
            m_Service = Input.location;

            m_Status = LocationServiceStatus.Stopped;
            m_DidUpdateThisFrame = false;
            m_LastEvent = GcGeolocationEvent.Null;
        }

        public bool DidUpdateGeolocationThisFrame => m_DidUpdateThisFrame;

        public LocationServiceStatus GeolocationStatus => m_Status;

        public bool HasUserAuthorizedPermissionGeolocation
#if UNITY_EDITOR
            => m_Service.isEnabledByUser;
#elif UNITY_ANDROID
            => UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.FineLocation);
#elif UNITY_IOS
            => m_Service.isEnabledByUser;
#endif // UNITY_EDITOR

        public GcGeolocationEvent LastGeolocationEvent => m_LastEvent;

        public void RequestUserAuthorizedPermissionGeolocationAsync(in System.Action<bool> callback)
        {
            var coroutine = RequestUserAuthorizedPermissionCoroutine(callback);
            m_Context.Behaviour.StartCoroutine(coroutine);
        }

        public void StartGeolocationService(float desiredAccuracy = 10f, float updateDistance = 10f)
        {
            if (m_Service.status == LocationServiceStatus.Stopped)
            {
                m_Service.Start(desiredAccuracy, updateDistance);
            }
        }

        public void StopGeolocationService()
        {
            if (m_Service.status != LocationServiceStatus.Stopped)
            {
                m_Service.Stop();
            }
        }

        public bool TryGetGeolocationEvent(out GcGeolocationEvent e)
        {
            e = m_LastEvent;
            return m_DidUpdateThisFrame;
        }
        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        void System.IDisposable.Dispose() { }

        void IEngine.OnAfterDraw() { }

        void IEngine.OnBeforeUpdate(in System.DateTimeOffset now)
        {
            m_DidUpdateThisFrame = false;

            if (!m_Service.isEnabledByUser) return;

            m_Status = m_Service.status;
            if (m_Status != LocationServiceStatus.Running) return;

            var e = new GcGeolocationEvent(m_Service.lastData);
            if (e.Time != m_LastEvent.Time)
            {
                m_LastEvent = e;
                m_DidUpdateThisFrame = true;
            }
        }

        private Coroutine RequestUserAuthorizedPermissionCoroutine(System.Action<bool> callback)
        {
#if UNITY_EDITOR
            yield return null;
            callback?.Invoke(m_Service.isEnabledByUser);
#elif UNITY_ANDROID
            if (HasUserAuthorizedPermissionGeolocation)
            {
                yield return null;
                callback?.Invoke(true);
            }
            else
            {
                var onFocus = false;
                m_Context.Behaviour.OnFocusOnce += () => onFocus = true;
                UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.FineLocation);
                while (!onFocus) yield return null;
                yield return null;
                callback?.Invoke(HasUserAuthorizedPermissionGeolocation);
            }
#elif UNITY_IOS
            if (HasUserAuthorizedPermissionGeolocation)
            {
                yield return null;
                callback?.Invoke(true);
            }
            else
            {
                throw new System.NotImplementedException();
            }
#endif // UNITY_EDITOR
        }
        #endregion
    }
}
