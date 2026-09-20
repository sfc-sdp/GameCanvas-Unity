#nullable enable
using System;
using Unity.Mathematics;

namespace GameCanvas
{
    /// <summary>1個の矩形を動かす操作。毎フレームgc.Dragへ渡す。中断時は開始位置へ戻す。</summary>
    public sealed class GcDrag
    {
        public bool Active { get; private set; }
        public bool Started { get; private set; }
        public bool Ended { get; private set; }
        public bool Cancelled { get; private set; }
        public int? PointerId { get; private set; }
        GcPoint start, origin;
        /// <summary>操作を中断し、矩形を開始位置へ戻す。</summary>
        public void Cancel(ref GcRect rect)
        {
            Started = Ended = false; Cancelled = Active;
            if (Active) { rect.X = origin.X; rect.Y = origin.Y; }
            Active = false; PointerId = null;
        }
        internal void Update(GcReadOnlyList<GcPointer> pointers, ref GcRect rect, GcAnchor anchor, float2x3 coordinate)
        {
            Started = Ended = Cancelled = false;
            bool found = false;
            for (int i = 0; i < pointers.Count; i++)
            {
                var p = pointers[i];
                if (!Active && !Ended && !Cancelled && p.Down && !p.Cancelled &&
                    GcHitTest.Contains(rect, p.StartPosition, anchor, coordinate))
                {
                    Active = Started = true; PointerId = p.Id;
                    start = p.StartPosition; origin = new GcPoint(rect.X, rect.Y);
                }
                if (!Active || PointerId != p.Id) continue;
                found = true;
                if (p.Cancelled) { Cancel(ref rect); continue; }
                if (p.Held || p.Up)
                {
                    rect.X = origin.X + p.X - start.X; rect.Y = origin.Y + p.Y - start.Y;
                }
                if (p.Up) { Active = false; Ended = true; PointerId = null; }
            }
            if (Active && !found) Cancel(ref rect);
        }
    }
    internal static class GcHitTest
    {
        internal static bool Contains(in GcRect rect, in GcPoint point, GcAnchor anchor, in float2x3 coordinate)
        {
            if ((uint)anchor > (uint)GcAnchor.LowerRight) throw new ArgumentOutOfRangeException(nameof(anchor));
            // Same matrix order and anchor offset as FillRect / DrawTexture.
            var m = GcAffine.FromTRS(rect.Position, rect.Radian, rect.Size).Mul(coordinate);
            float det = m.c0.x * m.c1.y - m.c1.x * m.c0.y;
            if (!math.isfinite(det) || det == 0) return false;
            var p = new float2(point.X, point.Y) - m.c2;
            float x = (p.x * m.c1.y - p.y * m.c1.x) / det + ((int)anchor % 3) * .5f;
            float y = (p.y * m.c0.x - p.x * m.c0.y) / det + ((int)anchor / 3) * .5f;
            return x >= 0 && x < 1 && y >= 0 && y < 1;
        }
    }
}
