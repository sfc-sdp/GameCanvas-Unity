#nullable enable
using NUnit.Framework;
using Unity.Mathematics;
using GameCanvas;

namespace GameCanvas.Editor.Tests
{
    public class CollisionTest
    {
        const float Delta = 0.001f;

        // ============================================================
        // GcAABB.Contains(float2)
        // ============================================================

        [Test]
        public void AABB_Contains_PointInside()
        {
            var box = new GcAABB(new float2(0, 0), new float2(5, 5));
            Assert.IsTrue(box.Contains(new float2(2, 3)));
        }

        [Test]
        public void AABB_Contains_PointOnEdge()
        {
            var box = new GcAABB(new float2(0, 0), new float2(5, 5));
            Assert.IsTrue(box.Contains(new float2(5, 0)));
            Assert.IsTrue(box.Contains(new float2(0, 5)));
            Assert.IsTrue(box.Contains(new float2(-5, -5)));
        }

        [Test]
        public void AABB_Contains_PointOutside()
        {
            var box = new GcAABB(new float2(0, 0), new float2(5, 5));
            Assert.IsFalse(box.Contains(new float2(6, 0)));
            Assert.IsFalse(box.Contains(new float2(0, 6)));
            Assert.IsFalse(box.Contains(new float2(10, 10)));
        }

        [Test]
        public void AABB_Contains_PointAtCenter()
        {
            var box = new GcAABB(new float2(3, 4), new float2(2, 2));
            Assert.IsTrue(box.Contains(new float2(3, 4)));
        }

        // ============================================================
        // GcCircle.Contains(float2)
        // ============================================================

        [Test]
        public void Circle_Contains_PointInside()
        {
            var circle = new GcCircle(new float2(0, 0), 5f);
            Assert.IsTrue(circle.Contains(new float2(1, 1)));
            Assert.IsTrue(circle.Contains(new float2(0, 0)));
        }

        [Test]
        public void Circle_Contains_PointOnBoundary()
        {
            var circle = new GcCircle(new float2(0, 0), 5f);
            Assert.IsTrue(circle.Contains(new float2(5, 0)));
            Assert.IsTrue(circle.Contains(new float2(0, 5)));
            Assert.IsTrue(circle.Contains(new float2(-5, 0)));
        }

        [Test]
        public void Circle_Contains_PointOutside()
        {
            var circle = new GcCircle(new float2(0, 0), 5f);
            Assert.IsFalse(circle.Contains(new float2(4, 4)));
            Assert.IsFalse(circle.Contains(new float2(6, 0)));
        }

        [Test]
        public void Circle_Contains_OffCenter()
        {
            var circle = new GcCircle(new float2(10, 10), 3f);
            Assert.IsTrue(circle.Contains(new float2(12, 10)));
            Assert.IsFalse(circle.Contains(new float2(14, 10)));
        }

        // ============================================================
        // GcLine.Contains(float2) — segment
        // ============================================================

        [Test]
        public void LineSegment_Contains_PointOnSegment()
        {
            var seg = GcLine.Segment(new float2(0, 0), new float2(10, 0));
            Assert.IsTrue(seg.Contains(new float2(5, 0)));
            Assert.IsTrue(seg.Contains(new float2(0, 0)));
            Assert.IsTrue(seg.Contains(new float2(10, 0)));
        }

        [Test]
        public void LineSegment_Contains_PointBeyondEnd()
        {
            var seg = GcLine.Segment(new float2(0, 0), new float2(10, 0));
            Assert.IsFalse(seg.Contains(new float2(11, 0)));
            Assert.IsFalse(seg.Contains(new float2(-1, 0)));
        }

        [Test]
        public void LineSegment_Contains_PointOffLine()
        {
            var seg = GcLine.Segment(new float2(0, 0), new float2(10, 0));
            // Contains uses d2+d3 < d1+eps on squared distances (ellipse check),
            // so small offsets near the midpoint are still "contained".
            // For (5,y): d2+d3 = 50+2y^2, d1 = 100, need y >= ~5 to be outside.
            Assert.IsFalse(seg.Contains(new float2(5, 6)));
            Assert.IsFalse(seg.Contains(new float2(5, -6)));
        }

        [Test]
        public void LineSegment_Contains_DiagonalSegment()
        {
            var seg = GcLine.Segment(new float2(0, 0), new float2(10, 10));
            Assert.IsTrue(seg.Contains(new float2(5, 5)));
            // For diagonal seg length^2 = 200, point (0,10): d2=100, d3=200 => 300 > 200
            Assert.IsFalse(seg.Contains(new float2(0, 10)));
        }

        // ============================================================
        // GcLine.Contains(float2) — infinite line
        // ============================================================

        [Test]
        public void InfiniteLine_Contains_PointOnLine()
        {
            var line = new GcLine(new float2(0, 0), new float2(1, 0));
            Assert.IsTrue(line.Contains(new float2(100, 0)));
            Assert.IsTrue(line.Contains(new float2(-50, 0)));
            Assert.IsTrue(line.Contains(new float2(0, 0)));
        }

        [Test]
        public void InfiniteLine_Contains_PointOffLine()
        {
            var line = new GcLine(new float2(0, 0), new float2(1, 0));
            Assert.IsFalse(line.Contains(new float2(5, 1)));
            Assert.IsFalse(line.Contains(new float2(0, -3)));
        }

        [Test]
        public void InfiniteLine_Contains_DiagonalLine()
        {
            var line = new GcLine(new float2(0, 0), math.normalize(new float2(1, 1)));
            Assert.IsTrue(line.Contains(new float2(50, 50)));
            Assert.IsFalse(line.Contains(new float2(50, 51)));
        }

        // ============================================================
        // GcAABB.Overlaps(GcAABB)
        // ============================================================

        [Test]
        public void AABB_Overlaps_Overlapping()
        {
            var a = new GcAABB(new float2(0, 0), new float2(5, 5));
            var b = new GcAABB(new float2(3, 3), new float2(5, 5));
            Assert.IsTrue(a.Overlaps(b));
            Assert.IsTrue(b.Overlaps(a));
        }

        [Test]
        public void AABB_Overlaps_Touching()
        {
            var a = new GcAABB(new float2(0, 0), new float2(5, 5));
            var b = new GcAABB(new float2(10, 0), new float2(5, 5));
            Assert.IsTrue(a.Overlaps(b));
        }

        [Test]
        public void AABB_Overlaps_Separated()
        {
            var a = new GcAABB(new float2(0, 0), new float2(5, 5));
            var b = new GcAABB(new float2(20, 0), new float2(5, 5));
            Assert.IsFalse(a.Overlaps(b));
        }

        [Test]
        public void AABB_Overlaps_Nested()
        {
            var outer = new GcAABB(new float2(0, 0), new float2(10, 10));
            var inner = new GcAABB(new float2(0, 0), new float2(2, 2));
            Assert.IsTrue(outer.Overlaps(inner));
            Assert.IsTrue(inner.Overlaps(outer));
        }

        [Test]
        public void AABB_Overlaps_SameBox()
        {
            var a = new GcAABB(new float2(5, 5), new float2(3, 3));
            Assert.IsTrue(a.Overlaps(a));
        }

        // ============================================================
        // GcCircle.Overlaps(GcCircle)
        // ============================================================

        [Test]
        public void Circle_Overlaps_Overlapping()
        {
            var a = new GcCircle(new float2(0, 0), 5f);
            var b = new GcCircle(new float2(3, 0), 5f);
            Assert.IsTrue(a.Overlaps(b));
            Assert.IsTrue(b.Overlaps(a));
        }

        [Test]
        public void Circle_Overlaps_Touching()
        {
            var a = new GcCircle(new float2(0, 0), 5f);
            var b = new GcCircle(new float2(10, 0), 5f);
            Assert.IsTrue(a.Overlaps(b));
        }

        [Test]
        public void Circle_Overlaps_Separated()
        {
            var a = new GcCircle(new float2(0, 0), 5f);
            var b = new GcCircle(new float2(20, 0), 5f);
            Assert.IsFalse(a.Overlaps(b));
        }

        [Test]
        public void Circle_Overlaps_Concentric()
        {
            var a = new GcCircle(new float2(0, 0), 10f);
            var b = new GcCircle(new float2(0, 0), 3f);
            Assert.IsTrue(a.Overlaps(b));
        }

        // ============================================================
        // GcLine.Intersects(GcLine) — segment vs segment
        // ============================================================

        [Test]
        public void Segment_Intersects_Crossing()
        {
            var a = GcLine.Segment(new float2(0, 0), new float2(10, 10));
            var b = GcLine.Segment(new float2(10, 0), new float2(0, 10));
            Assert.IsTrue(a.Intersects(b));
        }

        [Test]
        public void Segment_Intersects_Parallel()
        {
            var a = GcLine.Segment(new float2(0, 0), new float2(10, 0));
            var b = GcLine.Segment(new float2(0, 5), new float2(10, 5));
            Assert.IsFalse(a.Intersects(b));
        }

        [Test]
        public void Segment_Intersects_NonCrossing()
        {
            var a = GcLine.Segment(new float2(0, 0), new float2(5, 0));
            var b = GcLine.Segment(new float2(6, 1), new float2(6, 10));
            Assert.IsFalse(a.Intersects(b));
        }

        [Test]
        public void Segment_Intersects_TShaped()
        {
            var a = GcLine.Segment(new float2(0, 5), new float2(10, 5));
            var b = GcLine.Segment(new float2(5, 0), new float2(5, 5));
            Assert.IsTrue(a.Intersects(b));
        }

        // ============================================================
        // GcLine.Intersects(GcLine) — infinite line vs infinite line
        // ============================================================

        [Test]
        public void InfiniteLine_Intersects_Crossing()
        {
            var a = new GcLine(new float2(0, 0), new float2(1, 0));
            var b = new GcLine(new float2(0, 0), new float2(0, 1));
            Assert.IsTrue(a.Intersects(b));
        }

        [Test]
        public void InfiniteLine_Intersects_Parallel()
        {
            var a = new GcLine(new float2(0, 0), new float2(1, 0));
            var b = new GcLine(new float2(0, 5), new float2(1, 0));
            Assert.IsFalse(a.Intersects(b));
        }

        // ============================================================
        // GcLine.Intersects(GcLine, out float2)
        // ============================================================

        [Test]
        public void Segment_Intersects_WithPoint_Cross()
        {
            var a = GcLine.Segment(new float2(0, 0), new float2(10, 10));
            var b = GcLine.Segment(new float2(10, 0), new float2(0, 10));
            Assert.IsTrue(a.Intersects(b, out var pt));
            Assert.AreEqual(5f, pt.x, Delta);
            Assert.AreEqual(5f, pt.y, Delta);
        }

        [Test]
        public void Segment_Intersects_WithPoint_NoIntersection()
        {
            var a = GcLine.Segment(new float2(0, 0), new float2(5, 0));
            var b = GcLine.Segment(new float2(0, 2), new float2(5, 2));
            Assert.IsFalse(a.Intersects(b, out var pt));
            Assert.AreEqual(0f, pt.x, Delta);
            Assert.AreEqual(0f, pt.y, Delta);
        }

        [Test]
        public void Segment_Intersects_WithPoint_Perpendicular()
        {
            var a = GcLine.Segment(new float2(0, 5), new float2(10, 5));
            var b = GcLine.Segment(new float2(3, 0), new float2(3, 10));
            Assert.IsTrue(a.Intersects(b, out var pt));
            Assert.AreEqual(3f, pt.x, Delta);
            Assert.AreEqual(5f, pt.y, Delta);
        }

        [Test]
        public void Segment_Intersects_WithPoint_AtOrigin()
        {
            var a = GcLine.Segment(new float2(-5, 0), new float2(5, 0));
            var b = GcLine.Segment(new float2(0, -5), new float2(0, 5));
            Assert.IsTrue(a.Intersects(b, out var pt));
            Assert.AreEqual(0f, pt.x, Delta);
            Assert.AreEqual(0f, pt.y, Delta);
        }

        [Test]
        public void InfiniteLine_Intersects_WithPoint()
        {
            var a = new GcLine(new float2(0, 0), new float2(1, 0));
            var b = new GcLine(new float2(5, -5), math.normalize(new float2(0, 1)));
            Assert.IsTrue(a.Intersects(b, out var pt));
            Assert.AreEqual(5f, pt.x, Delta);
            Assert.AreEqual(0f, pt.y, Delta);
        }

        // ============================================================
        // GcAABB.SweepTest — point vs box (diagonal deltas)
        // ============================================================

        [Test]
        public void SweepTest_PointHitsBox()
        {
            // Box at (10,5), halfSize (5,5) => spans [5..15, 0..10]
            // Point at origin, moving diagonally toward the box
            var box = new GcAABB(new float2(10, 5), new float2(5, 5));
            var point = new float2(0, 0);
            var delta = new float2(20, 10);
            Assert.IsTrue(box.SweepTest(point, delta, out var sweep));
            Assert.Greater(sweep.SweepRatioOnHit, 0f);
            Assert.Less(sweep.SweepRatioOnHit, 1f);
        }

        [Test]
        public void SweepTest_PointMissesBox()
        {
            var box = new GcAABB(new float2(10, 0), new float2(5, 5));
            var point = new float2(0, 20);
            var delta = new float2(20, 1);
            Assert.IsFalse(box.SweepTest(point, delta, out _));
        }

        [Test]
        public void SweepTest_PointMovesAway()
        {
            var box = new GcAABB(new float2(10, 5), new float2(5, 5));
            var point = new float2(0, 0);
            var delta = new float2(-10, -5);
            Assert.IsFalse(box.SweepTest(point, delta, out _));
        }

        [Test]
        public void SweepTest_PointHitsBoxFromAbove()
        {
            // Box at (5,10), halfSize (5,5) => spans [0..10, 5..15]
            // Point at (5,0), moving diagonally down-right
            var box = new GcAABB(new float2(5, 10), new float2(5, 5));
            var point = new float2(5, 0);
            var delta = new float2(1, 20);
            Assert.IsTrue(box.SweepTest(point, delta, out var sweep));
            Assert.Greater(sweep.SweepRatioOnHit, 0f);
            Assert.Less(sweep.SweepRatioOnHit, 1f);
        }

        // ============================================================
        // GcAABB.SweepTest — box vs box (diagonal deltas)
        // ============================================================

        [Test]
        public void SweepTest_BoxHitsBox()
        {
            var staticBox = new GcAABB(new float2(20, 5), new float2(5, 5));
            var dynamicBox = new GcAABB(new float2(0, 0), new float2(3, 3));
            var delta = new float2(30, 10);
            Assert.IsTrue(staticBox.SweepTest(dynamicBox, delta, out var sweep));
            Assert.Greater(sweep.SweepRatioOnHit, 0f);
            Assert.Less(sweep.SweepRatioOnHit, 1f);
        }

        [Test]
        public void SweepTest_BoxMissesBox()
        {
            var staticBox = new GcAABB(new float2(20, 0), new float2(5, 5));
            var dynamicBox = new GcAABB(new float2(0, 20), new float2(3, 3));
            var delta = new float2(30, 1);
            Assert.IsFalse(staticBox.SweepTest(dynamicBox, delta, out _));
        }

        [Test]
        public void SweepTest_BoxHitsBox_HitNormal()
        {
            // Dynamic box approaches from the left side
            // Static box at (20,2), halfSize (5,5) => spans [15..25, -3..7]
            // Dynamic box at (0,2), halfSize (3,3), moves (30,1)
            // Should hit the left face => normal (-1, 0)
            var staticBox = new GcAABB(new float2(20, 2), new float2(5, 5));
            var dynamicBox = new GcAABB(new float2(0, 2), new float2(3, 3));
            var delta = new float2(30, 1);
            Assert.IsTrue(staticBox.SweepTest(dynamicBox, delta, out var sweep));
            Assert.AreEqual(-1f, sweep.HitNormal.x, Delta);
            Assert.AreEqual(0f, sweep.HitNormal.y, Delta);
        }

        // ============================================================
        // GcLine.CalcDistance(float2) — segment
        // ============================================================

        [Test]
        public void CalcDistance_PointOnSegment()
        {
            var seg = GcLine.Segment(new float2(0, 0), new float2(10, 0));
            Assert.AreEqual(0f, seg.CalcDistance(new float2(5, 0)), Delta);
        }

        [Test]
        public void CalcDistance_PerpendicularDistance()
        {
            var seg = GcLine.Segment(new float2(0, 0), new float2(10, 0));
            Assert.AreEqual(3f, seg.CalcDistance(new float2(5, 3)), Delta);
            Assert.AreEqual(7f, seg.CalcDistance(new float2(5, -7)), Delta);
        }

        [Test]
        public void CalcDistance_BeyondEndpoint()
        {
            var seg = GcLine.Segment(new float2(0, 0), new float2(10, 0));
            Assert.AreEqual(5f, seg.CalcDistance(new float2(15, 0)), Delta);
            Assert.AreEqual(3f, seg.CalcDistance(new float2(-3, 0)), Delta);
        }

        [Test]
        public void CalcDistance_DiagonalSegment()
        {
            var seg = GcLine.Segment(new float2(0, 0), new float2(10, 10));
            Assert.AreEqual(0f, seg.CalcDistance(new float2(5, 5)), Delta);
            float expected = math.abs(0f - 10f) / math.sqrt(2f);
            Assert.AreEqual(expected, seg.CalcDistance(new float2(0, 10)), Delta);
        }

        [Test]
        public void CalcDistance_AtEndpoints()
        {
            var seg = GcLine.Segment(new float2(0, 0), new float2(10, 0));
            Assert.AreEqual(0f, seg.CalcDistance(new float2(0, 0)), Delta);
            Assert.AreEqual(0f, seg.CalcDistance(new float2(10, 0)), Delta);
        }

        // ============================================================
        // GcLine.CalcDistance(float2) — infinite line
        // ============================================================

        [Test]
        public void CalcDistance_InfiniteLine_PointOnLine()
        {
            var line = new GcLine(new float2(0, 0), new float2(1, 0));
            Assert.AreEqual(0f, line.CalcDistance(new float2(100, 0)), Delta);
        }

        [Test]
        public void CalcDistance_InfiniteLine_PerpendicularDistance()
        {
            var line = new GcLine(new float2(0, 0), new float2(1, 0));
            Assert.AreEqual(5f, line.CalcDistance(new float2(100, 5)), Delta);
            Assert.AreEqual(3f, line.CalcDistance(new float2(-50, -3)), Delta);
        }

        [Test]
        public void CalcDistance_InfiniteLine_Diagonal()
        {
            var line = new GcLine(new float2(0, 0), math.normalize(new float2(1, 1)));
            // Point (10,0) distance to y=x line: |10-0|/sqrt(2) = ~7.071
            float expected = 10f / math.sqrt(2f);
            Assert.AreEqual(expected, line.CalcDistance(new float2(10, 0)), Delta);
        }
    }
}
