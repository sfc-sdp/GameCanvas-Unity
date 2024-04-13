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

        /// <summary>
        /// 矩形の指定された点の座標を計算します
        /// </summary>
        public readonly float2 GetPoint(in GcAnchor anchor)
        {
            return anchor switch
            {
                GcAnchor.UpperLeft => Center - HalfSize,
                GcAnchor.UpperCenter => Center + new float2(0f, -HalfSize.y),
                GcAnchor.UpperRight => Center + new float2(HalfSize.x, -HalfSize.y),
                GcAnchor.MiddleLeft => Center + new float2(-HalfSize.x, 0f),
                GcAnchor.MiddleCenter => Center,
                GcAnchor.MiddleRight => Center + new float2(HalfSize.x, 0f),
                GcAnchor.LowerLeft => Center + new float2(-HalfSize.x, HalfSize.y),
                GcAnchor.LowerCenter => Center + new float2(0f, HalfSize.y),
                GcAnchor.LowerRight => Center + HalfSize,
                _ => throw new System.ComponentModel.InvalidEnumArgumentException(nameof(anchor), (int)anchor, typeof(GcAnchor)),
            };
        }

        public static explicit operator GcAABB(Rect rect) => new(rect);

        public static explicit operator Rect(GcAABB aabb) => new(aabb.Position(), aabb.Size());

        public static implicit operator GcRect(GcAABB aabb) => new(aabb.Position(), aabb.Size());

        /// <summary>
        /// 2点を内包する最小の矩形
        /// </summary>
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

        public readonly bool Equals(GcAABB other)
            => Center.Equals(other.Center) && HalfSize.Equals(other.HalfSize);

        public override readonly bool Equals(object obj) => (obj is GcAABB other) && Equals(other);

        public override readonly int GetHashCode() => Center.GetHashCode() ^ HalfSize.GetHashCode();

        public override readonly string ToString()
                    => $"{nameof(GcAABB)}: {{ cx: {Center.x}, cy: {Center.y}, hw: {HalfSize.x}, hh: {HalfSize.y} }}";
        #endregion
    }
}
