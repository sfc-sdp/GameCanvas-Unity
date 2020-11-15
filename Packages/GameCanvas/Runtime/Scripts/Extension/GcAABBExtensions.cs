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
    /// <see cref="GcAABB"/> 拡張クラス
    /// </summary>
    public static class GcAABBExtensions
    {
        /// <summary>
        /// 領域内の最大座標（右下）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 Max(this in GcAABB self) => self.Center + self.HalfSize;

        /// <summary>
        /// 領域内の最小座標（左上。Positionに同じ）
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 Min(this in GcAABB self) => self.Center - self.HalfSize;

        /// <summary>
        /// 位置
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 Position(this in GcAABB self) => self.Center - self.HalfSize;

        /// <summary>
        /// 大きさ
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2 Size(this in GcAABB self) => self.HalfSize * 2f;
    }
}
