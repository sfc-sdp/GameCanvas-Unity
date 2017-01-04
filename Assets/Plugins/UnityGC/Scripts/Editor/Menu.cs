/*------------------------------------------------------------*/
/// <summary>GameCanvas for Unity [Menu]</summary>
/// <author>Seibe TAKAHASHI</author>
/// <remarks>
/// (c) 2015-2017 Smart Device Programming.
/// This software is released under the MIT License.
/// http://opensource.org/licenses/mit-license.php
/// </remarks>
/*------------------------------------------------------------*/
using UnityEditor;
using UnityEngine;
using System.IO;

namespace GameCanvas.Editor
{
    /// <summary>
    /// エディターメニュー
    /// </summary>
    public class Menu
    {
        [MenuItem("GameCanvas/アプリをビルドする", false, 100)]
        static void OpenBuilder()
        {
            BuildWindow.Open();
        }

        [MenuItem("GameCanvas/Game.cs をクリップボードにコピー", false, 200)]
        static void CopyScript()
        {
            var targetPath = Path.Combine(Application.dataPath, "Scripts/Game.cs");
            if (!File.Exists(targetPath))
            {
                EditorUtility.DisplayDialog("Game.cs が見つかりません", "Game.csが既定の場所にないのでコピーできませんでした", "OK");
                return;
            }

            var content = File.ReadAllText(targetPath);
            EditorGUIUtility.systemCopyBuffer = content;
            Debug.Log("クリップボードに Game.cs の内容をコピーしました");
        }

        [MenuItem("GameCanvas/UnityPackage の書き出し", false, 201)]
        static void ExportUnityPackage()
        {
            EditorApplication.ExecuteMenuItem("Assets/Export Package...");
        }

        [MenuItem("GameCanvas/アセットデータベースの強制更新", false, 202)]
        static void ForceRebuildDatabase()
        {
            AssetProcessor.RebuildAssetDatabase();
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }

        [MenuItem("GameCanvas/About GameCanvas", false, 1000)]
        static void OpenAbout()
        {
            var message = Env.Copyright + "\n\nAuthors: " + string.Join(", ", Env.Authors);
            EditorUtility.DisplayDialog("GameCanvas " + Env.Version, message, "OK");
        }
    }
}
