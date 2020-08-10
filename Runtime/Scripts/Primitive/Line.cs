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
    /// 2次元空間の直線
    /// </summary>
    public readonly struct Line : System.IEquatable<Line>
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        /// <summary>
        /// 原点
        /// </summary>
        public readonly float2 Origin;
        /// <summary>
        /// 正のX軸との間の平面上での角度（弧度法）
        /// </summary>
        public readonly float Radian;

        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        public static bool operator ==(Line lh, Line rh) => lh.Equals(rh);

        public static bool operator !=(Line lh, Line rh) => !lh.Equals(rh);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="origin">原点</param>
        /// <param name="radian">傾き（弧度法）</param>
        public Line(in float2 origin, in float radian)
        {
            Origin = origin;
            Radian = radian;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="origin">原点</param>
        /// <param name="direction">傾き（ベクトル）</param>
        public Line(in float2 origin, in float2 direction)
            : this(origin, Math.Atan2(direction)) { }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="ax">点AのX座標</param>
        /// <param name="ay">点AのY座標</param>
        /// <param name="bx">点BのX座標</param>
        /// <param name="by">点BのY座標</param>
        public Line(in float ax, in float ay, in float bx, in float by)
            : this(new float2(ax, ay), new float2(bx - ax, by - ay)) { }

        /// <summary>
        /// 点が直線上にあるかどうか
        /// </summary>
        /// <param name="point">点</param>
        /// <returns>点が直線上にあるかどうか</returns>
        public bool Contains(in float2 point)
        {
            return Math.AlmostZero(Math.Cross(Direction, point - Origin));
        }

        /// <summary>
        /// 指定した直線と交差しているかどうか
        /// </summary>
        /// <param name="other">判定する直線</param>
        /// <returns>交差しているかどうか</returns>
        public bool Intersects(in Line other)
        {
            return !Math.AlmostZero(Math.Cross(Direction, other.Direction));
        }

        /// <summary>
        /// 指定した直線との交点を求める
        /// </summary>
        /// <param name="other">判定する直線</param>
        /// <param name="intersection">交点</param>
        /// <returns>交差しているかどうか</returns>
        public bool Intersects(in Line other, out float2 intersection)
        {
            if (Intersects(other))
            {
                var v1 = Direction;
                var v2 = other.Direction;
                intersection = Origin + v1 * Math.Cross(v2, other.Origin - Origin) / Math.Cross(v2, v1);
                return true;
            }
            intersection = default;
            return false;
        }

        /// <summary>
        /// 直線と点の距離を求める
        /// </summary>
        /// <param name="point">点</param>
        /// <returns>距離</returns>
        public float Distance(in float2 point)
        {
            return Math.Abs(Math.Cross(Direction, point - Origin));
        }

        /// <summary>
        /// 指定した長さを持つ線分への変換
        /// </summary>
        /// <param name="length">線分の長さ</param>
        /// <returns>線分</returns>
        public Segment ToSegment(in float length = 1f)
        {
            return new Segment(Origin, Origin + Direction * length);
        }

        /// <summary>
        /// 正のX軸との間の平面上での角度（度数法）
        /// </summary>
        public float Degree => Radian * Mathf.Deg2Rad;

        /// <summary>
        /// 傾きを表す単位ベクトル
        /// </summary>
        public float2 Direction => new float2(Mathf.Cos(Radian), Mathf.Sin(Radian));
        
        public bool Equals(Line other) => Origin.Equals(other.Origin) && Radian.Equals(other.Radian);

        public override bool Equals(object obj) => (obj is Line other) && Equals(other);

        public override int GetHashCode() => Origin.GetHashCode() ^ Radian.GetHashCode();

        public override string ToString() => $"Line: {{ x: {Origin.x}, y: {Origin.y}, radian: {Radian} }}";

        #endregion
    }
}
