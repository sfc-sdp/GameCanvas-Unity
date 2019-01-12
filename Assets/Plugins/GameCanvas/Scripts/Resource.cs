/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2019 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/

namespace GameCanvas
{
    using UnityEngine;
    using UnityEngine.Video;
    using UnityEngine.U2D;
    using UnityEngine.Audio;

    public sealed class Resource : ScriptableObject
    {
        //----------------------------------------------------------
        #region フィールド変数
        //----------------------------------------------------------

        [SerializeField]
        private SpriteAtlas SpriteAtlas;
        [SerializeField]
        private string[] ImageIds;
        [SerializeField]
        private VideoClip[] VideoClips;
        [SerializeField]
        private AudioClip[] AudioClips;
        [SerializeField]
        private TextAsset[] TextAssets;
        [SerializeField]
        private Font[] Fonts;

        public AudioMixer AudioMixer;
        public Shader ShaderOpaque;
        public Shader ShaderTransparentImage;
        public Shader ShaderTransparentColor;

        [System.NonSerialized]
        private Sprite[] mSprites = null;
        [System.NonSerialized]
        private Mesh[] mSpriteMeshes = null;
        [System.NonSerialized]
        private string[] mTexts = null;

        #endregion

        //----------------------------------------------------------
        #region 構造体定義
        //----------------------------------------------------------

        public interface IRes<T>
        {
            int Id { get; }
            T Data { get; }
        }

        public struct Img : IRes<Sprite>
        {
            public Img(int id, Sprite data, Mesh mesh) { mId = id; mData = data; mMesh = mesh; }
            public int Id { get { return mId; } }
            public Sprite Data { get { return mData; } }
            public Mesh Mesh { get { return mMesh; } }
            public Texture2D Texture { get { return mData.texture; } }

            public readonly int mId;
            public readonly Sprite mData;
            public readonly Mesh mMesh;
        }

        public struct Snd : IRes<AudioClip>
        {
            public Snd(int id, AudioClip data) { mId = id; mData = data; }
            public int Id { get { return mId; } }
            public AudioClip Data { get { return mData; } }

            public readonly int mId;
            public readonly AudioClip mData;
        }

        public struct Mov : IRes<VideoClip>
        {
            public Mov(int id, VideoClip data) { mId = id; mData = data; }
            public int Id { get { return mId; } }
            public VideoClip Data { get { return mData; } }

            public readonly int mId;
            public readonly VideoClip mData;
        }

        public struct Txt : IRes<string>
        {
            public Txt(int id, string data) { mId = id; mData = data; }
            public int Id { get { return mId; } }
            public string Data { get { return mData; } }

            public readonly int mId;
            public readonly string mData;
        }

        public struct Fnt : IRes<Font>
        {
            public Fnt(int id, Font data) { mId = id; mData = data; }
            public int Id { get { return mId; } }
            public Font Data { get { return mData; } }

            public readonly int mId;
            public readonly Font mData;
        }

        #endregion

        //----------------------------------------------------------
        #region パブリック関数
        //----------------------------------------------------------

        public Mov GetMov(int id)
        {
            return (id < 0 || id >= VideoClips.Length) ? new Mov(-1, null) : new Mov(id, VideoClips[id]);
        }

        public Snd GetSnd(int id)
        {
            return (id < 0 || id >= AudioClips.Length) ? new Snd(-1, null) : new Snd(id, AudioClips[id]);
        }

        public Img GetImg(int id)
        {
            return (id < 0 || id >= mSprites.Length) ? new Img(-1, null, null) : new Img(id, mSprites[id], mSpriteMeshes[id]);
        }

        public Txt GetTxt(int id)
        {
            return (id < 0 || id >= mTexts.Length) ? new Txt(-1, null) : new Txt(id, mTexts[id]);
        }

        public Fnt GetFnt(int id)
        {
            return (id < 0 || id >= Fonts.Length) ? new Fnt(-1, null) : new Fnt(id, Fonts[id]);
        }

        internal void Initialize()
        {
            if (SpriteAtlas != null)
            {
                UnityEngine.Assertions.Assert.IsTrue(ImageIds.Length == SpriteAtlas.spriteCount);
                mSprites = new Sprite[ImageIds.Length];
                mSpriteMeshes = new Mesh[ImageIds.Length];
                for (var i = 0; i < ImageIds.Length; ++i)
                {
                    mSprites[i] = SpriteAtlas.GetSprite(ImageIds[i]);
                    UnityEngine.Assertions.Assert.IsNotNull(mSprites[i]);
                    setupMesh(out mSpriteMeshes[i], ref mSprites[i]);
                }
            }
            if (TextAssets != null)
            {
                mTexts = new string[TextAssets.Length];
                for (var i = 0; i < mTexts.Length; ++i) mTexts[i] = TextAssets[i].text;
            }

            if (VideoClips == null) VideoClips = new VideoClip[0];
            if (AudioClips == null) AudioClips = new AudioClip[0];
            if (mSprites == null) mSprites = new Sprite[0];
            if (mTexts == null) mTexts = new string[0];
            if (Fonts == null) Fonts = new Font[0];
        }

        #endregion

        //----------------------------------------------------------
        #region プライベート関数
        //----------------------------------------------------------

        private static void setupMesh(out Mesh mesh, ref Sprite sprite)
        {
            var v0 = sprite.vertices;
            var t0 = sprite.triangles;
            var uv = sprite.uv;

            var v1 = new Vector3[v0.Length];
            var c1 = new Color[v0.Length];
            for (var i = 0; i < v1.Length; ++i)
            {
                v1[i] = new Vector3(v0[i].x, v0[i].y - 1f);
                c1[i] = new Color(0f, 0f, 0f);
            }
            var t1 = new int[t0.Length];
            for (var i = 0; i < t1.Length; ++i) t1[i] = t0[i];

            mesh = new Mesh();
            mesh.vertices = v1;
            mesh.triangles = t1;
            mesh.uv = uv;
            mesh.colors = c1;
            mesh.RecalculateBounds();
        }

        #endregion

        //----------------------------------------------------------
        #region エディタ拡張
        //----------------------------------------------------------

#if UNITY_EDITOR
        internal void SetValue(SpriteAtlas atlas, AudioMixer mixer, string[] imageId, VideoClip[] video, AudioClip[] audio, TextAsset[] texts, Font[] fonts)
        {
            SpriteAtlas = atlas;
            AudioMixer = mixer;
            ImageIds = imageId ?? new string[0];
            VideoClips = video ?? new VideoClip[0];
            AudioClips = audio ?? new AudioClip[0];
            TextAssets = texts ?? new TextAsset[0];
            Fonts = fonts ?? new Font[0];

            ShaderOpaque = ShaderOpaque ?? Shader.Find("GameCanvas/Opaque");
            ShaderTransparentImage = ShaderTransparentImage ?? Shader.Find("GameCanvas/TransparentImage");
            ShaderTransparentColor = ShaderTransparentColor ?? Shader.Find("GameCanvas/TransparentColor");
        }
#endif //UNITY_EDITOR

        #endregion
    }

#if UNITY_EDITOR
    namespace Editor
    {
        using System.Linq;
        using System.IO;
        using System.Text.RegularExpressions;
        using UnityEditor;
        using UnityEditor.Build;
        using UnityEditor.Build.Reporting;

        public sealed class ResourceBuilder : IPreprocessBuildWithReport
        {
            private const string cOutputPath = "Assets/Plugins/GameCanvas/Res.asset";
            private const string cAtlasPath = "Assets/Plugins/GameCanvas/Atlas.spriteatlas";
            private const string cMixerPath = "Assets/Plugins/GameCanvas/Mixer.mixer";
            private const string cTextureImporterLabel = "GameCanvas TextureImporter 2.0";
            private static readonly string[] cInputFolders = new[] { "Assets/Res" };
            private static readonly Regex cRegImg = new Regex(@"^Assets/Res/(?<filename>img(?<id>\d+))\.(gif|GIF|png|PNG|jpg|JPG|tga|TGA|tif|TIF|tiff|TIFF|bmp|BMP|iff|IFF|pict|PICT)$");
            private static readonly Regex cRegSnd = new Regex(@"^Assets/Res/snd(?<id>\d+)\.(wav|WAV|mp3|MP3|ogg|OGG|aiff|AIFF|aif|AIF)$");
            private static readonly Regex cRegMov = new Regex(@"^Assets/Res/mov(?<id>\d+)\.(mp4|MP4|mov|MOV|mpg|MPG|mpeg|MPEG|avi|AVI|asf|ASF|dv|DV|ogv|OGV|vp8|VP8|webm|WEBM|wmv|WMV)$");
            private static readonly Regex cRegTxt = new Regex(@"^Assets/Res/txt(?<id>\d+)\.(txt|TXT|bytes|BYTES|json|JSON|xml|XML|csv|CSV|yaml|YAML)$");
            private static readonly Regex cRegFnt = new Regex(@"^Assets/Res/fnt(?<id>\d+)\.(ttf|TTF|otf|OTF)$");

            int IOrderedCallback.callbackOrder { get { return 0; } }
            void IPreprocessBuildWithReport.OnPreprocessBuild(BuildReport report) { onExitEditMode(); }

            [InitializeOnLoadMethod]
            private static void onInitialize()
            {
#if UNITY_2017_2_OR_NEWER
                EditorApplication.playModeStateChanged += (PlayModeStateChange state) =>
                {
                    if (state == PlayModeStateChange.ExitingEditMode) onExitEditMode();
                };
#else
                EditorApplication.playmodeStateChanged += () =>
                {
                    if (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode) onExitEditMode();
                };
#endif //UNITY_2017_2_OR_NEWER
                EditorApplication.delayCall += () =>
                {
                    if (!File.Exists(cOutputPath)) AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<Resource>(), cOutputPath);
                };
            }

            private static void onExitEditMode()
            {
                AssetDatabase.StartAssetEditing();
                {
                    validateImages();
                    listup();
                }
                AssetDatabase.StopAssetEditing();
            }

            private static void validateImages()
            {
                var importers = AssetDatabase.FindAssets("t:Texture2D", cInputFolders)
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

            private static void listup()
            {
                var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(cAtlasPath);
                var mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(cMixerPath);
                var imageId = AssetDatabase.FindAssets("t:Texture2D", cInputFolders)
                    .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                    .Select(path => cRegImg.Match(path))
                    .Where(match => match.Success)
                    .OrderBy(match => int.Parse(match.Groups["id"].Value))
                    .Select(match => match.Groups["filename"].Value)
                    .ToArray();
                var audio = AssetDatabase.FindAssets("t:AudioClip", cInputFolders)
                    .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                    .Select(path => cRegSnd.Match(path))
                    .Where(match => match.Success)
                    .OrderBy(match => int.Parse(match.Groups["id"].Value))
                    .Select(match => AssetDatabase.LoadAssetAtPath<AudioClip>(match.Value))
                    .Where(asset => (asset != null))
                    .ToArray();
                var video = AssetDatabase.FindAssets("t:VideoClip", cInputFolders)
                    .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                    .Select(path => cRegMov.Match(path))
                    .Where(match => match.Success)
                    .OrderBy(match => int.Parse(match.Groups["id"].Value))
                    .Select(match => AssetDatabase.LoadAssetAtPath<VideoClip>(match.Value))
                    .Where(asset => (asset != null))
                    .ToArray();
                var texts = AssetDatabase.FindAssets("t:TextAsset", cInputFolders)
                    .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                    .Select(path => cRegTxt.Match(path))
                    .Where(match => match.Success)
                    .OrderBy(match => int.Parse(match.Groups["id"].Value))
                    .Select(match => AssetDatabase.LoadAssetAtPath<TextAsset>(match.Value))
                    .Where(asset => (asset != null))
                    .ToArray();
                var fonts = AssetDatabase.FindAssets("t:Font", cInputFolders)
                    .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                    .Select(path => cRegFnt.Match(path))
                    .Where(match => match.Success)
                    .OrderBy(match => int.Parse(match.Groups["id"].Value))
                    .Select(match => AssetDatabase.LoadAssetAtPath<Font>(match.Value))
                    .Where(asset => (asset != null))
                    .ToArray();

                var res = AssetDatabase.LoadAssetAtPath<Resource>(cOutputPath);
                res.SetValue(atlas, mixer, imageId, video, audio, texts, fonts);
                EditorUtility.SetDirty(res);
                AssetDatabase.SaveAssets();
            }
        }
    }
#endif //UNITY_EDITOR
}
