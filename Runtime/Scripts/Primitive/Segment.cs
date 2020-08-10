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
    /// 2次元空間の線分
    /// </summary>
    public readonly struct Segment : System.IEquatable<Segment>
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        /// <summary>
        /// 点A
        /// </summary>
        public readonly float2 A;
        /// <summary>
        /// 点B
        /// </summary>
        public readonly float2 B;

        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        public static bool operator ==(Segment lh, Segment rh) => lh.Equals(rh);

        public static bool operator !=(Segment lh, Segment rh) => !lh.Equals(rh);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="a">点A</param>
        /// <param name="b">点B</param>
        public Segment(in float2 a, in float2 b)
        {
            A = a;
            B = b;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="ax">点AのX座標</param>
        /// <param name="ay">点AのY座標</param>
        /// <param name="bx">点BのX座標</param>
        /// <param name="by">点BのY座標</param>
        public Segment(in float ax, in float ay, in float bx, in float by)
            : this(new float2(ax, ay), new float2(bx, by)) { }

        /// <summary>
        /// 点が線分上にあるかどうか
        /// </summary>
        /// <remarks>
        /// 三角不等式を用いて判定
        /// </remarks>
        /// <param name="point">点</param>
        /// <returns>点が直線上にあるかどうか</returns>
        public bool Contains(in float2 point)
        {
            var d1 = math.lengthsq(B - A);
            var d2 = math.lengthsq(point - A);
            var d3 = math.lengthsq(point - B);
            return (d2 + d3 < d1 + float.Epsilon);
        }

        /// <summary>
        /// 指定した線分と交差しているかどうか
        /// </summary>
        /// <param name="other">判定する線分</param>
        /// <returns>交差しているかどうか</returns>
        public bool Intersects(in Segment other)
        {
            return (Math.Cross(B - A, other.A - A) * Math.Cross(B - A, other.B - A) < math.EPSILON)
                && (Math.Cross(other.B - other.A, A - other.A) * Math.Cross(other.B - other.A, B - other.A) < math.EPSILON);
        }

        /// <summary>
        /// 指定した線分との交点を求める
        /// </summary>
        /// <param name="other">判定する線分</param>
        /// <param name="intersection">交点</param>
        /// <returns>交差しているかどうか</returns>
        public bool Intersects(in Segment other, out float2 intersection)
        {
            if (Intersects(other))
            {
                var v = other.B - other.A;
                var d1 = Math.Abs(Math.Cross(v, A - other.A));
                var d2 = Math.Abs(Math.Cross(v, B - other.A));
                var t = d1 / (d1 + d2);
                intersection = A + (B - A) * t;
                return true;
            }
            intersection = default;
            return false;
        }

        /// <summary>
        /// 指定した直線と交差しているかどうか
        /// </summary>
        /// <param name="line">判定する直線</param>
        /// <returns>交差しているかどうか</returns>
        public bool Intersects(in Line line)
        {
            var d = line.Direction;
            return Math.Cross(A - line.Origin, d) * Math.Cross(B - line.Origin, d) < math.EPSILON;
        }

        /// <summary>
        /// 指定した直線との交点を求める
        /// </summary>
        /// <param name="line">判定する直線</param>
        /// <param name="intersection">交点</param>
        /// <returns>交差しているかどうか</returns>
        public bool Intersects(in Line line, out float2 intersection)
        {
            if (Intersects(line))
            {
                var v = line.Direction;
                var d1 = Math.Abs(Math.Cross(v, A - line.Origin));
                var d2 = Math.Abs(Math.Cross(v, B - line.Origin));
                var t = d1 / (d1 + d2);
                intersection = A + (B - A) * t;
                return true;
            }
            intersection = default;
            return false;
        }

        /// <summary>
        /// 線分と点の距離を求める
        /// </summary>
        /// <param name="point">点</param>
        /// <returns>距離</returns>
        public float Distance(in float2 point)
        {
            var v1 = B - A;
            var v2 = point - A;
            if (Math.Dot(v1, v2) < math.EPSILON) return math.length(v2);
            var v3 = A - B;
            var v4 = point - B;
            if (Math.Dot(v3, v4) < math.EPSILON) return math.length(v4);
            return Math.Abs(Math.Cross(v1, v2)) / math.length(v1);
        }

        /// <summary>
        /// 長さ
        /// </summary>
        public float Length => math.distance(A, B);

        /// <summary>
        /// 長さの二乗
        /// </summary>
        public float LengthSqr => math.lengthsq(B - A);

        /// <summary>
        /// 傾き（点A基準の単位ベクトル）
        /// </summary>
        public float2 Direction => math.normalize(B - A);

        public bool Equals(Segment other) => A.Equals(other.A) && B.Equals(other.B);

        public override bool Equals(object obj) => (obj is Segment other) && Equals(other);

        public override int GetHashCode() => A.GetHashCode() ^ B.GetHashCode();

        public override string ToString() => $"Line: {{ A: ({A.x}, {A.y}), B: ({B.x}, {B.y}) }}";

        #endregion
    }
}
