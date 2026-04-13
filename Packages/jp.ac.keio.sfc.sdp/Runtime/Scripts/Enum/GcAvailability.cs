/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2026 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
#nullable enable

namespace GameCanvas
{
    /// <summary>
    /// 可用性（ダウンロード状態）
    /// </summary>
    public enum GcAvailability
    {
        /// <summary>
        /// 未初期化
        /// </summary>
        /// <remarks>
        /// v7.x まで Enum.ToString() の結果を "Null" のまま維持するため、Null を先に
        /// 宣言する (列挙値の先頭が ToString の正規名になる)。v8.0 で除去予定。
        /// </remarks>
        [System.Obsolete("Use Uninitialized instead. Will be removed in v8.0.")]
        Null = 0,
        /// <summary>
        /// 未初期化 (Null の新しい推奨名)
        /// </summary>
        Uninitialized = 0,
        /// <summary>
        /// 利用不可
        /// </summary>
        NotAvailable = 1,
        /// <summary>
        /// 準備中
        /// </summary>
        NotReady = 2,
        /// <summary>
        /// いつでも利用可能
        /// </summary>
        Ready = 3
    }
}
