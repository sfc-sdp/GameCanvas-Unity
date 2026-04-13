#nullable enable
using NUnit.Framework;
using Unity.Mathematics;
using GameCanvas;

namespace GameCanvas.Editor.Tests
{
    public class UtilityTest
    {
        const float Delta = 0.01f;
        const float TrigDelta = 0.001f;
        static readonly float HalfPI = math.PI * 0.5f;
        static readonly float PI = math.PI;

        // ============================================================
        //  GcAffine
        // ============================================================

        [Test]
        public void GcAffine_FromTranslate_TransformsPoint()
        {
            var mtx = GcAffine.FromTranslate(new float2(3f, -7f));
            var result = mtx.Mul(new float2(1f, 2f));
            Assert.AreEqual(4f, result.x, Delta);
            Assert.AreEqual(-5f, result.y, Delta);
        }

        [Test]
        public void GcAffine_FromRotate_90Degrees()
        {
            var mtx = GcAffine.FromRotate(HalfPI);
            var result = mtx.Mul(new float2(1f, 0f));
            Assert.AreEqual(0f, result.x, Delta);
            Assert.AreEqual(1f, result.y, Delta);
        }

        [Test]
        public void GcAffine_FromRotate_180Degrees()
        {
            var mtx = GcAffine.FromRotate(PI);
            var result = mtx.Mul(new float2(1f, 0f));
            Assert.AreEqual(-1f, result.x, Delta);
            Assert.AreEqual(0f, result.y, Delta);
        }

        [Test]
        public void GcAffine_FromRotate_270Degrees()
        {
            var mtx = GcAffine.FromRotate(PI * 1.5f);
            var result = mtx.Mul(new float2(1f, 0f));
            Assert.AreEqual(0f, result.x, Delta);
            Assert.AreEqual(-1f, result.y, Delta);
        }

        [Test]
        public void GcAffine_FromScale_Float2()
        {
            var mtx = GcAffine.FromScale(new float2(2f, 3f));
            var result = mtx.Mul(new float2(4f, 5f));
            Assert.AreEqual(8f, result.x, Delta);
            Assert.AreEqual(15f, result.y, Delta);
        }

        [Test]
        public void GcAffine_FromScale_UniformFloat()
        {
            var mtx = GcAffine.FromScale(5f);
            var result = mtx.Mul(new float2(2f, -1f));
            Assert.AreEqual(10f, result.x, Delta);
            Assert.AreEqual(-5f, result.y, Delta);
        }

        [Test]
        public void GcAffine_MulMatrix_TranslateThenRotate()
        {
            var t = GcAffine.FromTranslate(new float2(1f, 0f));
            var r = GcAffine.FromRotate(HalfPI);
            // rotate first, then translate: T * R applied to (1,0)
            var combined = t.Mul(r);
            var result = combined.Mul(new float2(1f, 0f));
            // R*(1,0) = (0,1), then T => (1,1)
            Assert.AreEqual(1f, result.x, Delta);
            Assert.AreEqual(1f, result.y, Delta);
        }

        [Test]
        public void GcAffine_MulVector_IdentityReturnsSamePoint()
        {
            var pt = new float2(42f, -13f);
            var result = GcAffine.Identity.Mul(pt);
            Assert.AreEqual(pt.x, result.x, Delta);
            Assert.AreEqual(pt.y, result.y, Delta);
        }

        [Test]
        public void GcAffine_Inverse_Translation()
        {
            var t = GcAffine.FromTranslate(new float2(5f, -3f));
            // Manual inverse: translate by (-5, 3)
            var inv = GcAffine.FromTranslate(new float2(-5f, 3f));
            var combined = t.Mul(inv);
            // Should be identity
            var result = combined.Mul(new float2(7f, 11f));
            Assert.AreEqual(7f, result.x, Delta);
            Assert.AreEqual(11f, result.y, Delta);
        }

        [Test]
        public void GcAffine_Inverse_Rotation()
        {
            var r = GcAffine.FromRotate(HalfPI);
            var rInv = GcAffine.FromRotate(-HalfPI);
            var combined = r.Mul(rInv);
            var result = combined.Mul(new float2(3f, 4f));
            Assert.AreEqual(3f, result.x, Delta);
            Assert.AreEqual(4f, result.y, Delta);
        }

        [Test]
        public void GcAffine_CalcRotate_ExtractsAngle()
        {
            var angle = math.radians(45f);
            var mtx = GcAffine.FromRotate(angle);
            var extracted = mtx.CalcRotate();
            Assert.AreEqual(angle, extracted, Delta);
        }

        [Test]
        public void GcAffine_CalcRotate_ZeroForIdentity()
        {
            var extracted = GcAffine.Identity.CalcRotate();
            Assert.AreEqual(0f, extracted, Delta);
        }

        [Test]
        public void GcAffine_CalcRotate_WrapsAt270()
        {
            // FromRotate(3PI/2) => CalcRotate returns -atan2(c1.x, c0.x)
            // sin(3PI/2)=-1, cos(3PI/2)=0 => c1.x=1, c0.x=0 => -atan2(1,0) = -PI/2
            var mtx = GcAffine.FromRotate(PI * 1.5f);
            var extracted = mtx.CalcRotate();
            Assert.AreEqual(-HalfPI, extracted, Delta);
        }

        [Test]
        public void GcAffine_CalcScale_ExtractsScale()
        {
            var mtx = GcAffine.FromScale(new float2(3f, 7f));
            var scale = mtx.CalcScale();
            Assert.AreEqual(3f, scale.x, Delta);
            Assert.AreEqual(7f, scale.y, Delta);
        }

        [Test]
        public void GcAffine_CalcScale_IdentityReturnsOne()
        {
            var scale = GcAffine.Identity.CalcScale();
            Assert.AreEqual(1f, scale.x, Delta);
            Assert.AreEqual(1f, scale.y, Delta);
        }

        [Test]
        public void GcAffine_CalcScale_UniformWithRotation()
        {
            // Uniform scale + rotation: CalcScale recovers the magnitude
            var mtx = GcAffine.FromRotate(math.radians(30f))
                .Mul(GcAffine.FromScale(3f));
            var scale = mtx.CalcScale();
            Assert.AreEqual(3f, scale.x, Delta);
            Assert.AreEqual(3f, scale.y, Delta);
        }

        // ============================================================
        //  GcMath — untested methods
        // ============================================================

        [Test]
        public void GcMath_Sin_SpecificAngles()
        {
            Assert.AreEqual(0f, GcMath.Sin(0f), TrigDelta);
            Assert.AreEqual(0.5f, GcMath.Sin(30f), TrigDelta);
            Assert.AreEqual(1f, GcMath.Sin(90f), TrigDelta);
            Assert.AreEqual(0f, GcMath.Sin(180f), TrigDelta);
        }

        [Test]
        public void GcMath_Cos_SpecificAngles()
        {
            Assert.AreEqual(1f, GcMath.Cos(0f), TrigDelta);
            Assert.AreEqual(0.866f, GcMath.Cos(30f), TrigDelta);
            Assert.AreEqual(0f, GcMath.Cos(90f), TrigDelta);
            Assert.AreEqual(-1f, GcMath.Cos(180f), TrigDelta);
        }

        [Test]
        public void GcMath_Sqrt_PerfectSquares()
        {
            Assert.AreEqual(0f, GcMath.Sqrt(0f), Delta);
            Assert.AreEqual(1f, GcMath.Sqrt(1f), Delta);
            Assert.AreEqual(3f, GcMath.Sqrt(9f), Delta);
            Assert.AreEqual(10f, GcMath.Sqrt(100f), Delta);
        }

        [Test]
        public void GcMath_Clamp_WithinRange()
        {
            Assert.AreEqual(5f, GcMath.Clamp(5f, 0f, 10f), Delta);
        }

        [Test]
        public void GcMath_Clamp_BelowMin()
        {
            Assert.AreEqual(0f, GcMath.Clamp(-3f, 0f, 10f), Delta);
        }

        [Test]
        public void GcMath_Clamp_AboveMax()
        {
            Assert.AreEqual(10f, GcMath.Clamp(15f, 0f, 10f), Delta);
        }

        [Test]
        public void GcMath_Repeat_Wrapping()
        {
            Assert.AreEqual(1f, GcMath.Repeat(1f, 3f), Delta);
            Assert.AreEqual(1f, GcMath.Repeat(4f, 3f), Delta);
            Assert.AreEqual(2f, GcMath.Repeat(5f, 3f), Delta);
            Assert.AreEqual(0f, GcMath.Repeat(0f, 3f), Delta);
        }

        [Test]
        public void GcMath_Repeat_NegativeValue()
        {
            // Mathf.Repeat(-1, 3) => 2
            Assert.AreEqual(2f, GcMath.Repeat(-1f, 3f), Delta);
        }

        [Test]
        public void GcMath_RotateVector_90Degrees()
        {
            var result = GcMath.RotateVector(new float2(1f, 0f), 90f);
            Assert.AreEqual(0f, result.x, Delta);
            Assert.AreEqual(1f, result.y, Delta);
        }

        [Test]
        public void GcMath_RotateVector_180Degrees()
        {
            var result = GcMath.RotateVector(new float2(1f, 0f), 180f);
            Assert.AreEqual(-1f, result.x, Delta);
            Assert.AreEqual(0f, result.y, Delta);
        }

        [Test]
        public void GcMath_Round_Positive()
        {
            Assert.AreEqual(3, GcMath.Round(2.7));
            Assert.AreEqual(2, GcMath.Round(2.3));
        }

        [Test]
        public void GcMath_Round_Negative()
        {
            Assert.AreEqual(-3, GcMath.Round(-2.7));
            Assert.AreEqual(-2, GcMath.Round(-2.3));
        }

        [Test]
        public void GcMath_Round_HalfValues()
        {
            // System.Math.Round uses banker's rounding by default
            Assert.AreEqual(2, GcMath.Round(2.5));
            Assert.AreEqual(4, GcMath.Round(3.5));
        }

        [Test]
        public void GcMath_Cross_KnownVectors()
        {
            // (1,0) x (0,1) = 1
            Assert.AreEqual(1f, GcMath.Cross(new float2(1f, 0f), new float2(0f, 1f)), Delta);
            // (0,1) x (1,0) = -1
            Assert.AreEqual(-1f, GcMath.Cross(new float2(0f, 1f), new float2(1f, 0f)), Delta);
            // parallel => 0
            Assert.AreEqual(0f, GcMath.Cross(new float2(2f, 3f), new float2(4f, 6f)), Delta);
        }

        [Test]
        public void GcMath_Dot_KnownVectors()
        {
            // (1,0) . (0,1) = 0
            Assert.AreEqual(0f, GcMath.Dot(new float2(1f, 0f), new float2(0f, 1f)), Delta);
            // (3,4) . (3,4) = 25
            Assert.AreEqual(25f, GcMath.Dot(new float2(3f, 4f), new float2(3f, 4f)), Delta);
            // (1,2) . (3,4) = 11
            Assert.AreEqual(11f, GcMath.Dot(new float2(1f, 2f), new float2(3f, 4f)), Delta);
        }
    }
}
