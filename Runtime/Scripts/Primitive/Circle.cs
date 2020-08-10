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
    public readonly struct Circle : System.IEquatable<Circle>
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        public readonly float2 Center;
        public readonly float Radius;

        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        public static bool operator ==(Circle lh, Circle rh) => lh.Equals(rh);

        public static bool operator !=(Circle lh, Circle rh) => !lh.Equals(rh);

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="center">中心座標</param>
        /// <param name="radius">半径</param>
        public Circle(in float2 center, in float radius)
        {
            Center = center;
            Radius = radius;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="x">中心座標X</param>
        /// <param name="y">中心座標Y</param>
        /// <param name="radius">半径</param>
        public Circle(in float x, in float y, in float radius) : this(new float2(x, y), radius) { }

        /// <summary>
        /// 指定された点がこの円の領域内にあるかどうか
        /// </summary>
        /// <param name="point">点座標</param>
        public bool Contains(in float2 point)
        {
            return math.lengthsq(Center - point) <= (Radius * Radius);
        }

        /// <summary>
        /// 指定された円とこの円が重なり合っているかどうか
        /// </summary>
        public bool Overlaps(in Circle other)
        {
            var sum = Radius + other.Radius;
            return math.lengthsq(Center - other.Center) <= sum * sum;
        }

        public bool Equals(Circle other) => Center.Equals(other.Center) && Radius.Equals(other.Radius);

        public override bool Equals(object obj) => (obj is Circle other) && Equals(other);

        public override int GetHashCode() => Center.GetHashCode() ^ Radius.GetHashCode();

        public override string ToString() => $"Circle: {{ x: {Center.x}, y: {Center.y}, radius: {Radius} }}";

        #endregion
    }
}
