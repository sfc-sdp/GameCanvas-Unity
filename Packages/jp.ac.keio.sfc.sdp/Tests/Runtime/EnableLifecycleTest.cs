#nullable enable
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace GameCanvas.Tests
{
    public sealed class EnableLifecycleProbe : GameBase
    {
        internal IGameCanvas Canvas => gc;
        internal int Inits, Updates;
        public override void InitGame() { Inits++; }
        public override void UpdateGame() { Updates++; }
        public override void DrawGame() { }
    }
    public class EnableLifecycleTest
    {
        [UnityTest] public IEnumerator ReenableRecreatesDisposedServicesAndRunsOnlyOneLoop()
        {
            var go=new GameObject("Reenable probe",typeof(Camera),typeof(AudioListener));
            try
            {
                var probe=go.AddComponent<EnableLifecycleProbe>();yield return null;yield return null;
                var first=probe.Canvas;int updates=probe.Updates;
                probe.enabled=false;yield return null;yield return null;
                Assert.That(probe.Updates,Is.EqualTo(updates));
                Assert.Throws<System.ObjectDisposedException>(()=>first.Network.GetText("https://example.com"));
                probe.enabled=true;yield return null;yield return null;
                Assert.That(probe.Canvas,Is.Not.SameAs(first));Assert.That(probe.Inits,Is.EqualTo(2));
                Assert.That(probe.Updates,Is.GreaterThan(updates));
                probe.enabled=false;updates=probe.Updates;yield return null;
                Assert.That(probe.Updates,Is.EqualTo(updates));
            }
            finally {Object.DestroyImmediate(go);}
        }
    }
}
