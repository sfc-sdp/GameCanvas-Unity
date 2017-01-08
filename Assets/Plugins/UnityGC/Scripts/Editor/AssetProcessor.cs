/*------------------------------------------------------------*/
/// <summary>GameCanvas for Unity [Asset Processor]</summary>
/// <author>Seibe TAKAHASHI</author>
/// <remarks>
/// (c) 2015-2017 Smart Device Programming.
/// This software is released under the MIT License.
/// http://opensource.org/licenses/mit-license.php
/// </remarks>
/*------------------------------------------------------------*/
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace GameCanvas.Editor
{
    /// <summary>
    /// アセットの後処理
    /// </summary>
    public class AssetProcessor : AssetPostprocessor
    {
        const string ResourceDir        = "Assets/Res";
        const string ResourceImagePrefix= ResourceDir + "/img";
        const string ResourceSoundPrefix= ResourceDir + "/snd";
        const string PackingTag         = "GCAtlas";
        const string SpriteDir          = "Assets/Plugins/UnityGC/Sprites";
        const string RectPath           = SpriteDir + "/Rect.png";
        const string CirclePath         = SpriteDir + "/Circle.png";
        const string DummyPath          = SpriteDir + "/Dummy.png";
        const string FontPath           = SpriteDir + "/PixelMplus10.png";
        const string MaterialDir        = "Assets/Plugins/UnityGC/Materials";
        const string MaterialPath       = MaterialDir + "/GCSpriteDefault.mat";
        static bool willRebuildAssetDB  = false;

        void OnPreprocessTexture()
        {
            var path = assetImporter.assetPath;

            if (path.IndexOf(ResourceImagePrefix) == 0)
            {
                // インポートした画像をパッキングタグ付きスプライトにします
                var importer = (TextureImporter)assetImporter;
                importer.textureType = TextureImporterType.Sprite;
#if UNITY_5_4
                importer.textureFormat = TextureImporterFormat.AutomaticCompressed;
                importer.SetPlatformTextureSettings("Android", 2048, TextureImporterFormat.ETC2_RGBA8, 80, true);
                importer.SetPlatformTextureSettings("iPhone", 2048, TextureImporterFormat.PVRTC_RGBA4, 80, true);
#else
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings()
                {
                    name = "Android",
                    maxTextureSize = 2048,
                    format = TextureImporterFormat.ETC2_RGBA8,
                    compressionQuality = 80,
                    textureCompression = TextureImporterCompression.CompressedHQ,
                    allowsAlphaSplitting = true,
                    overridden = true
                });
                importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings()
                {
                    name = "iPhone",
                    maxTextureSize = 2048,
                    format = TextureImporterFormat.PVRTC_RGBA4,
                    compressionQuality = 80,
                    textureCompression = TextureImporterCompression.CompressedHQ,
                    allowsAlphaSplitting = true,
                    overridden = true
                });
#endif
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 1f;
                importer.spritePackingTag = PackingTag;
                
                var so = new SerializedObject(importer);
                var sp = so.FindProperty("m_Alignment");
                if (sp.intValue != (int)SpriteAlignment.TopLeft)
                {
                    sp.intValue = (int)SpriteAlignment.TopLeft;
                    so.ApplyModifiedProperties();
                }

                if (!willRebuildAssetDB)
                {
                    EditorApplication.update += RebuildAssetDatabase;
                    willRebuildAssetDB = true;
                }
            }
            else if (path.IndexOf(SpriteDir + "/") == 0)
            {
                // インポートした画像をパッキングタグ付きスプライトにします
                var importer = (TextureImporter)assetImporter;
                importer.textureType = TextureImporterType.Sprite;
#if UNITY_5_4
                importer.textureFormat = TextureImporterFormat.AutomaticCompressed;
                importer.SetPlatformTextureSettings("Android", 2048, TextureImporterFormat.ETC2_RGBA8, 80, true);
                importer.SetPlatformTextureSettings("iPhone", 2048, TextureImporterFormat.PVRTC_RGBA4, 80, true);
#else
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings()
                {
                    name = "Android",
                    maxTextureSize = 2048,
                    format = TextureImporterFormat.ETC2_RGBA8,
                    compressionQuality = 80,
                    textureCompression = TextureImporterCompression.CompressedHQ,
                    allowsAlphaSplitting = true,
                    overridden = true
                });
                importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings()
                {
                    name = "iPhone",
                    maxTextureSize = 2048,
                    format = TextureImporterFormat.PVRTC_RGBA4,
                    compressionQuality = 80,
                    textureCompression = TextureImporterCompression.CompressedHQ,
                    allowsAlphaSplitting = true,
                    overridden = true
                });
#endif
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.spriteImportMode = path.Contains("Circle") ? SpriteImportMode.Polygon :
                                            path.Contains("PixelMplus10") ? SpriteImportMode.Multiple : SpriteImportMode.Single;
                importer.spritePackingTag = path.Contains("Dummy") ? string.Empty : PackingTag;
            }
        }

        void OnPreprocessAudio()
        {
            var path = assetImporter.assetPath;

            if (path.IndexOf(ResourceSoundPrefix) == 0)
            {
                // インポートした音声の圧縮を設定します
                var audioImporter = (AudioImporter)assetImporter;
                var setting = audioImporter.defaultSampleSettings;
                setting.compressionFormat = AudioCompressionFormat.ADPCM;
                setting.loadType = AudioClipLoadType.Streaming;
                audioImporter.defaultSampleSettings = setting;

                if (!willRebuildAssetDB)
                {
                    EditorApplication.update += RebuildAssetDatabase;
                    willRebuildAssetDB = true;
                }
            }
        }

        public static void RebuildAssetDatabase()
        {
            if (willRebuildAssetDB)
            {
                EditorApplication.update -= RebuildAssetDatabase;
                willRebuildAssetDB = false;
            }
            var projectDir = Path.GetDirectoryName(Application.dataPath);

            // 画像データの取得
            var sprites = new Dictionary<int, Sprite>();
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2d", new string[1] { ResourceDir }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                var index = int.Parse(sprite.name.Substring(3));
                sprites.Add(index, sprite);
            }

            // 音声データの取得
            var clips = new Dictionary<int, AudioClip>();
            foreach (var guid in AssetDatabase.FindAssets("t:AudioClip", new string[1] { ResourceDir }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                var index = int.Parse(clip.name.Substring(3));
                clips.Add(index, clip);
            }

            // 図形データの取得
            var rect = AssetDatabase.LoadAssetAtPath<Sprite>(RectPath);
            var circle = AssetDatabase.LoadAssetAtPath<Sprite>(CirclePath);
            var dummy = AssetDatabase.LoadAssetAtPath<Sprite>(DummyPath);

            // 文字データの取得
            var characters = new List<Sprite>();
            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(FontPath))
            {
                if (obj is Sprite)
                {
                    characters.Add(obj as Sprite);
                }
            }

            // マテリアルの取得
            var mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            
            // データベースの作成
            var db = ScriptableObject.CreateInstance<GameCanvasAssetDB>();
            db.images = sprites.OrderBy(x => x.Key).Select(x => x.Value).ToArray();
            db.sounds = clips.OrderBy(x => x.Key).Select(x => x.Value).ToArray();
            db.rect   = rect;
            db.circle = circle;
            db.dummy  = dummy;
            db.characters = characters.ToArray();
            db.material = mat;

            // データベースの保存
            var absoluteDBPath = Path.Combine(projectDir, GameCanvasAssetDB.Path);
            if (File.Exists(absoluteDBPath))
            {
                var tempPath = GameCanvasAssetDB.Path + ".new.asset";
                var absoluteTempPath = Path.Combine(projectDir, tempPath);
                AssetDatabase.CreateAsset(db, tempPath);
                File.Delete(absoluteDBPath);
                File.Move(absoluteTempPath, absoluteDBPath);
                File.Delete(absoluteTempPath + ".meta");
                AssetDatabase.Refresh();
            }
            else
            {
                AssetDatabase.CreateAsset(db, GameCanvasAssetDB.Path);
            }
        }
    }
}
