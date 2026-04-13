#nullable enable
using System.Collections;
using GameCanvas;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GameCanvas.Tests
{
    /// <summary>
    /// GameBase subclass for testing. All abstract methods have empty default
    /// implementations via GameBase, so nothing extra is needed here.
    /// </summary>
    public sealed class TestBehaviourBase : GameBase
    {
        /// <summary>
        /// Expose the protected <c>gc</c> property for test assertions.
        /// </summary>
        public IGameCanvas GcProxy => gc;
    }

    public class PlaymodeTest
    {
        private GameObject? _go;
        private TestBehaviourBase? _behaviour;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("TestBehaviourBase",
                typeof(Camera),
                typeof(AudioListener),
                typeof(TestBehaviourBase));

            _behaviour = _go.GetComponent<TestBehaviourBase>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null)
            {
                Object.Destroy(_go);
                _go = null;
            }
            _behaviour = null;
        }

        // ----------------------------------------------------------------
        // Lifecycle tests
        // ----------------------------------------------------------------

        [UnityTest]
        public IEnumerator GcProxy_IsNotNull_AfterInitialization()
        {
            yield return null;

            Assert.IsNotNull(_behaviour);
            Assert.IsNotNull(_behaviour!.GcProxy,
                "gc (GcProxy) should be initialised after Awake/OnEnable");
        }

        [UnityTest]
        public IEnumerator CanvasSize_MatchesWidthAndHeight()
        {
            yield return null;

            var proxy = _behaviour!.GcProxy;
            var size = proxy.CanvasSize;
            Assert.AreEqual(size.x, proxy.CanvasWidth,
                "CanvasSize.x should equal CanvasWidth");
            Assert.AreEqual(size.y, proxy.CanvasHeight,
                "CanvasSize.y should equal CanvasHeight");

            // Default canvas size is 720x1280
            Assert.AreEqual(720, proxy.CanvasWidth,
                "Default CanvasWidth should be 720");
            Assert.AreEqual(1280, proxy.CanvasHeight,
                "Default CanvasHeight should be 1280");
        }

        [UnityTest]
        public IEnumerator TimeSinceStartup_IsNonNegative()
        {
            yield return null;

            Assert.GreaterOrEqual(_behaviour!.GcProxy.TimeSinceStartup, 0f,
                "TimeSinceStartup should be >= 0");
        }

        // ----------------------------------------------------------------
        // GcProxy delegation smoke tests
        // ----------------------------------------------------------------

        [UnityTest]
        public IEnumerator CurrentFrame_IsNonNegative()
        {
            yield return null;

            Assert.GreaterOrEqual(_behaviour!.GcProxy.CurrentFrame, 0,
                "CurrentFrame should be >= 0");
        }

        [UnityTest]
        public IEnumerator CanvasWidth_IsPositive()
        {
            yield return null;

            Assert.Greater(_behaviour!.GcProxy.CanvasWidth, 0,
                "CanvasWidth should be > 0");
        }

        [UnityTest]
        public IEnumerator CanvasHeight_IsPositive()
        {
            yield return null;

            Assert.Greater(_behaviour!.GcProxy.CanvasHeight, 0,
                "CanvasHeight should be > 0");
        }

        [UnityTest]
        public IEnumerator ColorWhite_EqualsWhite()
        {
            yield return null;

            var white = _behaviour!.GcProxy.ColorWhite;
            Assert.AreEqual(1f, white.r, 0.001f, "ColorWhite.r should be 1");
            Assert.AreEqual(1f, white.g, 0.001f, "ColorWhite.g should be 1");
            Assert.AreEqual(1f, white.b, 0.001f, "ColorWhite.b should be 1");
            Assert.AreEqual(1f, white.a, 0.001f, "ColorWhite.a should be 1");
        }

        [UnityTest]
        public IEnumerator ColorBlack_EqualsBlack()
        {
            yield return null;

            var black = _behaviour!.GcProxy.ColorBlack;
            Assert.AreEqual(0f, black.r, 0.001f, "ColorBlack.r should be 0");
            Assert.AreEqual(0f, black.g, 0.001f, "ColorBlack.g should be 0");
            Assert.AreEqual(0f, black.b, 0.001f, "ColorBlack.b should be 0");
            Assert.AreEqual(1f, black.a, 0.001f, "ColorBlack.a should be 1");
        }

        [UnityTest]
        public IEnumerator PointerCount_IsZero_Initially()
        {
            yield return null;

            Assert.AreEqual(0, _behaviour!.GcProxy.PointerCount,
                "PointerCount should be 0 when no input");
        }

        [UnityTest]
        public IEnumerator KeyDownCount_IsZero_Initially()
        {
            yield return null;

            Assert.AreEqual(0, _behaviour!.GcProxy.KeyDownCount,
                "KeyDownCount should be 0 when no keys pressed");
        }

        // ----------------------------------------------------------------
        // Math delegation tests
        // ----------------------------------------------------------------

        [UnityTest]
        public IEnumerator Abs_ReturnsCorrectValues()
        {
            yield return null;

            Assert.AreEqual(5f, _behaviour!.GcProxy.Abs(-5f), 0.001f,
                "Abs(-5f) should return 5f");
            Assert.AreEqual(5, _behaviour!.GcProxy.Abs(-5),
                "Abs(-5) should return 5");
        }

        [UnityTest]
        public IEnumerator Sin_DegreesInput_ReturnsCorrectValues()
        {
            yield return null;

            var proxy = _behaviour!.GcProxy;
            // sin(0) == 0
            Assert.AreEqual(0f, proxy.Sin(0f), 0.001f,
                "Sin(0) should be ~0");
            // sin(90) == 1
            Assert.AreEqual(1f, proxy.Sin(90f), 0.001f,
                "Sin(90) should be ~1");
            // sin(180) == 0
            Assert.AreEqual(0f, proxy.Sin(180f), 0.001f,
                "Sin(180) should be ~0");
            // sin(-90) == -1
            Assert.AreEqual(-1f, proxy.Sin(-90f), 0.001f,
                "Sin(-90) should be ~-1");
        }

        // ----------------------------------------------------------------
        // Physics delegation tests
        // ----------------------------------------------------------------

        [UnityTest]
        public IEnumerator HitTest_OverlappingAABBs_ReturnsTrue()
        {
            yield return null;

            // Two 100x100 boxes overlapping at origin area
            var a = GcAABB.XYWH(0f, 0f, 100f, 100f);
            var b = GcAABB.XYWH(50f, 50f, 100f, 100f);

            Assert.IsTrue(_behaviour!.GcProxy.HitTest(a, b),
                "Overlapping AABBs should return true");
        }

        [UnityTest]
        public IEnumerator HitTest_NonOverlappingAABBs_ReturnsFalse()
        {
            yield return null;

            // Two 100x100 boxes far apart
            var a = GcAABB.XYWH(0f, 0f, 100f, 100f);
            var b = GcAABB.XYWH(200f, 200f, 100f, 100f);

            Assert.IsFalse(_behaviour!.GcProxy.HitTest(a, b),
                "Non-overlapping AABBs should return false");
        }

        [UnityTest]
        public IEnumerator HitTest_EdgeTouchingAABBs_ReturnsTrue()
        {
            yield return null;

            // Two 100x100 boxes sharing an edge
            var a = GcAABB.XYWH(0f, 0f, 100f, 100f);
            var b = GcAABB.XYWH(100f, 0f, 100f, 100f);

            Assert.IsTrue(_behaviour!.GcProxy.HitTest(a, b),
                "Edge-touching AABBs should return true");
        }
    }
}
