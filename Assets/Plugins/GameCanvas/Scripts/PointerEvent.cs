/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2017 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/

namespace GameCanvas
{
    using UnityEngine;

    public struct PointerEvent : System.IEquatable<PointerEvent>
    {
        //----------------------------------------------------------
        #region フィールド変数
        //----------------------------------------------------------

        /// <summary>
        /// 識別子
        /// </summary>
        public readonly int Id;
        /// <summary>
        /// X座標
        /// </summary>
        public readonly int X;
        /// <summary>
        /// Y座標
        /// </summary>
        public readonly int Y;
        /// <summary>
        /// 段階
        /// </summary>
        public readonly EPhase Phase;
        /// <summary>
        /// 種別
        /// </summary>
        public readonly EType Type;
        /// <summary>
        /// 圧力 (検知できない場合は1)
        /// </summary>
        public readonly float Pressure;
        /// <summary>
        /// 傾き (X-Z面)
        /// </summary>
        public readonly float TiltX;
        /// <summary>
        /// 傾き (Y-Z面)
        /// </summary>
        public readonly float TiltY;
        /// <summary>
        /// フレーム番号
        /// </summary>
        public readonly int Frame;
        /// <summary>
        /// 起動からの経過秒数
        /// </summary>
        public readonly float Time;

        #endregion

        //----------------------------------------------------------
        #region 列挙体
        //----------------------------------------------------------

        public enum EPhase
        {
            Began,
            Moved,
            Stationary,
            Ended
        }

        public enum EType
        {
            Touch,
            Stylus,
            Others
        }

        #endregion

        //----------------------------------------------------------
        #region パブリック関数
        //----------------------------------------------------------

        public static bool operator ==(PointerEvent lh, PointerEvent rh)
        {
            return lh.Id == rh.Id
                && lh.X == rh.X
                && lh.Y == rh.Y
                && lh.Phase == rh.Phase
                && lh.Type == rh.Type
                && lh.Pressure == rh.Pressure
                && lh.TiltX == rh.TiltX
                && lh.TiltY == rh.TiltY;
        }

        public static bool operator !=(PointerEvent lh, PointerEvent rh) => !(lh == rh);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PointerEvent(int id, int x, int y, EPhase phase, EType type, float tiltX = 0f, float tiltY = 0f, float pressure = 0f)
        {
            Id = id;
            X = x;
            Y = y;
            Phase = phase;
            Type = type;
            TiltX = tiltX;
            TiltY = tiltY;
            Pressure = pressure;
            Frame = UnityEngine.Time.frameCount;
            Time = UnityEngine.Time.realtimeSinceStartup;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public PointerEvent(ref Touch touch)
        {
            Id = touch.fingerId;
            X = (int)touch.position.x;
            Y = (int)touch.position.y;
            Phase = touch.phase == TouchPhase.Began ? EPhase.Began
                : touch.phase == TouchPhase.Moved ? EPhase.Moved
                : touch.phase == TouchPhase.Stationary ? EPhase.Stationary
                : EPhase.Ended;
            Type = touch.type == TouchType.Direct ? EType.Touch
                : touch.type == TouchType.Stylus ? EType.Stylus
                : EType.Others;
            TiltX = touch.azimuthAngle * Mathf.Deg2Rad;
            TiltY = touch.altitudeAngle * Mathf.Deg2Rad;
            Pressure = touch.pressure;
            Frame = UnityEngine.Time.frameCount;
            Time = UnityEngine.Time.unscaledTime;
        }

        public bool Equals(PointerEvent other) => (this == other);

        public override bool Equals(object obj) => (obj is PointerEvent && this == (PointerEvent)obj);

        public override int GetHashCode()
        {
            return Id.GetHashCode()
                & X.GetHashCode() & Y.GetHashCode()
                & Phase.GetHashCode() & Type.GetHashCode()
                & TiltX.GetHashCode() & TiltY.GetHashCode()
                & Pressure.GetHashCode();
        }

        public override string ToString()
        {
            return (TiltX != 0f || TiltY != 0f)
                ? $"{Id}: point: ({X}, {Y}), phase: {Phase}, tilt: ({TiltX}, {TiltY}), pressure: {Pressure}"
                : $"{Id}: point: ({X}, {Y}), phase: {Phase}, pressure: {Pressure}";
        }

        #endregion
    }
}
