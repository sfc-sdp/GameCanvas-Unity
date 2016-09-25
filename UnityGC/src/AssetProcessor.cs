/**
 * GameCanvas for Unity [Asset Processor]
 * 
 * Copyright (c) 2015-2016 Seibe TAKAHASHI
 * 
 * This software is released under the MIT License.
 * http://opensource.org/licenses/mit-license.php
 */
using UnityEngine;
using UnityEditor;

namespace GameCanvas.Editor
{
    class AssetProcessor : AssetPostprocessor
    {
#if !GC_DISABLE_PREPROCESSOR
        void OnPreprocessTexture()
        {
            // インポートした画像を、スクリプトから読み込み可能にします
            var importar = (TextureImporter)assetImporter;
            importar.textureType         = TextureImporterType.Advanced;
            importar.textureFormat       = TextureImporterFormat.AutomaticTruecolor;
            importar.filterMode          = FilterMode.Point;
            importar.spritePixelsPerUnit = 1f;
            importar.mipmapEnabled       = false;
        }

        void OnPreprocessAudio()
        {
            var audioImporter = (AudioImporter)assetImporter;
            var setting = audioImporter.defaultSampleSettings;
            setting.compressionFormat   = AudioCompressionFormat.ADPCM;
            setting.loadType            = AudioClipLoadType.Streaming;
            audioImporter.defaultSampleSettings = setting;
        }
#endif
    }
}
