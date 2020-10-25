using NUnit.Framework;
using Unity.Mathematics;

namespace GameCanvas.Tests.Editor
{
    public class EditmodeTest
    {
        [Test]
        public void GcMathAbs()
        {
            for (var i = 0; i < 100; i++)
            {
                var v = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
                Assert.AreEqual(GcMath.Abs(v), UnityEngine.Mathf.Abs(v));
            }
            for (var i = 0; i < 100; i++)
            {
                var v = UnityEngine.Random.Range(float.MinValue, float.MaxValue);
                Assert.AreEqual(GcMath.Abs(v), UnityEngine.Mathf.Abs(v));
            }

            Assert.AreEqual(GcMath.Abs(float.NaN), UnityEngine.Mathf.Abs(float.NaN));
            Assert.AreEqual(GcMath.Abs(float.PositiveInfinity), UnityEngine.Mathf.Abs(float.PositiveInfinity));
            Assert.AreEqual(GcMath.Abs(float.NegativeInfinity), UnityEngine.Mathf.Abs(float.NegativeInfinity));
        }

        [Test]
        public void GcMathAlmostSame()
        {
            for (var i = 0; i < 100; i++)
            {
                var v1 = UnityEngine.Random.Range(float.MinValue, float.MaxValue - 1);
                var v2 = v1 + math.EPSILON;
                var v3 = v1 - math.EPSILON;
                Assert.IsTrue(GcMath.AlmostSame(v1, v2));
                Assert.IsTrue(GcMath.AlmostSame(v1, v3));
            }
            for (var i = 0; i < 100; i++)
            {
                var v1 = new float2(UnityEngine.Random.Range(float.MinValue, float.MaxValue - 1), UnityEngine.Random.Range(float.MinValue, float.MaxValue - 1));
                var v2 = v1 + new float2(math.EPSILON, math.EPSILON);
                var v3 = v1 + new float2(math.EPSILON, -math.EPSILON);
                var v4 = v1 + new float2(-math.EPSILON, -math.EPSILON);
                Assert.IsTrue(GcMath.AlmostSame(v1, v2));
                Assert.IsTrue(GcMath.AlmostSame(v1, v3));
                Assert.IsTrue(GcMath.AlmostSame(v1, v4));
            }

            Assert.IsTrue(GcMath.AlmostSame(float.PositiveInfinity, float.PositiveInfinity));
            Assert.IsTrue(GcMath.AlmostSame(float.NegativeInfinity, float.NegativeInfinity));
            Assert.IsFalse(GcMath.AlmostSame(0, float.PositiveInfinity));
            Assert.IsFalse(GcMath.AlmostSame(0, float.NegativeInfinity));
            Assert.IsFalse(GcMath.AlmostSame(float.PositiveInfinity, float.NegativeInfinity));
            Assert.IsFalse(GcMath.AlmostSame(float.NaN, float.NaN));
        }

        [Test]
        public void GcMathAlmostZero()
        {
            for (var i = 0; i < 100; i++)
            {
                var v = UnityEngine.Random.value * math.EPSILON;
                Assert.IsTrue(GcMath.AlmostZero(v));
            }
            for (var i = 0; i < 100; i++)
            {
                var v = UnityEngine.Random.Range(1f, float.MaxValue);
                Assert.IsFalse(GcMath.AlmostZero(v));
            }
            for (var i = 0; i < 100; i++)
            {
                var v = UnityEngine.Random.Range(float.MinValue, -1f);
                Assert.IsFalse(GcMath.AlmostZero(v));
            }
        }

        [Test]
        public void GcMathAtan2()
        {
            for (var i = 0; i < 100; i++)
            {
                var x = UnityEngine.Random.value;
                var y = UnityEngine.Random.value;

                var a1 = GcMath.Atan2(new float2(x, y));
                var a2 = math.degrees(math.atan2(y, x));
                Assert.IsTrue(GcMath.AlmostSame(a1, a2), $"{a1:e} != {a2:e}");
            }
        }

        [Test]
        public void GcMathCos()
        {
            Assert.IsTrue(GcMath.AlmostSame(GcMath.Cos(0f), 1f), $"{GcMath.Cos(0f):e}");
            //Assert.IsTrue(GcMath.AlmostSame(GcMath.Cos(90f), 0f), $"{GcMath.Cos(90f):e}");
            Assert.IsTrue(GcMath.AlmostSame(GcMath.Cos(180f), -1f), $"{GcMath.Cos(180f):e}");
            //Assert.IsTrue(GcMath.AlmostSame(GcMath.Cos(270f), 0f), $"{GcMath.Cos(270f):e}");
        }

        [Test]
        public void GcMathMin()
        {
            for (var i = 0; i < 100; i++)
            {
                var a = UnityEngine.Random.value;
                var b = UnityEngine.Random.value;
                Assert.AreEqual(GcMath.Min(a, b), UnityEngine.Mathf.Min(a, b));
            }
        }

        [Test]
        public void GcMathMax()
        {
            for (var i = 0; i < 100; i++)
            {
                var a = UnityEngine.Random.value;
                var b = UnityEngine.Random.value;
                Assert.AreEqual(GcMath.Max(a, b), UnityEngine.Mathf.Max(a, b));
            }
        }

        [Test]
        public void GcMathRandom()
        {
            var seed = (uint)(1 + UnityEngine.Random.value * (uint.MaxValue - 1));
            GcMath.SetRandomSeed(seed);

            for (var i = 0; i < 100; i++)
            {
                var v = GcMath.Random();
                Assert.IsTrue(v > 0f && v < 1f);
            }
        }

        [Test]
        public void GcMathRandomMinMax()
        {
            var seed = (uint)(1 + UnityEngine.Random.value * (uint.MaxValue - 1));
            GcMath.SetRandomSeed(seed);

            for (var i = 0; i < 100; i++)
            {
                var a = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
                var b = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
                var min = UnityEngine.Mathf.Min(a, b);
                var max = UnityEngine.Mathf.Max(a, b);

                var v = GcMath.Random(min, max);
                Assert.IsTrue(v >= min && v < max);
            }
            for (var i = 0; i < 100; i++)
            {
                var a = UnityEngine.Random.Range(float.MinValue, float.MaxValue);
                var b = UnityEngine.Random.Range(float.MinValue, float.MaxValue);
                var min = UnityEngine.Mathf.Min(a, b);
                var max = UnityEngine.Mathf.Max(a, b);

                var v = GcMath.Random(min, max);
                Assert.IsTrue(v >= min && v < max);
            }
        }
    }
}
