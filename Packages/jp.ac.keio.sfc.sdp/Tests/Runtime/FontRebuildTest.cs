#nullable enable
using System;
using System.Collections;
using System.Reflection;
using GameCanvas.Engine;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GameCanvas.Tests
{
    public sealed class FontRebuildTest
    {
        [UnityTest]
        public IEnumerator AtlasRebuildDoesNotReuseAMeshAlreadyQueuedThisFrame()
        {
            var go = new GameObject("Font rebuild regression", typeof(Camera), typeof(AudioListener), typeof(TestBehaviourBase));
            try
            {
                yield return null;
                var proxy = (GcProxy)go.GetComponent<TestBehaviourBase>().GcProxy;
                var context = (GcContext)typeof(GcProxy).GetField("m_Context", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(proxy);
                var graphics = context.Graphics;
                var meshMethod = typeof(GcGraphicsEngine).GetMethod("GetOrCreateTextMesh", BindingFlags.Instance | BindingFlags.NonPublic)!;
                var rebuildMethod = typeof(GcGraphicsEngine).GetMethod("OnFontTextureRebuild", BindingFlags.Instance | BindingFlags.NonPublic)!;
                object[] firstArgs = { "日本語の表示", null!, null! };
                meshMethod.Invoke(graphics, firstArgs);
                var first = (Mesh)firstArgs[1];
                var vertices = first.vertices;
                // Unityのアトラス再生成通知と同じ入口を、描画の途中で呼ぶ。
                rebuildMethod.Invoke(graphics, new object?[] { null });
                object[] secondArgs = { "別の長さの文字列を描く", null!, null! };
                meshMethod.Invoke(graphics, secondArgs);
                Assert.That(secondArgs[1], Is.Not.SameAs(first), "描画待ちのMeshを別の文字列に再利用しない");
                CollectionAssert.AreEqual(vertices, first.vertices);
                ((IEngine)graphics).OnBeforeUpdate(DateTimeOffset.Now);
                // 次フレームではキャッシュを再生成し、引き続き文字を描ける。
                meshMethod.Invoke(graphics, firstArgs);
                Assert.That(((Mesh)firstArgs[1]).vertexCount, Is.GreaterThan(0));
                Assert.That(firstArgs[2], Is.Not.Null);
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }
    }
}
