/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.U2D;
using UnityEngine.Video;

namespace GameCanvas
{
    public sealed class Resource : ScriptableObject
    {
        //----------------------------------------------------------
        #region フィールド変数
        //----------------------------------------------------------

#pragma warning disable 0649
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
#pragma warning restore 0649

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
                    SetupMesh(out mSpriteMeshes[i], ref mSprites[i]);
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

        private static void SetupMesh(out Mesh mesh, ref Sprite sprite)
        {
            var v0 = sprite.vertices;
            var t0 = sprite.triangles;
            var uv = sprite.uv;

            var v1 = new Vector3[v0.Length];
            var c1 = new Color[v0.Length];
            for (var i = 0; i < v1.Length; ++i)
            {
                v1[i] = new Vector3(v0[i].x, v0[i].y);
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
        public void SetValue(SpriteAtlas atlas, AudioMixer mixer, string[] imageId, VideoClip[] video, AudioClip[] audio, TextAsset[] texts, Font[] fonts)
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
}
