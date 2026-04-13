/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2024 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
#nullable enable
using Unity.Mathematics;
using UnityEngine.InputSystem.Controls;

namespace GameCanvas
{
    /// <summary>
    /// ポインターイベント
    /// </summary>
    public readonly struct GcPointerEvent
        : System.IEquatable<GcPointerEvent>, System.IComparable<GcPointerEvent>
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        public static readonly GcPointerEvent Null = default;

        /// <summary>
        /// フレーム番号
        /// </summary>
        public readonly int Frame;

        /// <summary>
        /// 識別子
        /// </summary>
        public readonly int Id;

        /// <summary>
        /// 段階
        /// </summary>
        public readonly GcPointerEventPhase Phase;

        /// <summary>
        /// 位置（キャンバス座標）
        /// </summary>
        public readonly float2 Point;

        /// <summary>
        /// 位置（端末スクリーン座標）
        /// </summary>
        public readonly float2 PointScreen;

        /// <summary>
        /// 時間（起動からの経過秒数）
        /// </summary>
        public readonly float Time;
        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        public static bool operator !=(GcPointerEvent lh, GcPointerEvent rh) => !lh.Equals(rh);

        public static bool operator ==(GcPointerEvent lh, GcPointerEvent rh) => lh.Equals(rh);

        public readonly int CompareTo(GcPointerEvent other) => Time.CompareTo(other.Time);

        public readonly bool Equals(GcPointerEvent other)
            => Id == other.Id
            && Phase == other.Phase
            && GcMath.AlmostSame(PointScreen, other.PointScreen)
            && GcMath.AlmostSame(Time, other.Time);

        public override readonly bool Equals(object obj) => (obj is GcPointerEvent other) && Equals(other);

        public override readonly int GetHashCode()
        {
            return System.HashCode.Combine(Id, Phase, PointScreen, Time);
        }

        public override readonly string ToString()
            => $"{nameof(GcPointerEvent)}: {{ x: {Point.x:0.0}, y: {Point.y:0.0}, frame: {Frame}, phase: {Phase} }}";
        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        internal GcPointerEvent(in int frame, in float time, in int id, in GcPointerEventPhase phase, in float2 point, in float2 pointScreen)
        {
            Time = time;
            Frame = frame;
            Id = id;
            Phase = phase;
            Point = point;
            PointScreen = pointScreen;
        }

        internal static GcPointerEvent FromTouch(in GcContext ctx, in TouchControl touch, in float time)
        {
            var frame = ctx.Time.CurrentFrame;
            var id = touch.touchId.ReadValue();
            var phase = touch.phase.ReadValue().ToGcPointerPhase();
            var pointScreen = touch.position.ReadValue();
            ctx.Graphics.ScreenToCanvasPoint(pointScreen, out float2 point);
            return new GcPointerEvent(frame, time, id, phase, point, pointScreen);
        }

        internal static GcPointerEvent FromTrace(in GcContext ctx, in GcPointerTrace trace)
        {
            var frame = ctx.Time.CurrentFrame;
            var time = ctx.Time.TimeSinceStartup;
            var id = trace.Current.Id;
            var phase = GcPointerEventPhase.Hold;
            var point = trace.Current.Point;
            var pointScreen = trace.Current.PointScreen;
            return new GcPointerEvent(frame, time, id, phase, point, pointScreen);
        }
        #endregion
    }
}
