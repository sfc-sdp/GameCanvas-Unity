#nullable enable
using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Unity.Mathematics;

namespace GameCanvas.Editor.Tests
{
    public class RandomRangeTest
    {
        uint randomState;
        [SetUp] public void SaveRandomState() => randomState = GcMath.GetRandomState();
        [TearDown] public void RestoreRandomState() => GcMath.s_Random.state = randomState;

        [Test] public void IntegerRandomUsesAnExclusiveUpperBoundIncludingExtremeRanges()
        {
            GcMath.SetRandomSeed(123);
            for (int i = 0; i < 1000; i++)
            {
                Assert.That(GcMath.Random(1), Is.Zero);
                Assert.That(GcMath.Random(-5, -4), Is.EqualTo(-5));
                Assert.That(GcMath.Random(int.MaxValue - 1, int.MaxValue), Is.EqualTo(int.MaxValue - 1));
                Assert.That(GcMath.Random(int.MinValue, int.MaxValue), Is.LessThan(int.MaxValue));
                int[] array = { 10, 20, 30 };
                Assert.That(array, Does.Contain(array[GcMath.Random(array.Length)]));
            }
        }


        [Test] public void RandomRejectsEmptyReversedAndNonFiniteRangesWithoutAdvancingState()
        {
            GcMath.SetRandomSeed(123); uint before = GcMath.GetRandomState();
            Assert.Throws<ArgumentOutOfRangeException>(() => GcMath.Random(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => GcMath.Random(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => GcMath.Random(2, 2));
            Assert.Throws<ArgumentOutOfRangeException>(() => GcMath.Random(3, 2));
            Assert.Throws<ArgumentOutOfRangeException>(() => GcMath.Random(2f, 2f));
            Assert.Throws<ArgumentOutOfRangeException>(() => GcMath.Random(3f, 2f));
            Assert.Throws<ArgumentOutOfRangeException>(() => GcMath.Random(float.NaN, 1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => GcMath.Random(0f, float.PositiveInfinity));
            Assert.Throws<ArgumentOutOfRangeException>(() => GcMath.Random(float.NegativeInfinity, 1f));
            Assert.That(GcMath.GetRandomState(), Is.EqualTo(before));
        }


        [Test] public void FloatingRandomKeepsFiniteExclusiveBoundsEvenWhenRounding()
        {
            GcMath.SetRandomSeed(123);
            for (int i = 0; i < 1000; i++)
            {
                float v = GcMath.Random(float.MinValue, float.MaxValue);
                Assert.That(float.IsInfinity(v) || float.IsNaN(v), Is.False);
                Assert.That(v, Is.GreaterThanOrEqualTo(float.MinValue).And.LessThan(float.MaxValue));
                Assert.That(GcMath.Random(1f, math.asfloat(math.asuint(1f) + 1u)), Is.EqualTo(1f));
                Assert.That(GcMath.Random(-float.Epsilon, 0f), Is.EqualTo(-float.Epsilon));
                Assert.That(GcMath.Random(-1f, math.asfloat(math.asuint(-1f) - 1u)), Is.EqualTo(-1f));
            }
        }


        [Test] public void SeedReproducesTheSameSequence()
        {
            int[] first = new int[32]; GcMath.SetRandomSeed(123);
            for (int i = 0; i < first.Length; i++) first[i] = GcMath.Random(100);
            GcMath.SetRandomSeed(123);
            for (int i = 0; i < first.Length; i++) Assert.That(GcMath.Random(100), Is.EqualTo(first[i]));
        }
    }
}
