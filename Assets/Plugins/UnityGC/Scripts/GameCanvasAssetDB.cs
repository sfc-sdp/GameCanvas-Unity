/*------------------------------------------------------------*/
/// <summary>Image Database</summary>
/// <author>Seibe TAKAHASHI</author>
/// <remarks>
/// (c) 2015-2017 Smart Device Programming.
/// This software is released under the MIT License.
/// http://opensource.org/licenses/mit-license.php
/// </remarks>
/*------------------------------------------------------------*/
using UnityEngine;

namespace GameCanvas
{
    /// <summary>
    /// リソース情報を保持するためのカスタムアセット
    /// </summary>
    public class GameCanvasAssetDB : ScriptableObject
    {
        /// <summary>
        /// カスタムアセットの保存先
        /// </summary>
        public const string Path = "Assets/Plugins/UnityGC/Resources/GCAssetDB.asset";

        /// <summary>
        /// 画像のリスト
        /// </summary>
        public Sprite[] images;

        /// <summary>
        /// 音声のリスト
        /// </summary>
        public AudioClip[] sounds;

        /// <summary>
        /// 矩形
        /// </summary>
        public Sprite rect;

        /// <summary>
        /// 円
        /// </summary>
        public Sprite circle;

        /// <summary>
        /// ダミー
        /// </summary>
        public Sprite dummy;

        /// <summary>
        /// 文字のリスト
        /// </summary>
        public Sprite[] characters;

        /// <summary>
        /// スプライト用マテリアル
        /// </summary>
        public Material material;
    }
}
