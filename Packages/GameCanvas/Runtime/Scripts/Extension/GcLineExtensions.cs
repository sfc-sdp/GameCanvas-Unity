/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace GameCanvas
{
    /// <summary>
    /// <see cref="GcLine"/> 拡張クラス
    /// </summary>
    public static class GcLineExtensions
    {
        /// <summary>
        /// AABB
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GcAABB AABB(this in GcLine self, in float2x3 matrix)
            => GcAABB.MinMax(matrix.Mul(self.Origin), matrix.Mul(self.End()));

        /// <summary>
        /// 始点（線分の場合）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 Begin(this in GcLine self) => self.Origin;

        /// <summary>
        /// 傾き（度数法）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Degree(this in GcLine self)
            => math.degrees(math.atan2(self.Direction.y, self.Direction.x));

        /// <summary>
        /// 終点（線分の場合）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 End(this in GcLine self)
            => self.Origin + self.Length * self.Direction;

        /// <summary>
        /// 線分（長さが有限）かどうか
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSegment(this in GcLine self)
            => !float.IsInfinity(self.Length);

        /// <summary>
        /// 長さがないかどうか
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsZero(this in GcLine self)
            => GcMath.AlmostZero(self.Length);

        /// <summary>
        /// 傾き（弧度法）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Radian(this in GcLine self)
            => math.atan2(self.Direction.y, self.Direction.x);

        /// <summary>
        /// ベクトル（線分の場合）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 Vector(this in GcLine self)
            => self.Length * self.Direction;
    }
}
