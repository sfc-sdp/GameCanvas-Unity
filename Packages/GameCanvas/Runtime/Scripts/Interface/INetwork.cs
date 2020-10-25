/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using System.ComponentModel;
using Unity.Mathematics;
using UnityEngine;

namespace GameCanvas
{
    public interface INetwork
    {
        /// <summary>
        /// 指定されたオンラインリソースのダウンロードキャッシュを削除します
        /// </summary>
        /// <param name="url">リソースURL</param>
        void ClearDownloadCache(in string url);

        /// <summary>
        /// 全てのオンラインリソースのダウンロードキャッシュを削除します
        /// </summary>
        void ClearDownloadCacheAll();

        /// <summary>
        /// オンライン画像リソースの取得を試みます
        /// </summary>
        /// <param name="url">リソースURL</param>
        /// <param name="texture">画像リソース。戻り値が<see cref="GcAvailability.Ready"/>以外だとNull</param>
        /// <returns>オンラインリソースの可用性</returns>
        GcAvailability TryGetOnlineImage(in string url, out Texture2D texture);

        /// <summary>
        /// オンライン画像リソースの寸法を取得します
        /// </summary>
        /// <param name="url">リソースURL</param>
        /// <param name="size">オンライン画像リソースの寸法</param>
        /// <returns>オンラインリソースを取得できたかどうか</returns>
        bool TryGetOnlineImageSize(in string url, out int2 size);

        /// <summary>
        /// オンライン音声リソースの取得を試みます
        /// </summary>
        /// <param name="url">リソースURL</param>
        /// <param name="clip">音声リソース。戻り値が<see cref="GcAvailability.Ready"/>以外だとNull</param>
        /// <returns>オンラインリソースの可用性</returns>
        GcAvailability TryGetOnlineSound(in string url, out AudioClip clip);

        /// <summary>
        /// オンライン音声リソースの取得を試みます
        /// </summary>
        /// <param name="url">リソースURL</param>
        /// <param name="type">音声リソースの形式</param>
        /// <param name="clip">音声リソース。戻り値が<see cref="GcAvailability.Ready"/>以外だとNull</param>
        /// <returns>オンラインリソースの可用性</returns>
        GcAvailability TryGetOnlineSound(in string url, in AudioType type, out AudioClip clip);

        /// <summary>
        /// オンラインテキストの取得を試みます
        /// </summary>
        /// <param name="url">リソースURL</param>
        /// <param name="clip">テキスト。戻り値が<see cref="GcAvailability.Ready"/>以外だとNull</param>
        /// <returns>オンラインリソースの可用性</returns>
        GcAvailability TryGetOnlineText(in string url, out string str);
    }

    public interface INetworkEx : INetwork
    {
        [System.Obsolete("Use to `ClearDownloadCacheAll` instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        void ClearDownloadCache();

        /// <summary>
        /// オンライン画像リソースを描画します
        /// </summary>
        /// <param name="url">リソースURL</param>
        /// <returns>オンラインリソースの可用性</returns>
        GcAvailability DrawOnlineImage(in string url);

        /// <summary>
        /// オンライン画像リソースを描画します
        /// </summary>
        /// <param name="url">リソースURL</param>
        /// <param name="position">位置</param>
        /// <param name="degree">回転（度数法）</param>
        /// <returns>オンラインリソースの可用性</returns>
        GcAvailability DrawOnlineImage(in string url, in float2 position, float degree = 0f);

        /// <summary>
        /// オンライン画像リソースを描画します
        /// </summary>
        /// <param name="url">リソースURL</param>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="degree">回転（度数法）</param>
        /// <returns>オンラインリソースの可用性</returns>
        GcAvailability DrawOnlineImage(in string url, in float x, in float y, float degree = 0f);

        /// <summary>
        /// オンライン画像リソースを拡縮して描画します
        /// </summary>
        /// <param name="url">リソースURL</param>
        /// <param name="rect">画像をフィッティングする矩形領域</param>
        GcAvailability DrawOnlineImage(in string url, in GcRect rect);

        /// <summary>
        /// オンライン画像リソースを拡縮して描画します
        /// </summary>
        /// <param name="url">リソースURL</param>
        /// <param name="x">X座標</param>
        /// <param name="y">Y座標</param>
        /// <param name="width">横幅。画像の横幅がこれになるように拡縮される</param>
        /// <param name="height">縦幅。画像の縦幅がこれになるように拡縮される</param>
        /// <param name="degree">回転（度数法）</param>
        GcAvailability DrawOnlineImage(in string url, in float x, in float y, in float width, in float height, float degree = 0f);

        /// <summary>
        /// オンライン画像リソースの縦幅を取得します
        /// </summary>
        /// <param name="url">リソースURL</param>
        /// <returns>オンライン画像リソースの縦幅。取得できなかった場合は0を返します</returns>
        int GetOnlineImageHeight(in string url);

        /// <summary>
        /// オンライン画像リソースの横幅を取得します
        /// </summary>
        /// <param name="url">リソースURL</param>
        /// <returns>オンライン画像リソースの横幅。取得できなかった場合は0を返します</returns>
        int GetOnlineImageWidth(in string url);

        [System.Obsolete("Use to `TryGetOnlineText`  instead.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        GcAvailability GetOnlineTextAsync(in string url, out string str);
    }
}
