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
    static class GcEditorUtility
    {
        //----------------------------------------------------------
        #region 構造体定義
        //----------------------------------------------------------

        public readonly struct AssetEditingScopeStruct : System.IDisposable
        {
            public void Dispose() => AssetDatabase.StopAssetEditing();
        }

        public readonly struct ProgressBarScopeStruct : System.IDisposable
        {
            public void Dispose() => EditorUtility.ClearProgressBar();

            public void Update(in string title, in string info, in float progress)
                            => EditorUtility.DisplayProgressBar(title, info, progress);
        }
        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        public static AssetEditingScopeStruct AssetEditingScope()
        {
            AssetDatabase.StartAssetEditing();
            return default;
        }

        public static ProgressBarScopeStruct ProgressBarScope(in string title, in string info)
        {
            EditorUtility.DisplayProgressBar(title, info, 0f);
            return default;
        }
        #endregion
    }
}
