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
    static class GcEditorExporter
    {
        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        public static void ExportPackage()
        {
            var path = EditorUtility.SaveFilePanel("パッケージ出力", "", "GameCanvas.unitypackage", "unitypackage");
            if (string.IsNullOrEmpty(path)) return;
            try
            {
                EditorUtility.DisplayProgressBar("パッケージ出力", "処理中…", 0f);
                AssetDatabase.ExportPackage(new string[] { "Assets/" }, path, ExportPackageOptions.IncludeLibraryAssets | ExportPackageOptions.Recurse);
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
        #endregion
    }
}
