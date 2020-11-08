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
    /// 円
    /// </summary>
    public struct GcCircle : IPrimitive<GcCircle>
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        /// <summary>
        /// 位置（中心）
        /// </summary>
        public float2 Position;
        /// <summary>
        /// 半径
        /// </summary>
        public float Radius;

        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="position">位置（中心）</param>
        /// <param name="radius">半径</param>
        public GcCircle(in float2 position, in float radius)
        {
            Position = position;
            Radius = radius;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="x">中心座標X</param>
        /// <param name="y">中心座標Y</param>
        /// <param name="radius">半径</param>
        public GcCircle(in float x, in float y, in float radius)
        {
            Position = new float2(x, y);
            Radius = radius;
        }

        public static bool operator !=(GcCircle lh, GcCircle rh) => !lh.Equals(rh);

        public static bool operator ==(GcCircle lh, GcCircle rh) => lh.Equals(rh);

        public bool Equals(GcCircle other)
            => Position.Equals(other.Position) && Radius.Equals(other.Radius);

        public override bool Equals(object obj) => (obj is GcCircle other) && Equals(other);

        public override int GetHashCode() => Position.GetHashCode() ^ Radius.GetHashCode();

        public override string ToString()
            => $"{nameof(GcCircle)}: {{ x: {Position.x}, y: {Position.y}, radius: {Radius} }}";

        #endregion
    }
}
