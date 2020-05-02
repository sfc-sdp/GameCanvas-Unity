/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.U2D;
using UnityEngine.Video;

namespace GameCanvas.Editor
{
    public sealed class ResourceBuilder : IPreprocessBuildWithReport
    {
        //----------------------------------------------------------
        #region フィールド変数
        //----------------------------------------------------------

        private const string cGameResFolderPath = "Assets/Res";
        private const string cLibFolderPath = "Assets/GameCanvas";
        private const string cLibResAssetPath = cLibFolderPath + "/Res.asset";
        private const string cLibAtlasPath = cLibFolderPath + "/Atlas.spriteatlas";
        private const string cLibMixerPath = cLibFolderPath + "/Mixer.mixer";

        private const string cTextureImporterLabel = "GameCanvas TextureImporter 2.0";
        private const string cAudioImporterLabel = "GameCanvas AudioImporter 1.0";
        private static readonly string[] cSearchFolder = new[] { cGameResFolderPath };
        private static readonly Regex cRegImg = new Regex(@"^Assets/Res/(?<filename>img(?<id>\d+))\.(gif|GIF|png|PNG|jpg|JPG|tga|TGA|tif|TIF|tiff|TIFF|bmp|BMP|iff|IFF|pict|PICT)$");
        private static readonly Regex cRegSnd = new Regex(@"^Assets/Res/snd(?<id>\d+)\.(wav|WAV|mp3|MP3|ogg|OGG|aiff|AIFF|aif|AIF)$");
        private static readonly Regex cRegMov = new Regex(@"^Assets/Res/mov(?<id>\d+)\.(mp4|MP4|mov|MOV|mpg|MPG|mpeg|MPEG|avi|AVI|asf|ASF|dv|DV|ogv|OGV|vp8|VP8|webm|WEBM|wmv|WMV)$");
        private static readonly Regex cRegTxt = new Regex(@"^Assets/Res/txt(?<id>\d+)\.(txt|TXT|bytes|BYTES|json|JSON|xml|XML|csv|CSV|yaml|YAML)$");
        private static readonly Regex cRegFnt = new Regex(@"^Assets/Res/fnt(?<id>\d+)\.(ttf|TTF|otf|OTF)$");

        #endregion

        //----------------------------------------------------------
        #region プライベート関数
        //----------------------------------------------------------

        int IOrderedCallback.callbackOrder { get { return 0; } }
        void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report) { OnExitEditMode(); }

        [InitializeOnLoadMethod]
#pragma warning disable IDE0051
        private static void OnInitialize()
#pragma warning restore IDE0051
        {
            EditorApplication.playModeStateChanged += (PlayModeStateChange state) =>
            {
                if (state == PlayModeStateChange.ExitingEditMode) OnExitEditMode();
            };
            if (!EditorApplication.isCompiling)
            {
                EditorApplication.delayCall += new EditorApplication.CallbackFunction(OnCompiled);
            }
        }

        private static void OnCompiled()
        {
            if (!File.Exists(cLibResAssetPath))
            {
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<Resource>(), cLibResAssetPath);
            }
        }

        private static void OnExitEditMode()
        {
            AssetDatabase.StartAssetEditing();
            try
            {
                ValidateImages();
                ValidateSounds();
                Listup();
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        private static void ValidateImages()
        {
            var importers = AssetDatabase.FindAssets("t:Texture2D", cSearchFolder)
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => cRegImg.Match(path))
                .Where(match => match.Success)
                .OrderBy(match => int.Parse(match.Groups["id"].Value))
                .Select(match => AssetImporter.GetAtPath(match.Value) as TextureImporter)
                .Where(importer => importer != null);

            foreach (var importer in importers)
            {
                if (importer.userData != cTextureImporterLabel)
                {
                    var settings = new TextureImporterSettings();
                    importer.ReadTextureSettings(settings);
                    {
                        settings.filterMode = FilterMode.Point;
                        settings.mipmapEnabled = false;
                        settings.readable = false;
                        settings.spriteAlignment = (int)SpriteAlignment.TopLeft;
                        settings.spriteMeshType = SpriteMeshType.Tight;
                        settings.spritePixelsPerUnit = 1f;
                        settings.textureType = TextureImporterType.Sprite;
                    }
                    importer.SetTextureSettings(settings);
                    importer.maxTextureSize = 2048;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.userData = cTextureImporterLabel;
                    importer.SaveAndReimport();
                }
            }
        }

        private static void ValidateSounds()
        {
            var sampleSettingSE = new AudioImporterSampleSettings()
            {
                compressionFormat = AudioCompressionFormat.ADPCM,
                loadType = AudioClipLoadType.CompressedInMemory,
                sampleRateOverride = 44100,
                sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate
            };
            var sampleSettingBGM = new AudioImporterSampleSettings()
            {
                compressionFormat = AudioCompressionFormat.Vorbis,
                loadType = AudioClipLoadType.Streaming,
                sampleRateOverride = 44100,
                sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate
            };

            var importers = AssetDatabase.FindAssets("t:AudioClip", cSearchFolder)
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(path => cRegSnd.Match(path))
                .Where(match => match.Success)
                .OrderBy(match => int.Parse(match.Groups["id"].Value))
                .Select(match => AssetImporter.GetAtPath(match.Value) as AudioImporter)
                .Where(importer => importer != null);

            foreach (var importer in importers)
            {
                if (importer.userData != cAudioImporterLabel)
                {
                    var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(importer.assetPath);
                    var sampleSetting = (clip.length < 3f) ? sampleSettingSE : sampleSettingBGM;
                    Resources.UnloadAsset(clip);
                    importer.loadInBackground = false;
                    importer.preloadAudioData = true;
                    importer.defaultSampleSettings = sampleSetting;
                    importer.SetOverrideSampleSettings("Standalone", sampleSetting);
                    importer.SetOverrideSampleSettings("iOS", sampleSetting);
                    importer.SetOverrideSampleSettings("Android", sampleSetting);
                    importer.userData = cAudioImporterLabel;
                    importer.SaveAndReimport();
                }
            }
        }

        private static void Listup()
        {
            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(cLibAtlasPath);
            var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(cLibMixerPath);
            var imageId = AssetDatabase.FindAssets("t:Texture2D", cSearchFolder)
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => cRegImg.Match(path))
                .Where(match => match.Success)
                .OrderBy(match => int.Parse(match.Groups["id"].Value))
                .Select(match => match.Groups["filename"].Value)
                .ToArray();
            var audio = AssetDatabase.FindAssets("t:AudioClip", cSearchFolder)
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => cRegSnd.Match(path))
                .Where(match => match.Success)
                .OrderBy(match => int.Parse(match.Groups["id"].Value))
                .Select(match => AssetDatabase.LoadAssetAtPath<AudioClip>(match.Value))
                .Where(asset => (asset != null))
                .ToArray();
            var video = AssetDatabase.FindAssets("t:VideoClip", cSearchFolder)
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => cRegMov.Match(path))
                .Where(match => match.Success)
                .OrderBy(match => int.Parse(match.Groups["id"].Value))
                .Select(match => AssetDatabase.LoadAssetAtPath<VideoClip>(match.Value))
                .Where(asset => (asset != null))
                .ToArray();
            var texts = AssetDatabase.FindAssets("t:TextAsset", cSearchFolder)
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => cRegTxt.Match(path))
                .Where(match => match.Success)
                .OrderBy(match => int.Parse(match.Groups["id"].Value))
                .Select(match => AssetDatabase.LoadAssetAtPath<TextAsset>(match.Value))
                .Where(asset => (asset != null))
                .ToArray();
            var fonts = AssetDatabase.FindAssets("t:Font", cSearchFolder)
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(path => cRegFnt.Match(path))
                .Where(match => match.Success)
                .OrderBy(match => int.Parse(match.Groups["id"].Value))
                .Select(match => AssetDatabase.LoadAssetAtPath<Font>(match.Value))
                .Where(asset => (asset != null))
                .ToArray();

            var res = AssetDatabase.LoadAssetAtPath<Resource>(cLibResAssetPath);
            res.SetValue(atlas, mixer, imageId, video, audio, texts, fonts);
            EditorUtility.SetDirty(res);
            AssetDatabase.SaveAssets();
        }

        #endregion
    }
}
