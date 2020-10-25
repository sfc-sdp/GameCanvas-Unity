/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using Unity.Mathematics;
using UnityEngine;

namespace GameCanvas
{
    /// <summary>
    /// キーイベント
    /// </summary>
    public readonly struct GcKeyEvent : System.IEquatable<GcKeyEvent>, System.IComparable<GcKeyEvent>
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        public static readonly GcKeyEvent Null = default;

        /// <summary>
        /// フレーム番号
        /// </summary>
        public readonly int Frame;

        /// <summary>
        /// キーコード
        /// </summary>
        public readonly KeyCode KeyCode;

        /// <summary>
        /// 段階
        /// </summary>
        public readonly GcKeyEventPhase Phase;

        /// <summary>
        /// 時間（起動からの経過秒数）
        /// </summary>
        public readonly float Time;
        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        public static bool operator !=(GcKeyEvent lh, GcKeyEvent rh) => !lh.Equals(rh);

        public static bool operator ==(GcKeyEvent lh, GcKeyEvent rh) => lh.Equals(rh);

        public bool Equals(GcKeyEvent other)
        {
            return KeyCode == other.KeyCode
                && Phase == other.Phase
                && Frame == other.Frame;
        }

        public override bool Equals(object obj) => (obj is GcKeyEvent other) && Equals(other);

        public override int GetHashCode()
            => (int)KeyCode ^ Frame ^ (int)Phase;

        public override string ToString()
            => $"{nameof(GcKeyEvent)}: {{ key: {KeyCode}, frame: {Frame}, phase: {Phase} }}";

        public int CompareTo(GcKeyEvent other) => Time.CompareTo(other.Time);
        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        internal GcKeyEvent(in KeyCode key, in GcKeyEventPhase phase, in int frame, in float time)
        {
            KeyCode = key;
            Phase = phase;
            Frame = frame;
            Time = time;
        }
        #endregion
    }
}
