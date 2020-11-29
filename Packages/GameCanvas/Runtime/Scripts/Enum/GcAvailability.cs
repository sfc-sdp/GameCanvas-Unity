/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
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
        Null,
        /// <summary>
        /// 利用不可
        /// </summary>
        NotAvailable,
        /// <summary>
        /// 準備中
        /// </summary>
        NotReady,
        /// <summary>
        /// いつでも利用可能
        /// </summary>
        Ready
    }
}
