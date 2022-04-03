/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2022 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
#nullable enable

namespace GameCanvas
{
    /// <summary>
    /// <see cref="GcKeyEvent"/> 段階
    /// </summary>
    public enum GcKeyEventPhase : byte
    {
        /// <summary>
        /// キーを押した瞬間
        /// </summary>
        Down,
        /// <summary>
        /// キーを押し続けている（押した瞬間を除く）
        /// </summary>
        Hold,
        /// <summary>
        /// キーを離した瞬間
        /// </summary>
        Up
    }
}
