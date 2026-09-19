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
