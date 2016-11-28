/*------------------------------------------------------------*/
/// <summary>GameCanvas for Unity [Updater]</summary>
/// <author>Seibe TAKAHASHI</author>
/// <remarks>
/// (c) 2015-2016 Smart Device Programming.
/// This software is released under the MIT License.
/// http://opensource.org/licenses/mit-license.php
/// </remarks>
/*------------------------------------------------------------*/
using UnityEngine;
using UnityEditor;

namespace GameCanvas.Editor
{
    /// <summary>
    /// 更新催促システム
    /// </summary>
    [InitializeOnLoad]
    public static class Updater
    {
        static Updater()
        {
            EditorApplication.delayCall += Awake;
        }

        static void Awake()
        {
            EditorApplication.delayCall -= Awake;

            CheckUnityVersion();
            CheckGameCanvasVersion();
        }

        static void CheckUnityVersion()
        {
#if !UNITY_5_4_OR_NEWER || UNITY_5_4_1 || UNITY_5_4_2
            EditorUtility.DisplayDialog("Required Unity 5.4.3", "Unityのバージョンが古いです。5.4.3以降をインストールしてください", "OK");
#else
            Debug.LogFormat("GameCanvas {0}", Env.Version);
#endif
        }

        static void CheckGameCanvasVersion()
        {
            // ToDo
        }
    }
}
