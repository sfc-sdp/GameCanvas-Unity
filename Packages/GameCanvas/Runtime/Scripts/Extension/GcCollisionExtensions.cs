/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace GameCanvas
{
    public static class GcCollisionExtensions
    {
        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        /// <summary>
        /// 線と点との距離を求めます
        /// </summary>
        /// <param name="self">線</param>
        /// <param name="point">点</param>
        /// <returns>線と点との距離</returns>
        public static float CalcDistance(this in GcLine self, in float2 point)
        {
            if (self.IsSegment())
            {
                // 線分と点の距離を求める
                var v1 = self.Vector();
                var v2 = point - self.Origin;
                if (GcMath.Dot(v1, v2) < math.EPSILON) return math.length(v2);
                var v3 = -v1;
                var v4 = point - (self.Origin + v1);
                if (GcMath.Dot(v3, v4) < math.EPSILON) return math.length(v4);
                return GcMath.Abs(GcMath.Cross(v1, v2)) / math.length(v1);
            }
            else
            {
                // 直線と点の距離を求める
                return GcMath.Abs(GcMath.Cross(self.Direction, point - self.Origin));
            }
        }

        /// <summary>
        /// 点が円と重なっているかどうかを調べます
        /// </summary>
        /// <param name="self">円</param>
        /// <param name="point">点</param>
        /// <returns>重なっているかどうか</returns>
        public static bool Contains(this in GcCircle self, in float2 point)
        {
            return math.lengthsq(self.Position - point) <= (self.Radius * self.Radius);
        }

        /// <summary>
        /// 点が矩形と重なっているかどうかを調べます
        /// </summary>
        /// <param name="self">矩形</param>
        /// <param name="point">点</param>
        /// <returns>重なっているかどうか</returns>
        public static bool Contains(this in GcAABB self, in float2 point)
        {
            var d = point - self.Center;
            var p = self.HalfSize - math.abs(d);
            return (p.x > -math.EPSILON) && (p.y > -math.EPSILON);
        }

        /// <summary>
        /// 点が線上に存在するかどうかを調べます
        /// </summary>
        /// <param name="self">線</param>
        /// <param name="point">点</param>
        /// <returns>存在するかどうか</returns>
        public static bool Contains(this in GcLine self, in float2 point)
        {
            if (self.IsSegment())
            {
                var d1 = self.Length * self.Length;
                var d2 = math.lengthsq(point - self.Origin);
                var d3 = math.lengthsq(point - self.End());
                return (d2 + d3 < d1 + math.EPSILON);
            }
            else
            {
                return GcMath.AlmostZero(GcMath.Cross(self.Direction, point - self.Origin));
            }
        }

        /// <summary>
        /// 点と矩形の衝突判定を行い、詳細な衝突点情報を計算します
        /// </summary>
        /// <param name="self">矩形</param>
        /// <param name="point">点</param>
        /// <param name="hit">衝突点情報</param>
        /// <returns>衝突しているかどうか</returns>
        public static bool HitTest(this in GcAABB self, in float2 point, out GcHitResult hit)
        {
            var d = point - self.Center;
            var p = self.HalfSize - math.abs(d);
            if (p.x > -math.EPSILON && p.y > -math.EPSILON)
            {
                var sign = (p.x < p.y)
                    ? new float2(math.sign(d.x), 0f)
                    : new float2(0f, math.sign(d.y));
                var pos = self.Center + self.HalfSize * sign;
                var inv = p * sign;
                hit = new GcHitResult(pos, sign, inv);
                return true;
            }
            hit = default;
            return false;
        }

        /// <summary>
        /// 矩形同士の衝突判定を行い、詳細な衝突点情報を計算します
        /// </summary>
        /// <param name="self">矩形1</param>
        /// <param name="other">矩形2</param>
        /// <param name="hit">衝突点情報</param>
        /// <returns>衝突しているかどうか</returns>
        public static bool HitTest(this in GcAABB self, GcAABB other, out GcHitResult hit)
        {
            var d = other.Center - self.Center;
            var p = other.HalfSize + self.HalfSize - math.abs(d);
            if (p.x > -math.EPSILON && p.y > -math.EPSILON)
            {
                float sign;
                float2 hitNrm, hitPos, inv;
                if (p.x < p.y)
                {
                    sign = math.sign(d.x);
                    inv = new float2(p.x * sign, 0);
                    hitNrm = new float2(sign, 0);
                    hitPos = new float2(self.Center.x + self.HalfSize.x * sign, other.Center.y);
                }
                else
                {
                    sign = math.sign(d.y);
                    inv = new float2(0, p.y * sign);
                    hitNrm = new float2(0, sign);
                    hitPos = new float2(other.Center.x, self.Center.y + self.HalfSize.y * sign);
                }
                hit = new GcHitResult(hitPos, hitNrm, inv);
                return true;
            }
            hit = default;
            return false;
        }

        /// <summary>
        /// 線同士が交差しているかどうかを調べます
        /// </summary>
        /// <param name="self">線1</param>
        /// <param name="other">線2</param>
        /// <returns>交差しているかどうか</returns>
        public static bool Intersects(this in GcLine self, in GcLine other)
        {
            var pattern = (self.IsSegment() ? 0b10 : 0) | (other.IsSegment() ? 0b01 : 0);

            switch (pattern)
            {
                case 0b11:
                    // 線分同士の交差判定
                    var v0 = self.Vector();
                    var v1 = other.Vector();
                    var e0 = self.Origin + v0;
                    var e1 = other.Origin + v1;
                    return (GcMath.Cross(v0, other.Origin - self.Origin) * GcMath.Cross(v0, e1 - self.Origin) < math.EPSILON)
                        && (GcMath.Cross(v1, self.Origin - other.Origin) * GcMath.Cross(v1, e0 - other.Origin) < math.EPSILON);

                case 0b01:
                    // 直線と線分の交差判定
                    return IntersectsLS(self, other);

                case 0b10:
                    // 線分と直線の交差判定
                    return IntersectsLS(other, self);

                case 0b00:
                    // 直線同士の交差判定
                    return !GcMath.AlmostZero(GcMath.Cross(self.Direction, other.Direction));
            }
            throw new System.NotImplementedException();

            bool IntersectsLS(in GcLine line, in GcLine segment)
            {
                var d = line.Direction;
                return GcMath.Cross(segment.Origin - line.Origin, d) * GcMath.Cross(segment.End() - line.Origin, d) < math.EPSILON;
            }
        }

        /// <summary>
        /// 線同士が交差しているかどうかを調べます
        /// </summary>
        /// <param name="self">線1</param>
        /// <param name="other">線2</param>
        /// <param name="intersection">交差座標</param>
        /// <returns>交差しているかどうか</returns>
        public static bool Intersects(this in GcLine self, in GcLine other, out float2 intersection)
        {
            var pattern = (self.IsSegment() ? 0b10 : 0) | (other.IsSegment() ? 0b01 : 0);

            switch (pattern)
            {
                case 0b11:
                    // 線分同士の交差判定
                    if (self.Intersects(other))
                    {
                        var v0 = self.Vector();
                        var v1 = other.Vector();
                        var e0 = self.Origin + v0;
                        var d1 = GcMath.Abs(GcMath.Cross(v1, self.Origin - other.Origin));
                        var d2 = GcMath.Abs(GcMath.Cross(v1, e0 - other.Origin));
                        var t = d1 / (d1 + d2);
                        intersection = self.Origin + v0 * t;
                        return true;
                    }
                    intersection = default;
                    return false;

                case 0b01:
                    // 直線と線分の交差判定
                    return IntersectsLS(self, other, out intersection);

                case 0b10:
                    // 線分と直線の交差判定
                    return IntersectsLS(other, self, out intersection);

                case 0b00:
                    // 直線同士の交差判定
                    if (self.Intersects(other))
                    {
                        intersection = self.Origin + self.Direction * GcMath.Cross(other.Direction, other.Origin - self.Origin) / GcMath.Cross(other.Direction, self.Direction);
                        return true;
                    }
                    intersection = default;
                    return false;
            }
            throw new System.NotImplementedException();

            bool IntersectsLS(in GcLine line, in GcLine segment, out float2 result)
            {
                if (segment.Intersects(line))
                {
                    var dir = line.Direction;
                    var v = segment.Vector();
                    var e = segment.Origin + v;
                    var d1 = GcMath.Abs(GcMath.Cross(dir, segment.Origin - line.Origin));
                    var d2 = GcMath.Abs(GcMath.Cross(dir, e - line.Origin));
                    var t = d1 / (d1 + d2);
                    result = segment.Origin + v * t;
                    return true;
                }
                result = default;
                return false;
            }
        }

        /// <summary>
        /// 矩形同士が重なっているかどうかを調べます
        /// </summary>
        /// <param name="self">矩形1</param>
        /// <param name="other">矩形2</param>
        /// <returns>重なっているかどうか</returns>
        public static bool Overlaps(this in GcAABB self, in GcAABB other)
        {
            var d = other.Center - self.Center;
            var p = other.HalfSize + self.HalfSize - math.abs(d);
            return (p.x > -math.EPSILON && p.y > -math.EPSILON);
        }

        /// <summary>
        /// 円同士が重なっているかどうかを調べます
        /// </summary>
        /// <param name="self">円1</param>
        /// <param name="other">円2</param>
        /// <returns>重なっているかどうか</returns>
        public static bool Overlaps(this in GcCircle self, in GcCircle other)
        {
            var sum = self.Radius + other.Radius;
            return math.lengthsq(self.Position - other.Position) <= sum * sum;
        }

        /// <summary>
        /// 移動量を考慮した 矩形と点の衝突判定を行います
        /// </summary>
        /// <param name="static">静止している矩形</param>
        /// <param name="dynamic">移動する点</param>
        /// <param name="delta">点の移動量</param>
        /// <param name="sweep">衝突情報</param>
        /// <returns>衝突するかどうか</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool SweepTest(this in GcAABB @static, in float2 @dynamic, in float2 delta, out GcSweepResult sweep)
            => SweepTest(@static, @dynamic, delta, float2.zero, out sweep);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static bool SweepTest(this in GcAABB @static, in float2 @dynamic, in float2 delta, in float2 padding, out GcSweepResult sweep)
        {
            var scale = 1f / delta;
            var sign = math.sign(delta);
            var nearRatio = (@static.Center - sign * (@static.HalfSize + padding) - @dynamic) * scale;
            var farRatio = (@static.Center + sign * (@static.HalfSize + padding) - @dynamic) * scale;
            if (nearRatio.x <= farRatio.y && nearRatio.y <= farRatio.x)
            {
                var nearRatioMax = math.max(nearRatio.x, nearRatio.y);
                var farRatioMin = math.min(farRatio.x, farRatio.y);
                if (nearRatioMax < 1 && farRatioMin > 0)
                {
                    ref var ratio = ref nearRatioMax;
                    var nrm = (nearRatio.x > nearRatio.y)
                        ? new float2(-sign.x, 0)
                        : new float2(0f, -sign.y);
                    var toHit = delta * ratio;
                    var onHit = @dynamic + toHit;
                    sweep = new GcSweepResult(delta, onHit, nrm, toHit, onHit, ratio);
                    return true;
                }
            }
            sweep = default;
            return false;
        }

        /// <summary>
        /// 移動量を考慮した 矩形同士の衝突判定を行います
        /// </summary>
        /// <param name="static">静止している矩形</param>
        /// <param name="dynamic">移動する矩形</param>
        /// <param name="delta">矩形の移動量</param>
        /// <param name="sweep">衝突情報</param>
        /// <returns>衝突するかどうか</returns>
        public static bool SweepTest(this in GcAABB @static, in GcAABB @dynamic, in float2 delta, out GcSweepResult sweep)
        {
            if (GcMath.AlmostZero(delta.x) && GcMath.AlmostZero(delta.y))
            {
                var ret = @static.HitTest(@dynamic, out var hit);
                var center = @dynamic.Center + hit.SinkVecInv;
                sweep = ret ? new GcSweepResult(delta, hit, float2.zero, center, 0f) : default;
                return ret;
            }
            else if (@static.SweepTest(@dynamic.Center, delta, @dynamic.HalfSize, out sweep))
            {
                var dir = math.normalize(delta);
                var hitPos = math.clamp(sweep.HitPoint + dir * @dynamic.HalfSize, @static.Min(), @static.Max());
                sweep = new GcSweepResult(delta, hitPos, sweep.HitNormal, sweep.VectorToHit, sweep.PositionOnHit, sweep.SweepRatioOnHit);
                return true;
            }
            sweep = default;
            return false;
        }
        #endregion
    }
}
