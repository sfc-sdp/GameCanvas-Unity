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

namespace GameCanvas.Editor
{
    [System.Serializable]
    public sealed class EditorSettings
    {
        //----------------------------------------------------------
        #region フィールド変数
        //----------------------------------------------------------

        private const int k_SerializeVersion = 1;
        private const string k_Path = "ProjectSettings/GameCanvasEditorSettings.json";

        private static EditorSettings sInstance;

        [SerializeField]
        private int SerializeVersion = k_SerializeVersion;
        [SerializeField]
        public bool OpenGameSceneOnLaunchEditor = true;
        [SerializeField]
        public bool CheckBuildTargetOnLaunchEditor = true;

        #endregion

        //----------------------------------------------------------
        #region パブリック関数
        //----------------------------------------------------------

        public static EditorSettings CurrentSettings
        {
            get
            {
                if (sInstance == null)
                {
                    Load();
                }
                return sInstance;
            }
        }

        public void Save()
        {
            if (sInstance != this) return;

            if (File.Exists(k_Path))
            {
                File.Delete(k_Path);
            }

            sInstance.SerializeVersion = k_SerializeVersion;
            var json = JsonUtility.ToJson(sInstance, true);
            File.WriteAllText(k_Path, json);
        }

        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        internal static void Load()
        {
            if (File.Exists(k_Path))
            {
                var json = File.ReadAllText(k_Path);
                sInstance = JsonUtility.FromJson<EditorSettings>(json);
                if (sInstance != null)
                {
                    if (sInstance.SerializeVersion == k_SerializeVersion) return;

                    Debug.LogWarning("[GameCanvas] バージョン更新により、エディタ設定が初期化されました\n");
                }
            }

            sInstance = new EditorSettings();
            sInstance.Save();
        }

        #endregion
    }
}
