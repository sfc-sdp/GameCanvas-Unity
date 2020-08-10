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
    public readonly struct PointerEvent : System.IEquatable<PointerEvent>
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        /// <summary>
        /// 識別子
        /// </summary>
        public readonly int Id;
        /// <summary>
        /// スクリーン座標
        /// </summary>
        public readonly float2 Screen;
        /// <summary>
        /// キャンバス座標
        /// </summary>
        public readonly float2 Canvas;
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

        /// <summary>
        /// 状態
        /// </summary>
        public enum EPhase
        {
            Began,
            Moved,
            Stationary,
            Ended
        }

        /// <summary>
        /// 種類
        /// </summary>
        public enum EType
        {
            Touch,
            Stylus,
            Others
        }

        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        public static bool operator ==(PointerEvent lh, PointerEvent rh) => lh.Equals(rh);

        public static bool operator !=(PointerEvent lh, PointerEvent rh) => !lh.Equals(rh);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal PointerEvent(in Engine.Time time, in Engine.Graphic graphic, int id, float2 screen, EPhase phase, EType type, float tiltX = 0f, float tiltY = 0f, float pressure = 0f)
        {
            Id = id;
            Screen = screen;
            graphic.ScreenToCanvas(Screen, out Canvas);
            Phase = phase;
            Type = type;
            TiltX = tiltX;
            TiltY = tiltY;
            Pressure = pressure;
            Frame = time.FrameCount;
            Time = time.SinceStartup;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal PointerEvent(in Engine.Time time, in Engine.Graphic graphic, in Touch touch)
        {
            Id = touch.fingerId;
            Screen = new int2(Mathf.RoundToInt(touch.position.x), Mathf.RoundToInt(touch.position.y));
            graphic.ScreenToCanvas(Screen, out Canvas);
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
            Frame = time.FrameCount;
            Time = time.SinceStartup;
        }

        public bool Equals(PointerEvent other)
        {
            return Id == other.Id
                && Screen.Equals(other.Screen)
                && Phase == other.Phase
                && Type == other.Type
                && Pressure == other.Pressure
                && TiltX == other.TiltX
                && TiltY == other.TiltY;
        }

        public override bool Equals(object obj) => (obj is PointerEvent other) && Equals(other);

        public override int GetHashCode()
        {
            return Id.GetHashCode()
                & Screen.GetHashCode()
                & Phase.GetHashCode()
                & Type.GetHashCode()
                & TiltX.GetHashCode() & TiltY.GetHashCode()
                & Pressure.GetHashCode();
        }

        public override string ToString()
        {
            return (TiltX != 0f || TiltY != 0f)
                ? $"{Id}: point: ({Screen.x}, {Screen.y}), phase: {Phase}, tilt: ({TiltX}, {TiltY}), pressure: {Pressure}"
                : $"{Id}: point: ({Screen.x}, {Screen.y}), phase: {Phase}, pressure: {Pressure}";
        }

        #endregion
    }
}
