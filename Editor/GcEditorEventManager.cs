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
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace GameCanvas.Editor
{
    [InitializeOnLoad]
    sealed class GcEditorEventManager : AssetPostprocessor, IActiveBuildTargetChanged, IPreprocessBuildWithReport
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        private const string k_LaunchFlagPath = "Temp/GameCanvasTempFile-LaunchFlag";

        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        static GcEditorEventManager()
        {
            if (EditorApplication.isCompiling) return;

            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            AssemblyReloadEvents.beforeAssemblyReload += OnPreCompile;
            EditorApplication.delayCall += OnReload;

            void OnReload()
            {
                GcEditorSettings.Load();

                if (File.Exists(k_LaunchFlagPath))
                {
                    // script reload event
                    OnPostCompile();
                }
                else
                {
                    File.Create(k_LaunchFlagPath);

                    // editor launch event
                    OnLaunch();
                }

                GcEditorMenu.OnReload();
            };
        }

        int IOrderedCallback.callbackOrder => 0;

        void IActiveBuildTargetChanged.OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
        {
            Debug.Log($"OnActiveBuildTargetChanged: {previousTarget} -> {newTarget}");
        }

        void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report)
        {
            GcEditorResourceBuilder.Build();
        }

        private static void OnLaunch()
        {
            GcEditorSceneHelper.OnLaunch();
            GcEditorBuilder.OnLaunch();
            GcEditorResourceBuilder.OnLaunch();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredEditMode:
                    break;

                case PlayModeStateChange.ExitingEditMode:
                    GcEditorResourceBuilder.Build();
                    break;

                case PlayModeStateChange.EnteredPlayMode:
                    break;

                case PlayModeStateChange.ExitingPlayMode:
                    break;
            }
        }

        private static void OnPostCompile()
        {
            // empty
        }

        private static void OnPreCompile()
        {
            // empty
        }

#pragma warning disable IDE0051
        void OnPostprocessAudio(AudioClip clip)
        {
            var importer = assetImporter as AudioImporter;
            GcEditorResourceBuilder.OnPostprocessAudio(importer, clip);
        }

        void OnPreprocessTexture()
        {
            var importer = assetImporter as TextureImporter;
            GcEditorResourceBuilder.OnPreprocessTexture(importer);
        }
#pragma warning restore IDE0051
        #endregion
    }
}
