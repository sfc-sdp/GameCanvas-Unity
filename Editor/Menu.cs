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
    static class Menu
    {
        //----------------------------------------------------------
        #region フィールド変数
        //----------------------------------------------------------

        private const string k_MenuPath11 = "GameCanvas/パッケージを出力する";
        private const string k_MenuPath12 = "GameCanvas/アプリをビルドする";
        private const string k_MenuPath30 = "GameCanvas/エディタ設定/起動時に Game.unity を開く";
        private const string k_MenuPath31 = "GameCanvas/エディタ設定/起動時にビルドターゲットを確認する";

        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        internal static void OnReload()
        {
            var settings = EditorSettings.CurrentSettings;
            UnityEditor.Menu.SetChecked(k_MenuPath30, settings.OpenGameSceneOnLaunchEditor);
            UnityEditor.Menu.SetChecked(k_MenuPath31, settings.CheckBuildTargetOnLaunchEditor);
        }

#pragma warning disable IDE0051
        [MenuItem(k_MenuPath11, priority = 11)]
        private static void ExportPackage()
        {
            Exporter.ExportPackage();
        }

        [MenuItem(k_MenuPath12, priority = 12)]
        private static void OpenBuildWindow()
        {
            Builder.OpenWindow();
        }

        [MenuItem(k_MenuPath30, priority = 30)]
        private static void ToggleOpenGameSceneOnLaunchEditor()
        {
            var settings = EditorSettings.CurrentSettings;
            settings.OpenGameSceneOnLaunchEditor = !settings.OpenGameSceneOnLaunchEditor;
            settings.Save();
            UnityEditor.Menu.SetChecked(k_MenuPath30, settings.OpenGameSceneOnLaunchEditor);
        }

        [MenuItem(k_MenuPath31, priority = 31)]
        private static void ToggleCheckBuildTargetOnLaunchEditor()
        {
            var settings = EditorSettings.CurrentSettings;
            settings.CheckBuildTargetOnLaunchEditor = !settings.CheckBuildTargetOnLaunchEditor;
            settings.Save();
            UnityEditor.Menu.SetChecked(k_MenuPath31, settings.CheckBuildTargetOnLaunchEditor);
        }
#pragma warning restore IDE0051

        #endregion
    }
}






















































































































































































