/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2019 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/

namespace GameCanvas.Engine
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Networking;

    public sealed class Network
    {
        //----------------------------------------------------------
        #region フィールド変数
        //----------------------------------------------------------

        private readonly BehaviourBase cBehaviour;
        private readonly Graphic cGraphic;
        private readonly Dictionary<string, object> cCache;

        #endregion

        //----------------------------------------------------------
        #region パブリック関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal Network(BehaviourBase behaviour, Graphic graphic)
        {
            cBehaviour = behaviour;
            cGraphic = graphic;
            cCache = new Dictionary<string, object>();
        }

        public void Clear()
        {
            cCache.Clear();
        }

        public EDownloadState DrawOnlineImage(ref string url, ref int x, ref int y)
        {
            if (!cCache.ContainsKey(url))
            {
                // ダウンロード開始
                cCache.Add(url, null);
                cBehaviour.StartCoroutine(downloadImage(url));
                return EDownloadState.Begin;
            }

            var cache = cCache[url] as Texture2D;
            if (cache == null)
            {
                // ダウンロード中
                return EDownloadState.Downloading;
            }

            cGraphic.DrawTexture(cache, ref x, ref y);
            return EDownloadState.Complete;
        }

        public int GetOnlineImageWidth(ref string url)
        {
            if (!cCache.ContainsKey(url))
            {
                // ダウンロード開始
                cCache.Add(url, null);
                cBehaviour.StartCoroutine(downloadImage(url));
                return 0;
            }

            var cache = cCache[url] as Texture2D;
            if (cache == null) return 0;
            return cache.width;
        }

        public int GetOnlineImageHeight(ref string url)
        {
            if (!cCache.ContainsKey(url))
            {
                // ダウンロード開始
                cCache.Add(url, null);
                cBehaviour.StartCoroutine(downloadImage(url));
                return 0;
            }

            var cache = cCache[url] as Texture2D;
            if (cache == null) return 0;
            return cache.height;
        }

        public EDownloadState GetOnlineText(ref string url, ref string text)
        {
            if (!cCache.ContainsKey(url))
            {
                // ダウンロード開始
                cCache.Add(url, null);
                cBehaviour.StartCoroutine(downloadText(url));
                return EDownloadState.Begin;
            }

            var cache = cCache[url] as string;
            if (cache == null)
            {
                // ダウンロード中
                return EDownloadState.Downloading;
            }

            text = cache;
            return EDownloadState.Complete;
        }

        #endregion

        //----------------------------------------------------------
        #region プライベート関数
        //----------------------------------------------------------

        private IEnumerator downloadImage(string url)
        {
            var req = UnityWebRequestTexture.GetTexture(url, true);
#if UNITY_2017_2_OR_NEWER
            yield return req.SendWebRequest();
#else
            yield return req.Send();
#endif //UNITY_2017_2_OR_NEWER

            if (req.isNetworkError || req.isHttpError)
            {
                Debug.LogWarning(req.error);
                yield break;
            }

            cCache[url] = ((DownloadHandlerTexture)req.downloadHandler).texture;
        }

        private IEnumerator downloadText(string url)
        {
            var req = UnityWebRequest.Get(url);
#if UNITY_2017_2_OR_NEWER
            yield return req.SendWebRequest();
#else
            yield return req.Send();
#endif //UNITY_2017_2_OR_NEWER

            if (req.isNetworkError || req.isHttpError)
            {
                Debug.LogWarning(req.error);
                cCache[url] = "";
                yield break;
            }

            var text = req.downloadHandler.text;
            cCache[url] = text;
        }

        #endregion
    }
}
