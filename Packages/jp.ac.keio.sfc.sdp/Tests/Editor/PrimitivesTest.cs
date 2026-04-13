#nullable enable
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

namespace GameCanvas.Editor.Tests
{
    public class PrimitivesTest
    {
        const float Delta = 0.001f;

        // ====================================================================
        // GcAABB
        // ====================================================================

        [Test]
        public void GcAABB_Constructor_SetsCenterAndHalfSize()
        {
            var aabb = new GcAABB(new float2(10f, 20f), new float2(5f, 8f));
            Assert.AreEqual(10f, aabb.Center.x, Delta);
            Assert.AreEqual(20f, aabb.Center.y, Delta);
            Assert.AreEqual(5f, aabb.HalfSize.x, Delta);
            Assert.AreEqual(8f, aabb.HalfSize.y, Delta);
        }

        [Test]
        public void GcAABB_Constructor_AbsHalfSize()
        {
            var aabb = new GcAABB(new float2(0f, 0f), new float2(-3f, -7f));
            Assert.AreEqual(3f, aabb.HalfSize.x, Delta);
            Assert.AreEqual(7f, aabb.HalfSize.y, Delta);
        }

        [Test]
        public void GcAABB_ConstructorRect_CenterAndHalfSize()
        {
            // Rect(x, y, width, height) — x,y is top-left in Unity screen coords
            var rect = new Rect(10f, 20f, 40f, 60f);
            var aabb = new GcAABB(rect);
            // Rect.center = (10+20, 20+30) = (30, 50)
            Assert.AreEqual(30f, aabb.Center.x, Delta);
            Assert.AreEqual(50f, aabb.Center.y, Delta);
            Assert.AreEqual(20f, aabb.HalfSize.x, Delta);
            Assert.AreEqual(30f, aabb.HalfSize.y, Delta);
        }

        [Test]
        public void GcAABB_GetPoint_UpperLeft()
        {
            var aabb = new GcAABB(new float2(10f, 10f), new float2(5f, 5f));
            var pt = aabb.GetPoint(GcAnchor.UpperLeft);
            Assert.AreEqual(5f, pt.x, Delta);
            Assert.AreEqual(5f, pt.y, Delta);
        }

        [Test]
        public void GcAABB_GetPoint_UpperCenter()
        {
            var aabb = new GcAABB(new float2(10f, 10f), new float2(5f, 5f));
            var pt = aabb.GetPoint(GcAnchor.UpperCenter);
            Assert.AreEqual(10f, pt.x, Delta);
            Assert.AreEqual(5f, pt.y, Delta);
        }

        [Test]
        public void GcAABB_GetPoint_UpperRight()
        {
            var aabb = new GcAABB(new float2(10f, 10f), new float2(5f, 5f));
            var pt = aabb.GetPoint(GcAnchor.UpperRight);
            Assert.AreEqual(15f, pt.x, Delta);
            Assert.AreEqual(5f, pt.y, Delta);
        }

        [Test]
        public void GcAABB_GetPoint_MiddleLeft()
        {
            var aabb = new GcAABB(new float2(10f, 10f), new float2(5f, 5f));
            var pt = aabb.GetPoint(GcAnchor.MiddleLeft);
            Assert.AreEqual(5f, pt.x, Delta);
            Assert.AreEqual(10f, pt.y, Delta);
        }

        [Test]
        public void GcAABB_GetPoint_MiddleCenter()
        {
            var aabb = new GcAABB(new float2(10f, 10f), new float2(5f, 5f));
            var pt = aabb.GetPoint(GcAnchor.MiddleCenter);
            Assert.AreEqual(10f, pt.x, Delta);
            Assert.AreEqual(10f, pt.y, Delta);
        }

        [Test]
        public void GcAABB_GetPoint_MiddleRight()
        {
            var aabb = new GcAABB(new float2(10f, 10f), new float2(5f, 5f));
            var pt = aabb.GetPoint(GcAnchor.MiddleRight);
            Assert.AreEqual(15f, pt.x, Delta);
            Assert.AreEqual(10f, pt.y, Delta);
        }

        [Test]
        public void GcAABB_GetPoint_LowerLeft()
        {
            var aabb = new GcAABB(new float2(10f, 10f), new float2(5f, 5f));
            var pt = aabb.GetPoint(GcAnchor.LowerLeft);
            Assert.AreEqual(5f, pt.x, Delta);
            Assert.AreEqual(15f, pt.y, Delta);
        }

        [Test]
        public void GcAABB_GetPoint_LowerCenter()
        {
            var aabb = new GcAABB(new float2(10f, 10f), new float2(5f, 5f));
            var pt = aabb.GetPoint(GcAnchor.LowerCenter);
            Assert.AreEqual(10f, pt.x, Delta);
            Assert.AreEqual(15f, pt.y, Delta);
        }

        [Test]
        public void GcAABB_GetPoint_LowerRight()
        {
            var aabb = new GcAABB(new float2(10f, 10f), new float2(5f, 5f));
            var pt = aabb.GetPoint(GcAnchor.LowerRight);
            Assert.AreEqual(15f, pt.x, Delta);
            Assert.AreEqual(15f, pt.y, Delta);
        }

        [Test]
        public void GcAABB_WH_CreatesAtOriginWithSize()
        {
            var aabb = GcAABB.WH(new float2(20f, 10f));
            // center = size * 0.5 = (10, 5), halfSize = abs(center) = (10, 5)
            Assert.AreEqual(10f, aabb.Center.x, Delta);
            Assert.AreEqual(5f, aabb.Center.y, Delta);
            Assert.AreEqual(10f, aabb.HalfSize.x, Delta);
            Assert.AreEqual(5f, aabb.HalfSize.y, Delta);
        }

        [Test]
        public void GcAABB_XYWH_FourFloats()
        {
            var aabb = GcAABB.XYWH(10f, 20f, 30f, 40f);
            // halfSize = abs((30, 40) * 0.5) = (15, 20)
            // center = (10, 20) + (15, 20) = (25, 40)
            Assert.AreEqual(25f, aabb.Center.x, Delta);
            Assert.AreEqual(40f, aabb.Center.y, Delta);
            Assert.AreEqual(15f, aabb.HalfSize.x, Delta);
            Assert.AreEqual(20f, aabb.HalfSize.y, Delta);
        }

        [Test]
        public void GcAABB_XYWH_PositionAndSize()
        {
            var aabb = GcAABB.XYWH(new float2(5f, 10f), new float2(20f, 30f));
            // halfSize = abs((20, 30) * 0.5) = (10, 15)
            // center = (5, 10) + (10, 15) = (15, 25)
            Assert.AreEqual(15f, aabb.Center.x, Delta);
            Assert.AreEqual(25f, aabb.Center.y, Delta);
            Assert.AreEqual(10f, aabb.HalfSize.x, Delta);
            Assert.AreEqual(15f, aabb.HalfSize.y, Delta);
        }

        [Test]
        public void GcAABB_MinMax_NormalOrder()
        {
            var aabb = GcAABB.MinMax(new float2(0f, 0f), new float2(10f, 20f));
            // min=(0,0), max=(10,20), center=(5,10), halfSize=abs(max-min)=(10,20)
            Assert.AreEqual(5f, aabb.Center.x, Delta);
            Assert.AreEqual(10f, aabb.Center.y, Delta);
            Assert.AreEqual(10f, aabb.HalfSize.x, Delta);
            Assert.AreEqual(20f, aabb.HalfSize.y, Delta);
        }

        [Test]
        public void GcAABB_MinMax_ReversedOrder()
        {
            var aabb = GcAABB.MinMax(new float2(10f, 20f), new float2(0f, 0f));
            // Same result regardless of order
            Assert.AreEqual(5f, aabb.Center.x, Delta);
            Assert.AreEqual(10f, aabb.Center.y, Delta);
            Assert.AreEqual(10f, aabb.HalfSize.x, Delta);
            Assert.AreEqual(20f, aabb.HalfSize.y, Delta);
        }

        [Test]
        public void GcAABB_MinMax_MixedOrder()
        {
            // a.x > b.x but a.y < b.y
            var aabb = GcAABB.MinMax(new float2(8f, 2f), new float2(4f, 6f));
            // min=(4,2), max=(8,6), center=(6,4), halfSize=abs((4,4))=(4,4)
            Assert.AreEqual(6f, aabb.Center.x, Delta);
            Assert.AreEqual(4f, aabb.Center.y, Delta);
            Assert.AreEqual(4f, aabb.HalfSize.x, Delta);
            Assert.AreEqual(4f, aabb.HalfSize.y, Delta);
        }

        [Test]
        public void GcAABB_Equality_EqualInstances()
        {
            var a = new GcAABB(new float2(1f, 2f), new float2(3f, 4f));
            var b = new GcAABB(new float2(1f, 2f), new float2(3f, 4f));
            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(a.Equals((object)b));
        }

        [Test]
        public void GcAABB_Equality_DifferentInstances()
        {
            var a = new GcAABB(new float2(1f, 2f), new float2(3f, 4f));
            var b = new GcAABB(new float2(1f, 2f), new float2(5f, 4f));
            Assert.IsFalse(a == b);
            Assert.IsTrue(a != b);
            Assert.IsFalse(a.Equals(b));
        }

        [Test]
        public void GcAABB_ImplicitConversion_ToGcRect()
        {
            var aabb = new GcAABB(new float2(10f, 10f), new float2(5f, 5f));
            GcRect rect = aabb;
            // Position = Center - HalfSize = (5, 5), Size = HalfSize * 2 = (10, 10)
            Assert.AreEqual(5f, rect.Position.x, Delta);
            Assert.AreEqual(5f, rect.Position.y, Delta);
            Assert.AreEqual(10f, rect.Size.x, Delta);
            Assert.AreEqual(10f, rect.Size.y, Delta);
            Assert.AreEqual(0f, rect.Radian, Delta);
        }

        [Test]
        public void GcAABB_Extensions_PositionAndSize()
        {
            var aabb = new GcAABB(new float2(10f, 20f), new float2(5f, 8f));
            var pos = aabb.Position();
            var size = aabb.Size();
            Assert.AreEqual(5f, pos.x, Delta);
            Assert.AreEqual(12f, pos.y, Delta);
            Assert.AreEqual(10f, size.x, Delta);
            Assert.AreEqual(16f, size.y, Delta);
        }

        [Test]
        public void GcAABB_Extensions_MinMax()
        {
            var aabb = new GcAABB(new float2(10f, 20f), new float2(5f, 8f));
            var min = aabb.Min();
            var max = aabb.Max();
            Assert.AreEqual(5f, min.x, Delta);
            Assert.AreEqual(12f, min.y, Delta);
            Assert.AreEqual(15f, max.x, Delta);
            Assert.AreEqual(28f, max.y, Delta);
        }

        // ====================================================================
        // GcCircle
        // ====================================================================

        [Test]
        public void GcCircle_Constructor_Float2AndRadius()
        {
            var circle = new GcCircle(new float2(3f, 4f), 10f);
            Assert.AreEqual(3f, circle.Position.x, Delta);
            Assert.AreEqual(4f, circle.Position.y, Delta);
            Assert.AreEqual(10f, circle.Radius, Delta);
        }

        [Test]
        public void GcCircle_Constructor_ThreeFloats()
        {
            var circle = new GcCircle(5f, 6f, 7f);
            Assert.AreEqual(5f, circle.Position.x, Delta);
            Assert.AreEqual(6f, circle.Position.y, Delta);
            Assert.AreEqual(7f, circle.Radius, Delta);
        }

        [Test]
        public void GcCircle_Equality_EqualInstances()
        {
            var a = new GcCircle(new float2(1f, 2f), 3f);
            var b = new GcCircle(new float2(1f, 2f), 3f);
            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(a.Equals((object)b));
        }

        [Test]
        public void GcCircle_Equality_DifferentInstances()
        {
            var a = new GcCircle(new float2(1f, 2f), 3f);
            var b = new GcCircle(new float2(1f, 2f), 5f);
            Assert.IsFalse(a == b);
            Assert.IsTrue(a != b);
            Assert.IsFalse(a.Equals(b));
        }

        [Test]
        public void GcCircle_Default_IsZeroRadius()
        {
            var circle = default(GcCircle);
            Assert.AreEqual(0f, circle.Position.x, Delta);
            Assert.AreEqual(0f, circle.Position.y, Delta);
            Assert.AreEqual(0f, circle.Radius, Delta);
        }

        // ====================================================================
        // GcLine
        // ====================================================================

        [Test]
        public void GcLine_Segment_CreatesFromTwoPoints()
        {
            var line = GcLine.Segment(new float2(0f, 0f), new float2(3f, 4f));
            Assert.AreEqual(0f, line.Origin.x, Delta);
            Assert.AreEqual(0f, line.Origin.y, Delta);
            Assert.AreEqual(5f, line.Length, Delta);
            // Direction = normalize((3,4)) = (0.6, 0.8)
            Assert.AreEqual(0.6f, line.Direction.x, Delta);
            Assert.AreEqual(0.8f, line.Direction.y, Delta);
        }

        [Test]
        public void GcLine_Segment_HorizontalLine()
        {
            var line = GcLine.Segment(new float2(1f, 5f), new float2(11f, 5f));
            Assert.AreEqual(10f, line.Length, Delta);
            Assert.AreEqual(1f, line.Direction.x, Delta);
            Assert.AreEqual(0f, line.Direction.y, Delta);
        }

        [Test]
        public void GcLine_IsSegment_TrueForFiniteLength()
        {
            var seg = GcLine.Segment(new float2(0f, 0f), new float2(1f, 0f));
            Assert.IsTrue(seg.IsSegment());
        }

        [Test]
        public void GcLine_IsSegment_FalseForInfiniteLine()
        {
            // Constructor with (origin, direction) creates an infinite line
            var inf = new GcLine(new float2(0f, 0f), new float2(1f, 0f));
            Assert.IsFalse(inf.IsSegment());
        }

        [Test]
        public void GcLine_Vector_ReturnsDirectionTimesLength()
        {
            var line = GcLine.Segment(new float2(0f, 0f), new float2(3f, 4f));
            var vec = line.Vector();
            Assert.AreEqual(3f, vec.x, Delta);
            Assert.AreEqual(4f, vec.y, Delta);
        }

        [Test]
        public void GcLine_IsZero_TrueForZeroLength()
        {
            var line = GcLine.Segment(new float2(5f, 5f), new float2(5f, 5f));
            Assert.IsTrue(line.IsZero());
        }

        [Test]
        public void GcLine_IsZero_FalseForNonZeroLength()
        {
            var line = GcLine.Segment(new float2(0f, 0f), new float2(10f, 0f));
            Assert.IsFalse(line.IsZero());
        }

        [Test]
        public void GcLine_Equality_EqualInstances()
        {
            var a = GcLine.Segment(new float2(0f, 0f), new float2(1f, 1f));
            var b = GcLine.Segment(new float2(0f, 0f), new float2(1f, 1f));
            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(a.Equals((object)b));
        }

        [Test]
        public void GcLine_Equality_DifferentInstances()
        {
            var a = GcLine.Segment(new float2(0f, 0f), new float2(1f, 0f));
            var b = GcLine.Segment(new float2(0f, 0f), new float2(0f, 1f));
            Assert.IsFalse(a == b);
            Assert.IsTrue(a != b);
        }

        [Test]
        public void GcLine_End_ReturnsEndPoint()
        {
            var line = GcLine.Segment(new float2(2f, 3f), new float2(5f, 7f));
            var end = line.End();
            Assert.AreEqual(5f, end.x, Delta);
            Assert.AreEqual(7f, end.y, Delta);
        }

        [Test]
        public void GcLine_Begin_ReturnsOrigin()
        {
            var line = GcLine.Segment(new float2(2f, 3f), new float2(5f, 7f));
            var begin = line.Begin();
            Assert.AreEqual(2f, begin.x, Delta);
            Assert.AreEqual(3f, begin.y, Delta);
        }

        [Test]
        public void GcLine_Constructor_OriginLengthDirection()
        {
            var line = new GcLine(new float2(1f, 2f), 10f, new float2(3f, 4f));
            Assert.AreEqual(1f, line.Origin.x, Delta);
            Assert.AreEqual(2f, line.Origin.y, Delta);
            Assert.AreEqual(10f, line.Length, Delta);
            // Direction is normalized: (3,4)/5 = (0.6, 0.8)
            Assert.AreEqual(0.6f, line.Direction.x, Delta);
            Assert.AreEqual(0.8f, line.Direction.y, Delta);
        }

        [Test]
        public void GcLine_Constructor_InfiniteLineFromDirection()
        {
            var line = new GcLine(new float2(1f, 2f), new float2(1f, 0f));
            Assert.AreEqual(float.PositiveInfinity, line.Length);
            Assert.IsFalse(line.IsSegment());
        }

        [Test]
        public void GcAABB_ExplicitCast_ToRect()
        {
            var aabb = new GcAABB(new float2(10f, 10f), new float2(5f, 5f));
            var rect = (Rect)aabb;
            // Position = (5, 5), Size = (10, 10)
            Assert.AreEqual(5f, rect.x, Delta);
            Assert.AreEqual(5f, rect.y, Delta);
            Assert.AreEqual(10f, rect.width, Delta);
            Assert.AreEqual(10f, rect.height, Delta);
        }

        [Test]
        public void GcAABB_ExplicitCast_FromRect()
        {
            var rect = new Rect(0f, 0f, 20f, 10f);
            var aabb = (GcAABB)rect;
            // center = (10, 5), halfSize = (10, 5)
            Assert.AreEqual(10f, aabb.Center.x, Delta);
            Assert.AreEqual(5f, aabb.Center.y, Delta);
            Assert.AreEqual(10f, aabb.HalfSize.x, Delta);
            Assert.AreEqual(5f, aabb.HalfSize.y, Delta);
        }

        // ====================================================================
        // GcRect
        // ====================================================================

        [Test]
        public void GcRect_FromDegrees_ConvertsToRadians()
        {
            var rect = GcRect.FromDegrees(new float2(1f, 2f), new float2(3f, 4f), 90f);
            Assert.AreEqual(1f, rect.Position.x, Delta);
            Assert.AreEqual(2f, rect.Position.y, Delta);
            Assert.AreEqual(3f, rect.Size.x, Delta);
            Assert.AreEqual(4f, rect.Size.y, Delta);
            Assert.AreEqual(math.PI * 0.5f, rect.Radian, Delta);
        }

        [Test]
        public void GcRect_FromDegrees_Components_ConvertsToRadians()
        {
            var rect = GcRect.FromDegrees(5f, 6f, 7f, 8f, 180f);
            Assert.AreEqual(5f, rect.Position.x, Delta);
            Assert.AreEqual(6f, rect.Position.y, Delta);
            Assert.AreEqual(7f, rect.Size.x, Delta);
            Assert.AreEqual(8f, rect.Size.y, Delta);
            Assert.AreEqual(math.PI, rect.Radian, Delta);
        }

        [Test]
        public void GcRect_FromDegrees_Zero()
        {
            var rect = GcRect.FromDegrees(new float2(0f, 0f), new float2(10f, 10f), 0f);
            Assert.AreEqual(0f, rect.Radian, Delta);
        }
    }
}
