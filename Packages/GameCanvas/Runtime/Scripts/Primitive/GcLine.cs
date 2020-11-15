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

namespace GameCanvas
{
    /// <summary>
    /// 直線もしくは線分
    /// </summary>
    public struct GcLine : IPrimitive<GcLine>
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        /// <summary>
        /// 傾きを表す単位ベクトル
        /// </summary>
        public float2 Direction;

        /// <summary>
        /// 線分の長さ
        /// </summary>
        public float Length;

        /// <summary>
        /// 基準点
        /// </summary>
        public float2 Origin;

        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ（線分）
        /// </summary>
        /// <param name="oridin">基準点</param>
        /// <param name="length">長さ</param>
        /// <param name="direction">傾きを表す単位ベクトル</param>
        public GcLine(in float2 oridin, in float length, in float2 direction)
        {
            Origin = oridin;
            Length = length;
            Direction = math.normalizesafe(direction);
        }

        /// <summary>
        /// コンストラクタ（線分）
        /// </summary>
        /// <param name="oridin">基準点</param>
        /// <param name="length">長さ</param>
        /// <param name="radian">傾き（弧度法）</param>
        public GcLine(in float2 oridin, in float length, in float radian)
        {
            Origin = oridin;
            Length = length;
            Direction = new float2(math.cos(radian), math.sin(radian));
        }

        /// <summary>
        /// コンストラクタ（直線）
        /// </summary>
        /// <param name="origin">基準点</param>
        /// <param name="radian">傾きを表す単位ベクトル</param>
        public GcLine(in float2 origin, in float2 direction)
        {
            Origin = origin;
            Length = float.PositiveInfinity;
            Direction = direction;
        }

        /// <summary>
        /// コンストラクタ（直線）
        /// </summary>
        /// <param name="origin">基準点</param>
        /// <param name="radian">傾き（弧度法）</param>
        public GcLine(in float2 origin, in float radian)
        {
            Origin = origin;
            Length = float.PositiveInfinity;
            Direction = new float2(math.cos(radian), math.sin(radian));
        }

        public static bool operator !=(GcLine lh, GcLine rh) => !lh.Equals(rh);

        public static bool operator ==(GcLine lh, GcLine rh) => lh.Equals(rh);

        /// <summary>
        /// コンストラクタ（線分）
        /// </summary>
        /// <param name="p0">始点</param>
        /// <param name="p1">終点</param>
        public static GcLine Segment(in float2 p0, in float2 p1)
            => new GcLine(p0, math.distance(p0, p1), math.normalizesafe(p1 - p0));

        public bool Equals(GcLine other)
            => Origin.Equals(other.Origin) && Length.Equals(other.Length) && Direction.Equals(other.Direction);

        public override bool Equals(object obj) => (obj is GcLine other) && Equals(other);

        public override int GetHashCode()
            => Origin.GetHashCode() ^ Length.GetHashCode() ^ Direction.GetHashCode();

        public override string ToString()
            => $"{nameof(GcLine)}: {{ Origin: ({Origin.x}, {Origin.y}), Angle: {this.Degree()}, Len: ({Length}) }}";

        #endregion
    }
}
