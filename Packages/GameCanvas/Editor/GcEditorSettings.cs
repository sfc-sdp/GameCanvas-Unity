/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2022 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
#nullable enable
using System.IO;
using UnityEngine;

namespace GameCanvas.Editor
{
    [System.Serializable]
    public sealed class GcEditorSettings
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        private const int k_SerializeVersion = 1;
        private const string k_Path = "ProjectSettings/GameCanvasEditorSettings.json";

        private static GcEditorSettings? s_Instance;

        [SerializeField]
        private int SerializeVersion = k_SerializeVersion;
        [SerializeField]
        public bool OpenGameSceneOnLaunchEditor = true;
        [SerializeField]
        public bool CheckBuildTargetOnLaunchEditor = true;
        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        public static GcEditorSettings CurrentSettings
            => s_Instance ??= Load();

        public void Save()
        {
            if (s_Instance != this) return;

            if (File.Exists(k_Path))
            {
                File.Delete(k_Path);
            }

            s_Instance.SerializeVersion = k_SerializeVersion;
            var json = JsonUtility.ToJson(s_Instance, true);
            File.WriteAllText(k_Path, json);
        }
        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        internal static GcEditorSettings Load()
        {
            GcEditorSettings instance;

            if (File.Exists(k_Path))
            {
                var json = File.ReadAllText(k_Path);
                instance = JsonUtility.FromJson<GcEditorSettings>(json);
                if (instance != null)
                {
                    if (instance.SerializeVersion == k_SerializeVersion) return instance;

                    Debug.LogWarning("[GameCanvas] バージョン更新により、エディタ設定が初期化されました\n");
                }
            }

            instance = new GcEditorSettings();
            instance.Save();
            return instance;
        }
        #endregion
    }
}
