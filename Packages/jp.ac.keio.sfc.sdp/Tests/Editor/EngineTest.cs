#nullable enable
using GameCanvas;
using GameCanvas.Engine;
using NUnit.Framework;
using UnityEngine;

namespace GameCanvas.Editor.Tests
{
    public class GcTimeEngineTest
    {
        GcTimeEngine m_Engine = null!;
        int m_OriginalVSyncCount;
        int m_OriginalTargetFrameRate;

        [SetUp]
        public void SetUp()
        {
            m_OriginalVSyncCount = QualitySettings.vSyncCount;
            m_OriginalTargetFrameRate = Application.targetFrameRate;
            m_Engine = new GcTimeEngine();
        }

        [TearDown]
        public void TearDown()
        {
            QualitySettings.vSyncCount = m_OriginalVSyncCount;
            Application.targetFrameRate = m_OriginalTargetFrameRate;
        }

        [Test]
        public void Constructor_CurrentFrameIsZero()
        {
            Assert.AreEqual(0, m_Engine.CurrentFrame);
        }

        [Test]
        public void Constructor_TargetFrameRateIs60()
        {
            Assert.AreEqual(60, m_Engine.TargetFrameRate);
        }

        [Test]
        public void Constructor_VSyncEnabledIsTrue()
        {
            Assert.IsTrue(m_Engine.VSyncEnabled);
        }

        [Test]
        public void OnBeforeUpdate_IncrementsFrameCount()
        {
            var engine = (IEngine)m_Engine;
            engine.OnBeforeUpdate(System.DateTimeOffset.Now);
            Assert.AreEqual(1, m_Engine.CurrentFrame);
        }

        [Test]
        public void OnBeforeUpdate_Twice_TimeSincePrevFrameReflectsDelta()
        {
            var engine = (IEngine)m_Engine;
            var t1 = new System.DateTimeOffset(2026, 1, 1, 12, 0, 0, System.TimeSpan.Zero);
            engine.OnBeforeUpdate(t1);

            var t2 = t1.AddSeconds(0.05);
            engine.OnBeforeUpdate(t2);

            Assert.AreEqual(2, m_Engine.CurrentFrame);
            Assert.AreEqual(0.05f, m_Engine.TimeSincePrevFrame, 0.001f);
        }

        [Test]
        public void SetFrameRate_30_UpdatesTargetFrameRate()
        {
            m_Engine.SetFrameRate(30);
            Assert.AreEqual(30, m_Engine.TargetFrameRate);
        }

        [Test]
        public void SetFrameRate_30_UpdatesTargetFrameInterval()
        {
            m_Engine.SetFrameRate(30);
            Assert.AreEqual(1.0 / 30.0, m_Engine.TargetFrameInterval, 0.001);
        }

        [Test]
        public void SetFrameRate_WithVSyncDisabled()
        {
            m_Engine.SetFrameRate(30, false);
            Assert.AreEqual(30, m_Engine.TargetFrameRate);
            Assert.IsFalse(m_Engine.VSyncEnabled);
        }

        [Test]
        public void CurrentTimestamp_DeterministicWithFixedTime()
        {
            var engine = (IEngine)m_Engine;
            // 2026-01-01T00:00:00Z
            var fixedTime = new System.DateTimeOffset(2026, 1, 1, 0, 0, 0, System.TimeSpan.Zero);
            engine.OnBeforeUpdate(fixedTime);

            var ts = m_Engine.CurrentTimestamp;
            // Compute expected value using the same local-epoch logic as the engine
            var expected = (long)((fixedTime - GcTimeEngine.k_UnixZero).TotalSeconds);
            Assert.AreEqual(expected, ts);
        }

        [Test]
        public void CurrentTime_ReturnsDateTimeOffset()
        {
            var engine = (IEngine)m_Engine;
            var fixedTime = new System.DateTimeOffset(2026, 6, 15, 10, 30, 0, System.TimeSpan.Zero);
            engine.OnBeforeUpdate(fixedTime);

            var current = m_Engine.CurrentTime;
            Assert.AreEqual(fixedTime, current);
        }
    }

    public class GcStorageEngineTest
    {
        const string k_Prefix = "gctest_";

        [SetUp]
        public void SetUp()
        {
            // Clean up only test-owned keys
            var keys = new[]
            {
                "str_key", "del_key", "no_such_key", "float_key", "float_del",
                "int_key", "int_del", "no_float", "no_int", "k1", "k2", "k3"
            };
            foreach (var key in keys)
            {
                PlayerPrefs.DeleteKey(k_Prefix + key);
            }
        }

        [Test]
        public void Save_StringValue_TryLoad_RoundTrip()
        {
            var engine = CreateEngine();
            engine.Save(k_Prefix + "str_key", "world");

            var result = engine.TryLoad(k_Prefix + "str_key", out string? value);
            Assert.IsTrue(result);
            Assert.AreEqual("world", value);
        }

        [Test]
        public void Save_NullString_DeletesKey()
        {
            var engine = CreateEngine();
            engine.Save(k_Prefix + "del_key", "temp");
            Assert.IsTrue(PlayerPrefs.HasKey(k_Prefix + "del_key"));

            engine.Save(k_Prefix + "del_key", (string?)null);
            Assert.IsFalse(PlayerPrefs.HasKey(k_Prefix + "del_key"));
        }

        [Test]
        public void TryLoad_NonExistentKey_ReturnsFalse()
        {
            var engine = CreateEngine();

            var result = engine.TryLoad(k_Prefix + "no_such_key", out string? value);
            Assert.IsFalse(result);
            Assert.IsNull(value);
        }

        [Test]
        public void Save_Float_TryLoad_RoundTrip()
        {
            var engine = CreateEngine();
            engine.Save(k_Prefix + "float_key", 3.14f);

            var result = engine.TryLoad(k_Prefix + "float_key", out float value);
            Assert.IsTrue(result);
            Assert.AreEqual(3.14f, value, 0.001f);
        }

        [Test]
        public void Save_NullFloat_DeletesKey()
        {
            var engine = CreateEngine();
            engine.Save(k_Prefix + "float_del", 1.0f);
            Assert.IsTrue(PlayerPrefs.HasKey(k_Prefix + "float_del"));

            engine.Save(k_Prefix + "float_del", (float?)null);
            Assert.IsFalse(PlayerPrefs.HasKey(k_Prefix + "float_del"));
        }

        [Test]
        public void Save_Int_TryLoad_RoundTrip()
        {
            var engine = CreateEngine();
            engine.Save(k_Prefix + "int_key", 42);

            var result = engine.TryLoad(k_Prefix + "int_key", out int value);
            Assert.IsTrue(result);
            Assert.AreEqual(42, value);
        }

        [Test]
        public void Save_NullInt_DeletesKey()
        {
            var engine = CreateEngine();
            engine.Save(k_Prefix + "int_del", 99);
            Assert.IsTrue(PlayerPrefs.HasKey(k_Prefix + "int_del"));

            engine.Save(k_Prefix + "int_del", (int?)null);
            Assert.IsFalse(PlayerPrefs.HasKey(k_Prefix + "int_del"));
        }

        [Test]
        public void TryLoad_Float_NonExistentKey_ReturnsFalse()
        {
            var engine = CreateEngine();

            var result = engine.TryLoad(k_Prefix + "no_float", out float value);
            Assert.IsFalse(result);
            Assert.AreEqual(0f, value);
        }

        [Test]
        public void TryLoad_Int_NonExistentKey_ReturnsFalse()
        {
            var engine = CreateEngine();

            var result = engine.TryLoad(k_Prefix + "no_int", out int value);
            Assert.IsFalse(result);
            Assert.AreEqual(0, value);
        }

        [Test]
        public void EraseSavedDataAll_ClearsAllData()
        {
            var engine = CreateEngine();
            engine.Save(k_Prefix + "k1", "v1");
            engine.Save(k_Prefix + "k2", 2.0f);
            engine.Save(k_Prefix + "k3", 3);

            engine.EraseSavedDataAll();

            Assert.IsFalse(PlayerPrefs.HasKey(k_Prefix + "k1"));
            Assert.IsFalse(PlayerPrefs.HasKey(k_Prefix + "k2"));
            Assert.IsFalse(PlayerPrefs.HasKey(k_Prefix + "k3"));
        }

        /// <summary>
        /// Creates a GcStorageEngine without calling its constructor.
        /// Save/TryLoad/EraseSavedDataAll only use PlayerPrefs and never
        /// touch m_Context, so this is safe for those methods.
        /// Do NOT call SaveScreenshotAsync on this instance (it requires m_Context).
        /// </summary>
        static GcStorageEngine CreateEngine()
        {
            return (GcStorageEngine)System.Runtime.Serialization
                .FormatterServices.GetUninitializedObject(typeof(GcStorageEngine));
        }
    }
}
