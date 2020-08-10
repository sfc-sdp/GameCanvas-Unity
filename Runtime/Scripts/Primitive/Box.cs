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

namespace GameCanvas
{
    /// <summary>
    /// 矩形領域
    /// </summary>
    public readonly struct Box : System.IEquatable<Box>
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        /// <summary>
        /// 中心座標
        /// </summary>
        public readonly float2 Center;
        /// <summary>
        /// 大きさの半値
        /// </summary>
        public readonly float2 HalfSize;

        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        public static bool operator ==(Box lh, Box rh) => lh.Equals(rh);

        public static bool operator !=(Box lh, Box rh) => !lh.Equals(rh);

        public static explicit operator Box(Rect rect) => new Box(rect);

        public static explicit operator Rect(Box box) => box.ToRect();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Box(in float2 center, in float2 halfSize)
        {
            if (halfSize.x <= 0 || halfSize.y <= 0)
            {
                throw new System.ArgumentException("負の大きさです", nameof(halfSize));
            }

            Center = center;
            HalfSize = halfSize;
        }

        public Box(in Rect rect)
            : this(rect.center, rect.size * 0.5f) { }

        public Box(in Circle circle)
            : this(circle.Center, new float2(circle.Radius, circle.Radius)) { }

        public bool Contains(in float2 point)
        {
            var d = point - Center;
            var p = HalfSize - math.abs(d);
            return (p.x > -math.EPSILON) && (p.y > -math.EPSILON);
        }

        public bool Overlaps(in Box box)
        {
            var d = box.Center - Center;
            var p = box.HalfSize + HalfSize - math.abs(d);
            return (p.x > -math.EPSILON && p.y > -math.EPSILON);
        }

        public bool HitTest(in float2 point, out HitResult hit)
        {
            var d = point - Center;
            var p = HalfSize - math.abs(d);
            if (p.x > -math.EPSILON && p.y > -math.EPSILON)
            {
                var sign = (p.x < p.y)
                    ? new float2(math.sign(d.x), 0f)
                    : new float2(0f, math.sign(d.y));
                var pos = Center + HalfSize * sign;
                var inv = p * sign;
                hit = new HitResult(pos, sign, inv, inv);
                return true;
            }
            hit = default;
            return false;
        }

        public bool HitTest(in Segment segment, out HitResult hit)
            => SweepTest(segment.A, segment.B - segment.A, float2.zero, out hit);

        public bool HitTest(in Box box, out HitResult hit)
        {
            var d = box.Center - Center;
            var p = box.HalfSize + HalfSize - math.abs(d);
            if (p.x > -math.EPSILON && p.y > -math.EPSILON)
            {
                float sign;
                float2 hitNrm, hitPos, inv;
                if (p.x < p.y)
                {
                    sign = math.sign(d.x);
                    inv = new float2(p.x * sign, 0);
                    hitNrm = new float2(sign, 0);
                    hitPos = new float2(Center.x + HalfSize.x * sign, box.Center.y);
                }
                else
                {
                    sign = math.sign(d.y);
                    inv = new float2(0, p.y * sign);
                    hitNrm = new float2(0, sign);
                    hitPos = new float2(box.Center.x, Center.y + HalfSize.y * sign);
                }
                var pos = box.Center + inv;
                hit = new HitResult(hitPos, hitNrm, inv, float2.zero, pos, 1f);
                return true;
            }
            hit = default;
            return false;
        }

        public bool SweepTest(in float2 point, in float2 delta, out HitResult hit)
            => SweepTest(point, delta, float2.zero, out hit);

        public bool SweepTest(in float2 point, in float2 delta, in float2 padding, out HitResult hit)
        {
            var scale = 1f / delta;
            var sign = math.sign(scale);
            var nearRatio = (Center - sign * (HalfSize + padding) - point) * scale;
            var farRatio = (Center + sign * (HalfSize + padding) - point) * scale;
            if (nearRatio.x <= farRatio.y && nearRatio.y <= farRatio.x)
            {
                var nearRatioMax = math.max(nearRatio.x, nearRatio.y);
                var farRatioMin = math.min(farRatio.x, farRatio.y);
                if (nearRatioMax < 1 && farRatioMin > 0)
                {
                    var rt = math.clamp(nearRatioMax, 0f, 1f);
                    var nrm = (nearRatio.x > nearRatio.y)
                        ? new float2(-sign.x, 0)
                        : new float2(0f, -sign.y);
                    var inv = (1f - rt) * -delta;
                    var mod = delta + inv * math.abs(nrm);
                    var pos = Center + delta * rt;
                    hit = new HitResult(pos, nrm, inv, mod, pos, rt);
                    return true;
                }
            }
            hit = default;
            return false;
        }

        public bool SweepTest(in Box box, in float2 delta, out HitResult sweep)
        {
            if (Math.AlmostZero(delta.x) && Math.AlmostZero(delta.y))
            {
                return HitTest(box, out sweep);
            }
            if (SweepTest(box.Center, delta, box.HalfSize, out var hit))
            {
                var dir = math.normalize(delta);
                var pos = box.Center + delta * hit.SweepRatio;
                var hitPos = math.clamp(
                    hit.HitPosition + dir * box.HalfSize,
                    Center - HalfSize,
                    Center + HalfSize
                );
                sweep = new HitResult(hitPos, hit.HitNormal, hit.SinkVecInv, hit.ModifiedDelta, pos, hit.SweepRatio);
                return true;
            }
            sweep = default;
            return false;
        }

        public bool Equals(Box other)
            => Center.Equals(other.Center) && HalfSize.Equals(other.HalfSize);

        public override bool Equals(object obj) => (obj is Box other) && Equals(other);

        public override int GetHashCode() => Center.GetHashCode() ^ HalfSize.GetHashCode();

        public override string ToString()
            => $"Box: {{ cx: {Center.x}, cy: {Center.y}, hw: {HalfSize.x}, hh: {HalfSize.y} }}";

        public Rect ToRect() => new Rect(Center - HalfSize, HalfSize * 2);

        #endregion
    }
}
