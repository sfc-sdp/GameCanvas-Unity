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
    /// ポインター種別
    /// </summary>
    public enum GcPointerType : byte
    {
        /// <summary>
        /// 不明
        /// </summary>
        Unknown,
        /// <summary>
        /// タッチ
        /// </summary>
        Touch,
        /// <summary>
        /// スタイラス
        /// </summary>
        Stylus,
        /// <summary>
        /// マウス、その他
        /// </summary>
        Others
    }
}
