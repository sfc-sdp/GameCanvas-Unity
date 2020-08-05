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
using UnityEditor;
using UnityEditor.Build;

namespace GameCanvas.Editor
{
    [InitializeOnLoad]
    sealed class EditorEventHelper : IActiveBuildTargetChanged
    {
        //----------------------------------------------------------
        #region フィールド変数
        //----------------------------------------------------------

        private const string k_LaunchFlagPath = "Temp/GameCanvasTempFile-LaunchFlag";

        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        static EditorEventHelper()
        {
            if (EditorApplication.isCompiling) return;

            EditorApplication.delayCall += OnReload;

            void OnReload()
            {
                EditorSettings.Load();

                if (File.Exists(k_LaunchFlagPath))
                {
                    // script reload event
                    OnRecompile();
                }
                else
                {
                    File.Create(k_LaunchFlagPath);

                    // editor launch event
                    OnLaunch();
                }

                Menu.OnReload();
            }

            void OnLaunch()
            {
                SceneHelper.OnLaunch();
                Builder.OnLaunch();
            }

            void OnRecompile()
            {
                // todo
            }
        }

        int IOrderedCallback.callbackOrder => 1;

        void IActiveBuildTargetChanged.OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
        {
            UnityEngine.Debug.Log($"OnActiveBuildTargetChanged: {previousTarget} -> {newTarget}");
        }

        #endregion
    }
}
