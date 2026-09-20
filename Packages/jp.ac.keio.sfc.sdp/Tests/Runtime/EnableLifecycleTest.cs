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
        internal int Inits, Updates, Pauses, Resumes;
        public override void InitGame() { Inits++; }
        public override void UpdateGame() { Updates++; }
        public override void DrawGame() { }
        public override void PauseGame() { Pauses++; }
        public override void ResumeGame() { Resumes++; }
    }
    public class EnableLifecycleTest
    {
        [UnityTest] public IEnumerator HiddenPagePausesOnceAndWaitsForBothResumeSignals()
        {
            var go = new GameObject("Visibility probe", typeof(Camera), typeof(AudioListener));
            try
            {
                var probe = go.AddComponent<EnableLifecycleProbe>();
                yield return null;
                probe.OnWebVisibility(true);
                probe.OnWebVisibility(true);
                probe.SendMessage("OnApplicationPause", true);
                int updates = probe.Updates;
                yield return null;
                yield return null;
                Assert.That(probe.Updates, Is.EqualTo(updates));
                Assert.That(probe.Pauses, Is.EqualTo(1));
                probe.OnWebVisibility(false);
                Assert.That(probe.Resumes, Is.Zero);
                probe.SendMessage("OnApplicationPause", false);
                probe.OnWebVisibility(false);
                yield return null;
                yield return null;
                Assert.That(probe.Resumes, Is.EqualTo(1));
                Assert.That(probe.Updates, Is.GreaterThan(updates));
                probe.OnWebVisibility(true);
                probe.SendMessage("OnApplicationPause", false);
                Assert.That(probe.Resumes, Is.EqualTo(1));
                probe.OnWebVisibility(false);
                Assert.That(probe.Resumes, Is.EqualTo(2));
            }
            finally { Object.DestroyImmediate(go); }
        }

        [UnityTest] public IEnumerator ReenableWhileHiddenRemainsPausedUntilVisible()
        {
            var go = new GameObject("Hidden reenable probe", typeof(Camera), typeof(AudioListener));
            try
            {
                var probe = go.AddComponent<EnableLifecycleProbe>();
                probe.OnWebVisibility(true);
                probe.enabled = false;
                probe.enabled = true;
                int updates = probe.Updates;
                yield return null;
                yield return null;
                Assert.That(probe.Updates, Is.EqualTo(updates));
                probe.OnWebVisibility(false);
                yield return null;
                yield return null;
                Assert.That(probe.Updates, Is.GreaterThan(updates));
                Assert.That(probe.Resumes, Is.EqualTo(1));
            }
            finally { Object.DestroyImmediate(go); }
        }

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
