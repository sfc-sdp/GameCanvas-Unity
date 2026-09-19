#nullable enable
using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace GameCanvas.Editor.Tests
{
    public class GeolocationTimestampTest
    {
        [Test]
        public void LocationTimestamp_UsesUnixSecondsRatherThanMinutes()
        {
            // 実測値と同じ桁数を使う。AddMinutesではDateTimeOffsetの範囲も超える。
            object info = default(LocationInfo);
            var field = typeof(LocationInfo).GetField("m_Timestamp", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "UnityのLocationInfo内部構造が変わった場合はfixtureを更新する");
            field!.SetValue(info, 1789776000d);
            var data = new GcGeolocationEvent((LocationInfo)info);
            Assert.That(data.Time, Is.EqualTo(DateTimeOffset.FromUnixTimeSeconds(1789776000)));
        }
    }
}
