/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using System.IO;
using UnityEngine;
using Coroutine = System.Collections.IEnumerator;

namespace GameCanvas.Engine
{
    sealed class GcStorageEngine : IStorage, IEngine
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        readonly GcContext m_Context;
        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        public void EraseSavedDataAll()
        {
            PlayerPrefs.DeleteAll();
        }

        public void Save(in string key, string value)
        {
            if (value != null)
            {
                PlayerPrefs.SetString(key, value);
                PlayerPrefs.Save();
            }
            else if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
                PlayerPrefs.Save();
            }
        }

        public void Save(in string key, float? value)
        {
            if (value.HasValue)
            {
                PlayerPrefs.SetFloat(key, value.Value);
                PlayerPrefs.Save();
            }
            else if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
                PlayerPrefs.Save();
            }
        }

        public void Save(in string key, int? value)
        {
            if (value.HasValue)
            {
                PlayerPrefs.SetInt(key, value.Value);
                PlayerPrefs.Save();
            }
            else if (PlayerPrefs.HasKey(key))
            {
                PlayerPrefs.DeleteKey(key);
                PlayerPrefs.Save();
            }
        }

        public void SaveScreenshotAsync(in System.Action<string> onComplete = null)
        {
            m_Context.Behaviour.StartCoroutine(SaveScreenshotCoroutine(onComplete));
        }

        public bool TryLoad(in string key, out string value)
        {
            if (PlayerPrefs.HasKey(key))
            {
                value = PlayerPrefs.GetString(key);
                return true;
            }
            value = null;
            return false;
        }

        public bool TryLoad(in string key, out float value)
        {
            if (PlayerPrefs.HasKey(key))
            {
                value = PlayerPrefs.GetFloat(key);
                return true;
            }
            value = default;
            return false;
        }

        public bool TryLoad(in string key, out int value)
        {
            if (PlayerPrefs.HasKey(key))
            {
                value = PlayerPrefs.GetInt(key);
                return true;
            }
            value = default;
            return false;
        }
        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        Coroutine SaveScreenshotCoroutine(System.Action<string> onComplete)
        {
            var filename = $"screenshot-{m_Context.Time.CurrentTimestamp}.png";
#if UNITY_EDITOR
            var path = filename;
#elif UNITY_ANDROID
            var path = Path.Combine(Application.persistentDataPath, filename);
#elif UNITY_IOS
            var path = Path.Combine(Application.persistentDataPath, filename);
#else
            var path = Path.Combine(Application.temporaryCachePath, filename);
#endif // UNITY_EDITOR

            yield return new WaitForEndOfFrame();

            ScreenCapture.CaptureScreenshot(path);

            var success = false;
            for (var i = 0; i < 300; i++)
            {
                yield return null;

                if (File.Exists(path))
                {
                    success = true;
                    break;
                }
            }

            onComplete?.Invoke(success ? path : null);
        }

        internal GcStorageEngine(in GcContext context)
        {
            m_Context = context;
        }

        void System.IDisposable.Dispose() { }

        void IEngine.OnAfterDraw() { }

        void IEngine.OnBeforeUpdate(in System.DateTimeOffset now) { }
        #endregion
    }
}
