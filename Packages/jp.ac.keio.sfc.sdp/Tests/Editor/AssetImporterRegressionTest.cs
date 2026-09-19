#nullable enable
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace GameCanvas.Editor.Tests
{
    public class AssetImporterRegressionTest
    {
        const string Path = "Assets/Res/__gc_importer_regression.asset";
        [Test]
        public void NativeTextureAsset_DoesNotCastNativeImporterToTextureImporter()
        {
            Assert.That(AssetDatabase.LoadMainAssetAtPath(Path), Is.Null, "既存ファイルを保全するため中止");
            var texture = new Texture2D(4, 4);
            try
            {
                AssetDatabase.CreateAsset(texture, Path);
                Assert.That(AssetImporter.GetAtPath(Path), Is.Not.InstanceOf<TextureImporter>());
                var method = typeof(GcEditorResourceBuilder).GetMethod("ValidateImages", BindingFlags.NonPublic | BindingFlags.Static);
                Assert.That(method, Is.Not.Null);
                Assert.DoesNotThrow(() => method!.Invoke(null, null));
                Assert.That(AssetDatabase.LoadAssetAtPath<Texture2D>(Path), Is.Not.Null);
            }
            finally { AssetDatabase.DeleteAsset(Path); }
        }
    }
}
