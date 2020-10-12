/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using UnityEditor;

namespace GameCanvas.Editor
{
    static class GcEditorMenu
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        private const string k_MenuPath001 = "GameCanvas/パッケージを出力する";
        private const string k_MenuPath002 = "GameCanvas/アプリをビルドする";
        private const string k_MenuPath101 = "GameCanvas/リソース定義の強制更新";
        private const string k_MenuPath201 = "GameCanvas/エディタ設定/起動時に Game.unity を開く";
        private const string k_MenuPath202 = "GameCanvas/エディタ設定/起動時にビルドターゲットを確認する";
        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        internal static void OnReload()
        {
            var settings = GcEditorSettings.CurrentSettings;
            Menu.SetChecked(k_MenuPath201, settings.OpenGameSceneOnLaunchEditor);
            Menu.SetChecked(k_MenuPath202, settings.CheckBuildTargetOnLaunchEditor);
        }

#pragma warning disable IDE0051
        [MenuItem(k_MenuPath001, priority = 1)]
        private static void ExportPackage()
        {
            GcEditorExporter.ExportPackage();
        }

        [MenuItem(k_MenuPath002, priority = 2)]
        private static void OpenBuildWindow()
        {
            GcEditorBuilder.OpenWindow();
        }

        [MenuItem(k_MenuPath101, priority = 101)]
        private static void BuildResource()
        {
            GcEditorResourceBuilder.Build();
        }

        [MenuItem(k_MenuPath201, priority = 201)]
        private static void ToggleOpenGameSceneOnLaunchEditor()
        {
            var settings = GcEditorSettings.CurrentSettings;
            settings.OpenGameSceneOnLaunchEditor = !settings.OpenGameSceneOnLaunchEditor;
            settings.Save();
            Menu.SetChecked(k_MenuPath201, settings.OpenGameSceneOnLaunchEditor);
        }

        [MenuItem(k_MenuPath202, priority = 201)]
        private static void ToggleCheckBuildTargetOnLaunchEditor()
        {
            var settings = GcEditorSettings.CurrentSettings;
            settings.CheckBuildTargetOnLaunchEditor = !settings.CheckBuildTargetOnLaunchEditor;
            settings.Save();
            Menu.SetChecked(k_MenuPath202, settings.CheckBuildTargetOnLaunchEditor);
        }
#pragma warning restore IDE0051
        #endregion
    }
}
