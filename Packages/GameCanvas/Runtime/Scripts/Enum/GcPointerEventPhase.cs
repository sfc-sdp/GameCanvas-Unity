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
    /// <see cref="GcPointerEvent"/> 段階
    /// </summary>
    public enum GcPointerEventPhase : byte
    {
        /// <summary>
        /// タッチした瞬間
        /// </summary>
        Begin,
        /// <summary>
        /// タッチし続けている（タッチした瞬間を除く）
        /// </summary>
        Hold,
        /// <summary>
        /// 離した瞬間
        /// </summary>
        End
    }
}
