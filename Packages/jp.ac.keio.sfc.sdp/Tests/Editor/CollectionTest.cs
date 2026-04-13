#nullable enable
using NUnit.Framework;
using UnityEngine;

namespace GameCanvas.Editor.Tests
{
    public class CollectionTest
    {
        const float Delta = 0.001f;
        // ====================================================================
        // DictWithLife<TKey, TValue>
        // ====================================================================

        [Test]
        public void DictWithLife_Issue_ThenTryGetValue_ReturnsTrue()
        {
            var dict = new DictWithLife<int, TestObj>();
            dict.Issue(1, out var issued);
            Assert.IsNotNull(issued);

            var found = dict.TryGetValue(1, out var retrieved);
            Assert.IsTrue(found);
            Assert.AreSame(issued, retrieved);
        }

        [Test]
        public void DictWithLife_TryGetValue_UnknownKey_ReturnsFalse()
        {
            var dict = new DictWithLife<int, TestObj>();
            var found = dict.TryGetValue(42, out _);
            Assert.IsFalse(found);
        }

        [Test]
        public void DictWithLife_DecrementLife_RemovesExpiredEntries()
        {
            var dict = new DictWithLife<int, TestObj>();
            dict.Issue(1, out _, life: 2);

            // First decrement: life 2 -> 1, still alive
            dict.DecrementLife();
            Assert.IsTrue(dict.TryGetValue(1, out _));

            // TryGetValue above refreshed the entry. Decrement again.
            dict.DecrementLife();

            // Entry was refreshed, so still alive after one more decrement.
            // Now do NOT access it and decrement: should expire.
            dict.DecrementLife();
            Assert.IsFalse(dict.TryGetValue(1, out _));
        }

        [Test]
        public void DictWithLife_ReleaseAll_ClearsAllEntries()
        {
            var dict = new DictWithLife<int, TestObj>();
            dict.Issue(1, out _);
            dict.Issue(2, out _);

            dict.ReleaseAll();

            Assert.IsFalse(dict.TryGetValue(1, out _));
            Assert.IsFalse(dict.TryGetValue(2, out _));
        }

        [Test]
        public void DictWithLife_Issue_DuplicateKey_Throws()
        {
            var dict = new DictWithLife<int, TestObj>();
            dict.Issue(1, out _);

            Assert.Throws<System.InvalidOperationException>(() =>
            {
                dict.Issue(1, out _);
            });
        }

        [Test]
        public void DictWithLife_ReleaseAll_RecyclesInstances()
        {
            var dict = new DictWithLife<int, TestObj>();
            dict.Issue(1, out var first);
            dict.ReleaseAll();

            // After release, re-issuing should reuse the pooled instance
            dict.Issue(2, out var second);
            Assert.AreSame(first, second);
        }

        // ====================================================================
        // ObjectPool<T>
        // ====================================================================

        [Test]
        public void ObjectPool_Get_EmptyPool_ThrowsInvalidOperation()
        {
            var pool = new ObjectPool<TestObj>();
            Assert.Throws<System.InvalidOperationException>(() => pool.Get());
        }

        [Test]
        public void ObjectPool_GetOrCreate_EmptyPool_CreatesNewInstance()
        {
            var pool = new ObjectPool<TestObj>();
            var obj = pool.GetOrCreate();
            Assert.IsNotNull(obj);
        }

        [Test]
        public void ObjectPool_ReleaseAll_ThenGet_ReusesInstance()
        {
            var pool = new ObjectPool<TestObj>();
            var first = pool.GetOrCreate();
            pool.ReleaseAll();

            var second = pool.Get();
            Assert.AreSame(first, second);
        }

        [Test]
        public void ObjectPool_InitialSize_PreallocatesInstances()
        {
            var pool = new ObjectPool<TestObj>(capacity: 4, initialSize: 3);
            // Should be able to Get (not GetOrCreate) 3 times without error
            var a = pool.Get();
            var b = pool.Get();
            var c = pool.Get();
            Assert.IsNotNull(a);
            Assert.IsNotNull(b);
            Assert.IsNotNull(c);
            Assert.AreNotSame(a, b);
            Assert.AreNotSame(b, c);
        }

        [Test]
        public void ObjectPool_MultipleGetReturn_Cycles()
        {
            var pool = new ObjectPool<TestObj>(initialSize: 1);
            var first = pool.GetOrCreate();
            pool.ReleaseAll();

            var second = pool.GetOrCreate();
            Assert.AreSame(first, second);

            pool.ReleaseAll();
            var third = pool.GetOrCreate();
            Assert.AreSame(first, third);
        }

        [Test]
        public void ObjectPool_GetOrCreate_BeyondInitialSize_StillWorks()
        {
            var pool = new ObjectPool<TestObj>(initialSize: 1);
            var a = pool.GetOrCreate(); // from pool
            var b = pool.GetOrCreate(); // newly created
            Assert.IsNotNull(a);
            Assert.IsNotNull(b);
            Assert.AreNotSame(a, b);
        }

        // ====================================================================
        // GcStyle
        // ====================================================================

        [Test]
        public void GcStyle_Default_HasExpectedValues()
        {
            var s = GcStyle.Default;
            Assert.AreEqual(24, s.CircleResolution);
            Assert.AreEqual(Color.black, s.Color);
            Assert.AreEqual(25, s.FontSize);
            Assert.AreEqual(GcLineCap.Butt, s.LineCap);
            Assert.AreEqual(1f, s.LineWidth, Delta);
            Assert.AreEqual(GcAnchor.UpperLeft, s.RectAnchor);
            Assert.AreEqual(GcAnchor.UpperLeft, s.StringAnchor);
            Assert.AreEqual(10f, s.CornerRadius, Delta);
        }

        [Test]
        public void GcStyle_Equals_SameValues_ReturnsTrue()
        {
            var a = GcStyle.Default;
            var b = GcStyle.Default;
            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(a.Equals((object)b));
        }

        [Test]
        public void GcStyle_Equals_DifferentColor_ReturnsFalse()
        {
            var a = GcStyle.Default;
            var b = GcStyle.Default;
            b.Color = Color.red;
            Assert.IsFalse(a.Equals(b));
        }

        [Test]
        public void GcStyle_Equals_DifferentFontSize_ReturnsFalse()
        {
            var a = GcStyle.Default;
            var b = GcStyle.Default;
            b.FontSize = 100;
            Assert.IsFalse(a.Equals(b));
        }

        [Test]
        public void GcStyle_GetHashCode_SameValues_SameHash()
        {
            var a = GcStyle.Default;
            var b = GcStyle.Default;
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        // ====================================================================
        // Helper
        // ====================================================================

        sealed class TestObj
        {
            public int Value;
        }
    }
}
