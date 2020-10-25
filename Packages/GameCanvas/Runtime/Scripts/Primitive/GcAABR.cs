/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using UnityEngine;
using Unity.Mathematics;
using System.Runtime.CompilerServices;

namespace GameCanvas
{
    /// <summary>
    /// 軸に平行な矩形 (Axis Aligned Bounding Box)
    /// </summary>
    public struct GcAABB : IPrimitive<GcAABB>
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        /// <summary>
        /// 中心座標
        /// </summary>
        public float2 Center;
        /// <summary>
        /// 大きさの半値
        /// </summary>
        public float2 HalfSize;

        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        public GcAABB(in float2 center, in float2 halfSize)
        {
            Center = center;
            HalfSize = math.abs(halfSize);
        }

        public GcAABB(in Rect rect)
        {
            Center = rect.center;
            HalfSize = rect.size * 0.5f;
        }

        public static explicit operator GcAABB(Rect rect) => new GcAABB(rect);

        public static explicit operator Rect(GcAABB aabb) => new Rect(aabb.Position(), aabb.Size());

        public static implicit operator GcRect(GcAABB aabb) => new GcRect(aabb.Position(), aabb.Size());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GcAABB MinMax(in float2 a, in float2 b)
        {
            var min = math.min(a, b);
            var max = math.max(a, b);
            return new GcAABB((min + max) * 0.5f, max - min);
        }

        public static bool operator !=(GcAABB lh, GcAABB rh) => !lh.Equals(rh);

        public static bool operator ==(GcAABB lh, GcAABB rh) => lh.Equals(rh);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GcAABB WH(in float2 size)
        {
            var center = size * 0.5f;
            var halfSize = math.abs(center);
            return new GcAABB(center, halfSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GcAABB XYWH(in float x, in float y, in float width, in float height)
        {
            var halfSize = math.abs(new float2(width, height) * 0.5f);
            return new GcAABB(new float2(x, y) + halfSize, halfSize);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GcAABB XYWH(in float2 position, in float2 size)
        {
            var halfSize = math.abs(size * 0.5f);
            return new GcAABB(position + halfSize, halfSize);
        }

        public bool Equals(GcAABB other)
            => Center.Equals(other.Center) && HalfSize.Equals(other.HalfSize);

        public override bool Equals(object obj) => (obj is GcAABB other) && Equals(other);

        public override int GetHashCode() => Center.GetHashCode() ^ HalfSize.GetHashCode();

        public override string ToString()
                    => $"{nameof(GcAABB)}: {{ cx: {Center.x}, cy: {Center.y}, hw: {HalfSize.x}, hh: {HalfSize.y} }}";
        #endregion
    }

    /// <summary>
    /// <see cref="GcAABB"/> 拡張クラス
    /// </summary>
    public static class GcAABBExtension
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
