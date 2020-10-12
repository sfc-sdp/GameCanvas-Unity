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
    /// ポインターイベント
    /// </summary>
    public struct GcPointerEvent
        : System.IEquatable<GcPointerEvent>, System.IComparable<GcPointerEvent>
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        public static readonly GcPointerEvent Null = default;

        /// <summary>
        /// フレーム番号
        /// </summary>
        public int Frame;

        /// <summary>
        /// 識別子
        /// </summary>
        public int Id;

        /// <summary>
        /// 段階
        /// </summary>
        public GcPointerEventPhase Phase;

        /// <summary>
        /// 位置（キャンバス座標）
        /// </summary>
        public float2 Point;

        /// <summary>
        /// 位置（端末スクリーン座標）
        /// </summary>
        public float2 PointScreen;

        /// <summary>
        /// 圧力（検知できない場合は1）
        /// </summary>
        public float Pressure;

        /// <summary>
        /// 検知可能な最大圧力（検知できない場合は1）
        /// </summary>
        public float PressureMax;

        /// <summary>
        /// 傾き（X-Z面）
        /// </summary>
        public float TiltX;

        /// <summary>
        /// 傾き（Y-Z面）
        /// </summary>
        public float TiltY;

        /// <summary>
        /// 時間（起動からの経過秒数）
        /// </summary>
        public float Time;

        /// <summary>
        /// 種別
        /// </summary>
        public GcPointerType Type;
        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        public static bool operator !=(GcPointerEvent lh, GcPointerEvent rh) => !lh.Equals(rh);

        public static bool operator ==(GcPointerEvent lh, GcPointerEvent rh) => lh.Equals(rh);

        public int CompareTo(GcPointerEvent other) => Time.CompareTo(other.Time);

        public bool Equals(GcPointerEvent other)
                    => Id == other.Id
            && Phase == other.Phase
            && Type == other.Type
            && GcMath.AlmostSame(PointScreen, other.PointScreen)
            && GcMath.AlmostSame(Time, other.Time);

        public override bool Equals(object obj) => (obj is GcPointerEvent other) && Equals(other);

        public override int GetHashCode()
            => Id ^ (int)Phase ^ (int)Type ^ PointScreen.GetHashCode() ^ Time.GetHashCode();

        public override string ToString()
            => $"{nameof(GcPointerEvent)}: {{ x: {Point.x:0.0}, y: {Point.y:0.0}, frame: {Frame}, phase: {Phase} }}";
        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        internal GcPointerEvent(in GcContext ctx, in int id, in float2 screen, in GcPointerEventPhase phase, in GcPointerType type)
        {
            Id = id;
            ctx.Graphics.ScreenToCanvasPoint(screen, out Point);
            PointScreen = screen;
            Phase = phase;
            Type = type;
            TiltX = 0f;
            TiltY = 0f;
            Pressure = 1f;
            PressureMax = 1f;
            Frame = ctx.Time.CurrentFrame;
            Time = ctx.Time.TimeSinceStartup;
        }

        internal GcPointerEvent(in GcContext ctx, in Touch touch)
        {
            Id = touch.fingerId;
            ctx.Graphics.ScreenToCanvasPoint(touch.position, out Point);
            PointScreen = (int2)(math.round(touch.position));
            Phase = touch.phase == TouchPhase.Began ? GcPointerEventPhase.Begin
                : touch.phase == TouchPhase.Ended ? GcPointerEventPhase.End
                : GcPointerEventPhase.Hold;
            Type = touch.type == TouchType.Direct ? GcPointerType.Touch
                : touch.type == TouchType.Stylus ? GcPointerType.Stylus
                : GcPointerType.Others;
            TiltX = touch.azimuthAngle * Mathf.Deg2Rad;
            TiltY = touch.altitudeAngle * Mathf.Deg2Rad;
            Pressure = touch.pressure;
            PressureMax = touch.maximumPossiblePressure;
            Frame = ctx.Time.CurrentFrame;
            Time = ctx.Time.TimeSinceStartup;
        }
        #endregion
    }
}
