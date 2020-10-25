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
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Networking;

namespace GameCanvas.Engine
{
    sealed class GcNetworkEngine : INetwork, IEngine
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        readonly Dictionary<string, UnityWebRequestAsyncOperation> m_DownloaderDict;
        readonly Dictionary<string, object> m_ObjectDict;
        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal GcNetworkEngine()
        {
            m_ObjectDict = new Dictionary<string, object>();
            m_DownloaderDict = new Dictionary<string, UnityWebRequestAsyncOperation>();
        }

        public void ClearDownloadCache(in string url)
        {
            if (m_ObjectDict.TryGetValue(url, out var obj))
            {
                if (obj is Texture tex)
                {
                    Resources.UnloadAsset(tex);
                }
                if (obj is AudioClip clip)
                {
                    Resources.UnloadAsset(clip);
                }
                m_ObjectDict.Remove(url);
            }

            if (m_DownloaderDict.TryGetValue(url, out var op))
            {
                var www = op.webRequest;
                www.Abort();
                www.Dispose();
                m_DownloaderDict.Remove(url);
            }
        }

        public void ClearDownloadCacheAll()
        {
            foreach (var obj in m_ObjectDict.Values)
            {
                if (obj is Texture tex)
                {
                    Resources.UnloadAsset(tex);
                }
                if (obj is AudioClip clip)
                {
                    Resources.UnloadAsset(clip);
                }
            }
            m_ObjectDict.Clear();

            foreach (var op in m_DownloaderDict.Values)
            {
                var www = op.webRequest;
                www.Abort();
                www.Dispose();
            }
            m_DownloaderDict.Clear();
        }

        public GcAvailability TryGetOnlineImage(in string url, out Texture2D texture)
        {
            if (m_ObjectDict.TryGetValue(url, out var obj))
            {
                texture = obj as Texture2D;
                return (texture != null)
                    ? GcAvailability.Ready
                    : GcAvailability.NotAvailable;
            }
            texture = null;

            if (m_DownloaderDict.TryGetValue(url, out var downloader))
            {
                return GcAvailability.NotReady;
            }

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                return GcAvailability.NotAvailable;
            }

            {
                var www = UnityWebRequestTexture.GetTexture(url, true);
                www.disposeDownloadHandlerOnDispose = true;
                var op = www.SendWebRequest();
                m_DownloaderDict.Add(www.url, op);
                op.completed += OnCompleteGetTexture;
            }
            return GcAvailability.NotReady;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetOnlineImageSize(in string url, out int2 size)
        {
            if (TryGetOnlineImage(url, out var texture) == GcAvailability.Ready)
            {
                size = new int2(texture.width, texture.height);
                return true;
            }
            size = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GcAvailability TryGetOnlineSound(in string url, out AudioClip clip)
        {
            if (url.EndsWith(".wav"))
            {
                return TryGetOnlineSound(url, AudioType.WAV, out clip);
            }
            if (url.EndsWith(".mp3"))
            {
                return TryGetOnlineSound(url, AudioType.MPEG, out clip);
            }
            if (url.EndsWith(".ogg"))
            {
                return TryGetOnlineSound(url, AudioType.OGGVORBIS, out clip);
            }

            return TryGetOnlineSound(url, AudioType.UNKNOWN, out clip);
        }

        public GcAvailability TryGetOnlineSound(in string url, in AudioType type, out AudioClip clip)
        {
            if (m_ObjectDict.TryGetValue(url, out var obj))
            {
                clip = obj as AudioClip;
                return (clip != null)
                    ? GcAvailability.Ready
                    : GcAvailability.NotAvailable;
            }
            clip = null;

            if (m_DownloaderDict.TryGetValue(url, out var downloader))
            {
                return GcAvailability.NotReady;
            }

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                return GcAvailability.NotAvailable;
            }

            {
                var www = UnityWebRequestMultimedia.GetAudioClip(url, type);
                www.disposeDownloadHandlerOnDispose = true;
                var op = www.SendWebRequest();
                m_DownloaderDict.Add(www.url, op);
                op.completed += OnCompleteGetAudioClip;
            }
            return GcAvailability.NotReady;
        }

        public GcAvailability TryGetOnlineText(in string url, out string str)
        {
            if (m_ObjectDict.TryGetValue(url, out var obj))
            {
                str = obj as string;
                return (str != null)
                    ? GcAvailability.Ready
                    : GcAvailability.NotAvailable;
            }
            str = null;

            if (m_DownloaderDict.TryGetValue(url, out var downloader))
            {
                return GcAvailability.NotReady;
            }

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                return GcAvailability.NotAvailable;
            }

            {
                var www = UnityWebRequest.Get(url);
                www.disposeDownloadHandlerOnDispose = true;
                var op = www.SendWebRequest();
                m_DownloaderDict.Add(www.url, op);
                op.completed += OnCompleteGetText;
            }
            return GcAvailability.NotReady;
        }
        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        void System.IDisposable.Dispose() { }

        void IEngine.OnAfterDraw() { }

        void IEngine.OnBeforeUpdate(in System.DateTimeOffset now) { }

        private void OnCompleteGetAudioClip(AsyncOperation op)
        {
            var www = ((UnityWebRequestAsyncOperation)op).webRequest;
            var url = www.url;

            m_DownloaderDict.Remove(url);

            if (www.responseCode != 200)
            {
                m_ObjectDict.Add(url, null);
            }
            else
            {
                var handler = www.downloadHandler as DownloadHandlerAudioClip;
                handler.compressed = true;
                handler.streamAudio = true;
                m_ObjectDict.Add(url, handler.audioClip);
            }

            www.Dispose();
        }

        private void OnCompleteGetText(AsyncOperation op)
        {
            var www = ((UnityWebRequestAsyncOperation)op).webRequest;
            var url = www.url;

            m_DownloaderDict.Remove(url);

            if (www.responseCode != 200)
            {
                m_ObjectDict.Add(url, null);
            }
            else
            {
                var handler = www.downloadHandler;
                m_ObjectDict.Add(url, handler.text);
            }

            www.Dispose();
        }

        private void OnCompleteGetTexture(AsyncOperation op)
        {
            var www = ((UnityWebRequestAsyncOperation)op).webRequest;
            var url = www.url;

            m_DownloaderDict.Remove(url);

            if (www.responseCode != 200)
            {
                m_ObjectDict.Add(url, null);
            }
            else
            {
                var handler = www.downloadHandler as DownloadHandlerTexture;
                m_ObjectDict.Add(url, handler.texture);
            }

            www.Dispose();
        }
        #endregion
    }
}
