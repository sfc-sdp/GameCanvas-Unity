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

using UnityEngine.InputSystem;

namespace GameCanvas
{
    /// <summary>
    /// <see cref="GcPointerEvent"/> 段階
    /// </summary>
    public enum GcPointerEventPhase : byte
    {
        /// <summary>
        /// 不正な値
        /// </summary>
        Invalid,
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

    static class UnityTouchPhaseExtension
    {
        public static GcPointerEventPhase ToGcPointerPhase(this TouchPhase self)
        {
            return self switch
            {
                TouchPhase.Began => GcPointerEventPhase.Begin,
                TouchPhase.Moved => GcPointerEventPhase.Hold,
                TouchPhase.Stationary => GcPointerEventPhase.Hold,
                TouchPhase.Ended => GcPointerEventPhase.End,
                TouchPhase.Canceled => GcPointerEventPhase.End,
                _ => GcPointerEventPhase.Invalid,
            };
        }
    }
}
