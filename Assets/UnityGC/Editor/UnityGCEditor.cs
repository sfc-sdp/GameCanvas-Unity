using UnityEngine;
using UnityEditor;

namespace GameCanvas
{
    namespace Editor
    {
        public class GameCanvasAssetProcessor : AssetPostprocessor
        {
            void OnPreprocessTexture()
            {
                // インポートした画像を、スクリプトから読み込み可能にします
                TextureImporter textureImporter = (TextureImporter)assetImporter;
                textureImporter.textureType = TextureImporterType.Advanced;
                textureImporter.textureFormat = TextureImporterFormat.AutomaticTruecolor;
                textureImporter.mipmapEnabled = false;
                textureImporter.isReadable = true;
            }
        }
    }
}
