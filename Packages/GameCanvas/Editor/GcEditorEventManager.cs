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
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace GameCanvas.Editor
{
    [InitializeOnLoad]
    sealed class GcEditorEventManager : AssetPostprocessor
        , IActiveBuildTargetChanged, IPreprocessBuildWithReport
#if UNITY_ANDROID
        , UnityEditor.Android.IPostGenerateGradleAndroidProject
#endif // UNITY_ANDROID
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        const string k_LaunchFlagPath = "Temp/GameCanvasTempFile-LaunchFlag";
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

            static void OnReload()
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

#if UNITY_ANDROID
        void UnityEditor.Android.IPostGenerateGradleAndroidProject.OnPostGenerateGradleAndroidProject(string path)
        {
            GcEditorBuilder.OnPostGenerateGradleAndroidProject(path);
        }
#endif // UNITY_ANDROID

        void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report)
        {
            GcEditorResourceBuilder.Build();
        }

        static void OnLaunch()
        {
            GcEditorSceneHelper.OnLaunch();
            GcEditorBuilder.OnLaunch();
            GcEditorResourceBuilder.OnLaunch();
        }

        static void OnPlayModeStateChanged(PlayModeStateChange state)
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

        static void OnPostCompile()
        {
            // empty
        }

        static void OnPreCompile()
        {
            // empty
        }

#pragma warning disable IDE0051
        void OnPostprocessAudio(AudioClip clip)
        {
            var importer = (AudioImporter)assetImporter;
            GcEditorResourceBuilder.OnPostprocessAudio(importer, clip);
        }

        void OnPreprocessTexture()
        {
            var importer = (TextureImporter)assetImporter;
            GcEditorResourceBuilder.OnPreprocessTexture(importer);
        }

        static string OnGeneratedCSProject(string path, string content)
            => GcEditorCompiler.OnGeneratedCSProject(path, content);
#pragma warning restore IDE0051
        #endregion
    }
}
