/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2019 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/

#if UNITY_EDITOR
namespace GameCanvas.Editor
{
    using UnityEditor;

    public sealed class Exporter
    {
        //----------------------------------------------------------
        #region パブリック関数
        //----------------------------------------------------------

        [MenuItem("GameCanvas/パッケージを出力する")]
        public static void ExportPackage()
        {
            EditorApplication.ExecuteMenuItem("Assets/Export Package...");
        }

        #endregion
    }
}
#endif //UNITY_EDITOR
