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
        public readonly Key Key;

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

        public int CompareTo(GcKeyEvent other) => Time.CompareTo(other.Time);

        public bool Equals(GcKeyEvent other)
            => Key == other.Key
            && Phase == other.Phase
            && Frame == other.Frame;

        public override bool Equals(object obj) => (obj is GcKeyEvent other) && Equals(other);

        public override int GetHashCode()
        {
            int hashCode = -756422210;
            hashCode = hashCode * -1521134295 + Frame.GetHashCode();
            hashCode = hashCode * -1521134295 + Key.GetHashCode();
            hashCode = hashCode * -1521134295 + Phase.GetHashCode();
            hashCode = hashCode * -1521134295 + Time.GetHashCode();
            return hashCode;
        }

        public override string ToString()
                    => $"{nameof(GcKeyEvent)}: {{ key: {Key}, frame: {Frame}, phase: {Phase} }}";
        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        internal GcKeyEvent(in Key key, in GcKeyEventPhase phase, in int frame, in float time)
        {
            Key = key;
            Phase = phase;
            Frame = frame;
            Time = time;
        }
        #endregion
    }
}
