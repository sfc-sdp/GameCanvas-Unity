/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;

namespace GameCanvas.Engine
{
    sealed class GcGraphicsEngine : IGraphics, IEngine
    {
        //----------------------------------------------------------
        #region 構造体
        //----------------------------------------------------------

        readonly struct TextGenKey : System.IEquatable<TextGenKey>
        {
            readonly Color Color;
            readonly Font Font;
            readonly int FontSize;
            readonly string Str;
            readonly int TextAnchor;

            public TextGenKey(in string str, in TextGenerationSettings settings)
            {
                Str = str;
                TextAnchor = (int)settings.textAnchor;
                Color = settings.color;
                Font = settings.font;
                FontSize = settings.fontSize;
            }

            public bool Equals(TextGenKey other)
                => (Str == other.Str)
                && TextAnchor.Equals(other.TextAnchor)
                && Color.Equals(other.Color)
                && (Font == other.Font)
                && FontSize.Equals(other.FontSize);

            public override bool Equals(object obj)
                => (obj is TextGenKey other) && Equals(other);

            public override int GetHashCode()
                => (Str?.GetHashCode() ?? 0)
                ^ TextAnchor
                ^ Color.GetHashCode()
                ^ (Font?.GetHashCode() ?? 0)
                ^ FontSize;
        }

        #endregion

        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        const int k_MeshCircleLif = 60;

        static readonly Color k_ColorBlack = Color.black;
        static readonly Color k_ColorWhite = Color.white;
        static readonly int k_ShaderPropColor = Shader.PropertyToID("_Color");
        static readonly int k_ShaderPropMainTex = Shader.PropertyToID("_MainTex");

        readonly GcReferenceAtlas m_Atlas;
        readonly GcContext m_Ctx;
        readonly Material m_MaterialImage;
        readonly Material m_MaterialOpaque;
        readonly Material m_MaterialTransparent;
        readonly DictWithLife<int, Mesh> m_MeshCircle;
        readonly Dictionary<string, Mesh> m_MeshImage;
        readonly ObjectPool<Mesh> m_MeshPool;
        readonly Mesh m_MeshRect;
        readonly MaterialPropertyBlock m_MPBlock;
        readonly Dictionary<string, Texture2D> m_TexImage;
        readonly Dictionary<string, GcReferenceFont> m_TextFont;
        readonly DictWithLife<TextGenKey, TextGenerator> m_TextGenerator;
        readonly DictWithLife<TextGenKey, Mesh> m_TextMesh;
        readonly List<UIVertex> m_TextMeshVerticesCache;
#pragma warning disable IDE0032
        GcMinMaxBox3D m_CanvasBox;
        int2 m_CanvasSize;
        Color m_ColorBackground;
        Color m_ColorOutside;
        CommandBuffer m_CommandBufferEndOfFrame;
        CommandBuffer m_CommandBufferOpaque;
        CommandBuffer m_CommandBufferTransparent;
        float2x3 m_CurrentMatrix;
        GcStyle m_CurrentStyle;
        Rect m_DeviceScreenRect;
        int2 m_DeviceScreenSize;
        int m_DrawCount;
        GcFont m_Font;
        RenderTexture m_FrameBuffer;
        bool m_IsInit;
        float m_PixelSizeMin;
        float4x4 m_ProjectionMtx;
        NativeList<float2x3> m_StackMatrix;
        NativeList<GcStyle> m_StackStyle;
        GcMinMaxBox3D m_ViewportBox;
        float4x4 m_ViewportMtx;
#pragma warning restore IDE0032
        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        public Color BackgroundColor
        {
            get => m_ColorBackground;
            set { value.a = 1f; m_ColorBackground = value; }
        }

        public Color BorderColor => m_ColorOutside;

        public int2 CanvasSize => m_CanvasSize;

        public int CircleResolution
        {
            get => m_CurrentStyle.CircleResolution;
            set { m_CurrentStyle.CircleResolution = math.max(3, value); }
        }

        public Color Color
        {
            get => m_CurrentStyle.Color;
            set { m_CurrentStyle.Color = value; }
        }

        public CoordianteScope CoordinateScope => new CoordianteScope(this);

        public float2x3 CurrentCoordinate
        {
            get => m_CurrentMatrix;
            set { m_CurrentMatrix = value; }
        }

        public GcStyle CurrentStyle
        {
            get => m_CurrentStyle;
            set { m_CurrentStyle = value; }
        }

        public int2 DeviceScreenSize => m_DeviceScreenSize;

        public GcFont Font
        {
            get => m_Font;
            set { if (!value.Invalid) m_Font = value; }
        }

        public int FontSize
        {
            get => m_CurrentStyle.FontSize;
            set { m_CurrentStyle.FontSize = math.max(1, value); }
        }

        public GcLineCap LineCap
        {
            get => m_CurrentStyle.LineCap;
            set { m_CurrentStyle.LineCap = value; }
        }

        public float LineWidth
        {
            get => m_CurrentStyle.LineWidth;
            set { m_CurrentStyle.LineWidth = math.max(0, value); }
        }
        public GcAnchor RectAnchor
        {
            get => m_CurrentStyle.RectAnchor;
            set { m_CurrentStyle.RectAnchor = value; }
        }

        public GcAnchor StringAnchor
        {
            get => m_CurrentStyle.StringAnchor;
            set { m_CurrentStyle.StringAnchor = value; }
        }

        public StyleScope StyleScope => new StyleScope(this);

        public float CalcStringHeight(in string str)
        {
            GetOrCreateTextGenerator(str, out var gen, out var settings);
            return gen.GetPreferredHeight(str, settings);
        }

        public float2 CalcStringSize(in string str)
        {
            GetOrCreateTextGenerator(str, out var gen, out var settings);
            return new float2(
                gen.GetPreferredWidth(str, settings),
                gen.GetPreferredHeight(str, settings)
            );
        }

        public float CalcStringWidth(in string str)
        {
            GetOrCreateTextGenerator(str, out var gen, out var settings);
            return gen.GetPreferredWidth(str, settings);
        }

        public void CanvasToScreenPoint(in float2 canvas, out float2 screen)
        {
            screen = new float2(
                canvas.x * m_DeviceScreenRect.width / m_CanvasSize.x + m_DeviceScreenRect.xMin,
                (m_CanvasSize.y - canvas.y) * m_DeviceScreenRect.height / m_CanvasSize.y + m_DeviceScreenRect.yMin
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CanvasToScreenPoint(in float2 canvas, out int2 screen)
        {
            CanvasToScreenPoint(canvas, out float2 temp);
            screen = new int2((int)math.round(temp.x), (int)math.round(temp.y));
        }

        public void ChangeBorderColor(in Color color)
        {
            if (m_ColorOutside == color) return;

            m_ColorOutside = color;
            m_ColorOutside.a = 1f;

            ReleaseFrameBuffer();
            CreateFrameBuffer();
        }

        public void ChangeCanvasSize(in int2 size)
        {
            if (m_CanvasSize.x != size.x
                || m_CanvasSize.y != size.y)
            {
                m_CanvasSize = size;
                UpdateBoxAndMtx();
            }
        }

        public void ClearCoordinate()
        {
            m_CurrentMatrix = GcAffine.Identity;
        }

        public void ClearScreen()
        {
            if (!m_IsInit) return;

            m_CommandBufferOpaque.ClearRenderTarget(true, true, m_ColorBackground);
        }

        public void ClearStyle()
        {
            m_CurrentStyle = GcStyle.Default;
        }

        public void DrawCircle()
        {
            if (!m_IsInit) return;

            var mesh = m_MeshPool.GetOrCreate();
            var lineWidth = math.max(m_CurrentStyle.LineWidth, m_PixelSizeMin);
            SetupMeshAsWiredCircle(mesh, m_CurrentStyle.CircleResolution, lineWidth, m_CurrentMatrix);
            DrawMeshDirect(mesh, m_CurrentStyle.Color);
        }

        public void DrawCircle(in GcCircle circle)
        {
            if (!m_IsInit) return;

            var s = new float2(circle.Radius, circle.Radius);
            var mtx = GcAffine.FromTS(circle.Position, s).Mul(m_CurrentMatrix);

            var mesh = m_MeshPool.GetOrCreate();
            var lineWidth = math.max(m_CurrentStyle.LineWidth, m_PixelSizeMin);
            SetupMeshAsWiredCircle(mesh, m_CurrentStyle.CircleResolution, lineWidth, mtx);
            DrawMeshDirect(mesh, m_CurrentStyle.Color);
        }

        public void DrawImage(in GcImage image)
        {
            if (!m_IsInit || image.Invalid) return;

            if (!m_MeshImage.TryGetValue(image.m_Path, out Mesh mesh))
            {
                if (!m_Atlas.TryGet(image.m_Path, out Sprite sprite)) return;

                SetupMeshAsSprite(out mesh, sprite);
                m_MeshImage.Add(image.m_Path, mesh);
                m_TexImage.Add(image.m_Path, sprite.texture);
            }

            var mtx = (m_CurrentStyle.RectAnchor == GcAnchor.UpperLeft)
                ? m_CurrentMatrix
                : m_CurrentMatrix.Mul(GcAffine.FromTranslate(image.m_Size * GetOffset(m_CurrentStyle.RectAnchor)));

            DrawMesh(mesh, m_TexImage[image.m_Path], mtx);
        }

        public void DrawImage(in GcImage image, in float2 position, float degree = 0f)
        {
            if (!m_IsInit || image.Invalid) return;

            if (!m_MeshImage.TryGetValue(image.m_Path, out Mesh mesh))
            {
                if (!m_Atlas.TryGet(image.m_Path, out Sprite sprite)) return;

                SetupMeshAsSprite(out mesh, sprite);
                m_MeshImage.Add(image.m_Path, mesh);
                m_TexImage.Add(image.m_Path, sprite.texture);
            }

            var r = math.radians(degree);
            var s = new float2(1f, 1f);
            var mtx = GcAffine.FromTRS(position, r, s).Mul(m_CurrentMatrix);
            if (m_CurrentStyle.RectAnchor != GcAnchor.UpperLeft)
            {
                mtx = mtx.Mul(GcAffine.FromTranslate(image.m_Size * GetOffset(m_CurrentStyle.RectAnchor)));
            }

            DrawMesh(mesh, m_TexImage[image.m_Path], mtx);
        }

        public void DrawImage(in GcImage image, in GcRect rect)
        {
            if (!m_IsInit || image.Invalid) return;

            if (!m_MeshImage.TryGetValue(image.m_Path, out Mesh mesh))
            {
                if (!m_Atlas.TryGet(image.m_Path, out Sprite sprite)) return;

                SetupMeshAsSprite(out mesh, sprite);
                m_MeshImage.Add(image.m_Path, mesh);
                m_TexImage.Add(image.m_Path, sprite.texture);
            }

            var s = rect.Size / image.m_Size;
            var mtx = GcAffine.FromTRS(rect.Position, rect.Radian, s).Mul(m_CurrentMatrix);
            if (m_CurrentStyle.RectAnchor != GcAnchor.UpperLeft)
            {
                mtx = mtx.Mul(GcAffine.FromTranslate(image.m_Size * GetOffset(m_CurrentStyle.RectAnchor)));
            }

            DrawMesh(mesh, m_TexImage[image.m_Path], mtx);
        }

        public void DrawLine()
        {
            if (!m_IsInit) return;

            var mesh = m_MeshPool.GetOrCreate();
            var lineWidth = math.max(m_CurrentStyle.LineWidth, m_PixelSizeMin);
            if (TrySetupMeshAsLine(mesh, m_CurrentStyle.LineCap, lineWidth, m_CurrentMatrix))
            {
                DrawMeshDirect(mesh, m_CurrentStyle.Color);
            }
        }

        public void DrawLine(in GcLine line)
        {
            if (!m_IsInit || line.IsZero()) return;

            if (!line.IsSegment())
            {
                // todo: 直線の描画
                throw new System.NotImplementedException();
            }

            var s = new float2(line.Length, line.Length);
            var mtx = GcAffine.FromTRS(line.Origin, line.Radian(), s).Mul(m_CurrentMatrix);

            var mesh = m_MeshPool.GetOrCreate();
            var lineWidth = math.max(m_CurrentStyle.LineWidth, m_PixelSizeMin);
            if (TrySetupMeshAsLine(mesh, m_CurrentStyle.LineCap, lineWidth, mtx))
            {
                DrawMeshDirect(mesh, m_CurrentStyle.Color);
            }
        }

        public void DrawRect()
        {
            if (!m_IsInit) return;

            var mesh = m_MeshPool.GetOrCreate();
            var lineWidth = math.max(m_CurrentStyle.LineWidth, m_PixelSizeMin);
            if (TrySetupMeshAsWiredRect(mesh, m_CurrentStyle.RectAnchor, lineWidth, m_CurrentMatrix))
            {
                DrawMeshDirect(mesh, m_CurrentStyle.Color);
            }
        }

        public void DrawRect(in GcRect rect)
        {
            if (!m_IsInit) return;

            var mtx = GcAffine.FromTRS(rect.Position, rect.Radian, rect.Size).Mul(m_CurrentMatrix);

            var mesh = m_MeshPool.GetOrCreate();
            var lineWidth = math.max(m_CurrentStyle.LineWidth, m_PixelSizeMin);
            if (TrySetupMeshAsWiredRect(mesh, m_CurrentStyle.RectAnchor, lineWidth, mtx))
            {
                DrawMeshDirect(mesh, m_CurrentStyle.Color);
            }
        }

        public void DrawString(in string str)
        {
            GetOrCreateTextMesh(str, out var mesh, out var texture);

            DrawMesh(mesh, texture, m_CurrentMatrix);
        }

        public void DrawString(in string str, in float2 position, float degree = 0f)
        {
            GetOrCreateTextMesh(str, out var mesh, out var texture);

            var r = math.radians(degree);
            var s = new float2(1f, 1f);
            var mtx = GcAffine.FromTRS(position, r, s).Mul(m_CurrentMatrix);

            DrawMesh(mesh, texture, mtx);
        }

        public void DrawString(in string str, in GcRect rect)
        {
            GetOrCreateTextMesh(str, out var mesh, out var texture);

            var s = rect.Size / CalcStringSize(str);
            var mtx = GcAffine.FromTRS(rect.Position, rect.Radian, s).Mul(m_CurrentMatrix);

            DrawMesh(mesh, texture, mtx);
        }

        public void DrawTexture(in Texture texture)
        {
            if (!m_IsInit || texture == null) return;

            var s = new float2(texture.width, texture.height);
            var mtx = GcAffine.FromScale(s).Mul(m_CurrentMatrix);
            if (m_CurrentStyle.RectAnchor != GcAnchor.UpperLeft)
            {
                mtx = mtx.Mul(GcAffine.FromTranslate(GetOffset(m_CurrentStyle.RectAnchor)));
            }

            DrawMesh(m_MeshRect, texture, mtx);
        }

        public void DrawTexture(in Texture texture, in float2 position, float degree = 0f)
        {
            if (!m_IsInit || texture == null) return;

            var r = math.radians(degree);
            var s = new float2(texture.width, texture.height);
            var mtx = GcAffine.FromTRS(position, r, s).Mul(m_CurrentMatrix);
            if (m_CurrentStyle.RectAnchor != GcAnchor.UpperLeft)
            {
                mtx = mtx.Mul(GcAffine.FromTranslate(GetOffset(m_CurrentStyle.RectAnchor)));
            }

            DrawMesh(m_MeshRect, texture, mtx);
        }

        public void DrawTexture(in Texture texture, in GcRect rect)
        {
            if (!m_IsInit || texture == null) return;

            var mtx = GcAffine.FromTRS(rect.Position, rect.Radian, rect.Size).Mul(m_CurrentMatrix);
            if (m_CurrentStyle.RectAnchor != GcAnchor.UpperLeft)
            {
                mtx = mtx.Mul(GcAffine.FromTranslate(GetOffset(m_CurrentStyle.RectAnchor)));
            }

            DrawMesh(m_MeshRect, texture, mtx);
        }

        public void DrawTexture(in Texture texture, in float2x3 matrix)
        {
            if (!m_IsInit || texture == null) return;

            var mtx = matrix.Mul(m_CurrentMatrix);
            if (m_CurrentStyle.RectAnchor != GcAnchor.UpperLeft)
            {
                mtx = mtx.Mul(GcAffine.FromTranslate(GetOffset(m_CurrentStyle.RectAnchor)));
            }

            DrawMesh(m_MeshRect, texture, mtx);
        }

        public void FillCircle()
        {
            if (!m_IsInit) return;

            if (!m_MeshCircle.TryGetValue(m_CurrentStyle.CircleResolution, out var mesh))
            {
                m_MeshCircle.Issue(m_CurrentStyle.CircleResolution, out mesh, k_MeshCircleLif);
                SetupMeshAsFilledCircle(mesh, m_CurrentStyle.CircleResolution);
            }

            DrawMesh(mesh, m_CurrentStyle.Color, m_CurrentMatrix);
        }

        public void FillCircle(in GcCircle circle)
        {
            if (!m_IsInit) return;

            var s = new float2(circle.Radius, circle.Radius);
            var mtx = GcAffine.FromTS(circle.Position, s).Mul(m_CurrentMatrix);

            if (!m_MeshCircle.TryGetValue(m_CurrentStyle.CircleResolution, out var mesh))
            {
                m_MeshCircle.Issue(m_CurrentStyle.CircleResolution, out mesh, k_MeshCircleLif);
                SetupMeshAsFilledCircle(mesh, m_CurrentStyle.CircleResolution);
            }

            DrawMesh(mesh, m_CurrentStyle.Color, mtx);
        }

        public void FillRect()
        {
            if (!m_IsInit) return;

            var mtx = (m_CurrentStyle.RectAnchor == GcAnchor.UpperLeft)
                ? m_CurrentMatrix
                : m_CurrentMatrix.Mul(GcAffine.FromTranslate(GetOffset(m_CurrentStyle.RectAnchor)));

            DrawMesh(m_MeshRect, m_CurrentStyle.Color, mtx);
        }

        public void FillRect(in GcRect rect)
        {
            if (!m_IsInit) return;

            var mtx = GcAffine.FromTRS(rect.Position, rect.Radian, rect.Size).Mul(m_CurrentMatrix);
            if (m_CurrentStyle.RectAnchor != GcAnchor.UpperLeft)
            {
                mtx = mtx.Mul(GcAffine.FromTranslate(GetOffset(m_CurrentStyle.RectAnchor)));
            }

            DrawMesh(m_MeshRect, m_CurrentStyle.Color, mtx);
        }

        public void PopCoordinate()
        {
            var len = m_StackMatrix.Length;
            if (len > 0)
            {
                m_CurrentMatrix = m_StackMatrix[len - 1];
                m_StackMatrix.Length -= 1;
                return;
            }
            throw new System.InvalidOperationException("[GameCanvas] MatrixStackは空です");
        }

        public void PopStyle()
        {
            var len = m_StackStyle.Length;
            if (len > 0)
            {
                m_CurrentStyle = m_StackStyle[len - 1];
                m_StackStyle.Length -= 1;
                return;
            }
            throw new System.InvalidOperationException("[GameCanvas] StyleStackは空です");
        }

        public void PushCoordinate()
        {
            m_StackMatrix.Add(m_CurrentMatrix);
        }

        public void PushStyle()
        {
            m_StackStyle.Add(m_CurrentStyle);
        }

        public void RotateCoordinate(in float degree)
        {
            m_CurrentMatrix = GcAffine.FromRotate(math.radians(degree)).Mul(m_CurrentMatrix);
        }

        public void RotateCoordinate(in float degree, in float2 origin)
        {
            m_CurrentMatrix = GcAffine.FromTranslate(origin)
                .Mul(GcAffine.FromRotate(math.radians(degree)))
                .Mul(GcAffine.FromTranslate(-origin))
                .Mul(m_CurrentMatrix);
        }

        public void ScaleCoordinate(in float2 scaling)
        {
            m_CurrentMatrix = GcAffine.FromScale(scaling).Mul(m_CurrentMatrix);
        }

        public void ScreenToCanvasPoint(in float2 screen, out float2 canvas)
        {
            canvas = new float2(
                (screen.x - m_DeviceScreenRect.xMin) * m_CanvasSize.x / m_DeviceScreenRect.width,
                m_CanvasSize.y - (screen.y - m_DeviceScreenRect.yMin) * m_CanvasSize.y / m_DeviceScreenRect.height
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ScreenToCanvasPoint(in float2 screen, out int2 canvas)
        {
            ScreenToCanvasPoint(screen, out float2 temp);
            canvas = new int2((int)math.round(temp.x), (int)math.round(temp.y));
        }

        public void TranslateCoordinate(in float2 translation)
        {
            m_CurrentMatrix = GcAffine.FromTranslate(translation).Mul(m_CurrentMatrix);
        }
        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        internal GcGraphicsEngine(in GcContext ctx)
        {
            m_Ctx = ctx;
            m_Atlas = GcReferenceAtlas.Load("GcAtlas");
            m_MPBlock = new MaterialPropertyBlock();
            m_MaterialOpaque = new Material(Shader.Find("GameCanvas/Opaque"));
            m_MaterialTransparent = new Material(Shader.Find("GameCanvas/TransparentColor"));
            m_MaterialImage = new Material(Shader.Find("GameCanvas/TransparentImage"));
            m_MeshRect = new Mesh();
            m_MeshCircle = new DictWithLife<int, Mesh>(2);
            m_MeshImage = new Dictionary<string, Mesh>(GcImage.__Length__);
            m_TexImage = new Dictionary<string, Texture2D>(GcImage.__Length__);
            m_MeshPool = new ObjectPool<Mesh>();
            m_TextFont = new Dictionary<string, GcReferenceFont>();
            m_TextMesh = new DictWithLife<TextGenKey, Mesh>();
            m_TextMesh = new DictWithLife<TextGenKey, Mesh>();
            m_TextGenerator = new DictWithLife<TextGenKey, TextGenerator>();
            m_TextMeshVerticesCache = new List<UIVertex>();

            m_CanvasSize = new int2(720, 1280);
            m_DeviceScreenSize = new int2(Screen.width, Screen.height);
            m_CurrentStyle = GcStyle.Default;
            m_CurrentMatrix = GcAffine.Identity;
            m_Font = GcFont.DefaultFont;
            m_ColorBackground = k_ColorWhite;
            m_ColorOutside = k_ColorBlack;

            SetupCamera(m_Ctx.MainCamera);
            SetupMeshAsFilledRect(m_MeshRect);
        }

        ~GcGraphicsEngine()
        {
            DisposeInternal();
        }

        void System.IDisposable.Dispose()
        {
            DisposeInternal();
            System.GC.SuppressFinalize(this);
        }

        void IEngine.OnAfterDraw() { }

        void IEngine.OnBeforeUpdate(in System.DateTimeOffset now)
        {
            if (!m_IsInit) return;

            // もし画面サイズに更新があれば、変換行列を再計算する
            if (m_DeviceScreenSize.x != Screen.width
                || m_DeviceScreenSize.y != Screen.height)
            {
                m_DeviceScreenSize = new int2(Screen.width, Screen.height);
                UpdateBoxAndMtx();
            }

            m_CommandBufferOpaque.Clear();
#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_IOS
            m_CommandBufferOpaque.Blit(m_FrameBuffer, BuiltinRenderTextureType.CameraTarget, new float2(1, -1f), new float2(0, 1f));
#else
            m_CommandBufferOpaque.Blit(m_FrameBuffer, BuiltinRenderTextureType.CameraTarget);
#endif // UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_IOS
            m_CommandBufferOpaque.SetViewport(m_DeviceScreenRect);
            m_CommandBufferOpaque.SetViewProjectionMatrices(m_ViewportMtx, m_ProjectionMtx);
            m_CommandBufferTransparent.Clear();
            m_CommandBufferTransparent.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            m_CommandBufferTransparent.SetViewport(m_DeviceScreenRect);
            m_CommandBufferTransparent.SetViewProjectionMatrices(m_ViewportMtx, m_ProjectionMtx);

            m_MeshPool.ReleaseAll();
            m_TextMesh.DecrementLife();
            m_TextGenerator.DecrementLife();
            m_DrawCount = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static float2 GetOffset(in GcAnchor anchor)
        {
            switch (anchor)
            {
                case GcAnchor.UpperCenter: return new float2(-0.5f, 0f);
                case GcAnchor.UpperRight: return new float2(-1f, 0f);
                case GcAnchor.MiddleRight: return new float2(-1f, -0.5f);
                case GcAnchor.LowerRight: return new float2(-1f, -1f);
                case GcAnchor.LowerCenter: return new float2(-0.5f, -1f);
                case GcAnchor.LowerLeft: return new float2(0f, -1f);
                case GcAnchor.MiddleLeft: return new float2(0f, -0.5f);
                case GcAnchor.MiddleCenter: return new float2(-0.5f, -0.5f);
                default: return float2.zero;
            }
        }

        internal void Init()
        {
            if (m_IsInit) return;

            m_CommandBufferOpaque = new CommandBuffer();
            m_CommandBufferTransparent = new CommandBuffer();
            m_CommandBufferEndOfFrame = new CommandBuffer();
            m_CommandBufferOpaque.name = "GameCanvas Opaque";
            m_CommandBufferTransparent.name = "GameCanvas Transparent";
            m_CommandBufferEndOfFrame.name = "GameCanvas EndOfFrame";
            m_StackStyle = new NativeList<GcStyle>(Allocator.Persistent);
            m_StackMatrix = new NativeList<float2x3>(Allocator.Persistent);

            UpdateBoxAndMtx();
            CreateFrameBuffer();
            m_Ctx.MainCamera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, m_CommandBufferOpaque);
            m_Ctx.MainCamera.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, m_CommandBufferTransparent);
            m_Ctx.MainCamera.AddCommandBuffer(CameraEvent.AfterEverything, m_CommandBufferEndOfFrame);
            UnityEngine.Font.textureRebuilt += OnFontTextureRebuild;

            m_IsInit = true;
            System.GC.ReRegisterForFinalize(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetupCamera(in Camera camera)
        {
            camera.clearFlags = CameraClearFlags.Depth;
            camera.orthographic = true;
            camera.orthographicSize = 5;
            camera.farClipPlane = 100;
            camera.nearClipPlane = 0;
            camera.allowMSAA = false;
        }

        private static void SetupMeshAsFilledCircle(in Mesh mesh, in int resolution)
        {
            var vertices = new NativeArray<float3>(resolution, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            var indices = new NativeArray<ushort>(resolution * 3 - 6, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            var uvs = new NativeArray<float2>(resolution, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            {
                var unit = math.PI * 2 / resolution;

                for (var i = 0; i < resolution; i++)
                {
                    var rad = i * unit;
                    var x = math.sin(rad);
                    var y = math.cos(rad);
                    vertices[i] = new float3(x, y, 0f);
                    uvs[i] = new float2(x * 0.5f + 0.5f, y * 0.5f + 0.5f);

                    if (i < resolution - 2)
                    {
                        var j = i * 3;
                        indices[j] = 0;
                        indices[j + 1] = (ushort)(i + 1);
                        indices[j + 2] = (ushort)(i + 2);
                    }
                }

                mesh.Clear();
                mesh.SetVertices(vertices);
                mesh.SetIndices(indices, MeshTopology.Triangles, 0);
                mesh.SetUVs(0, uvs);
                mesh.RecalculateBounds();
            }
            uvs.Dispose();
            indices.Dispose();
            vertices.Dispose();
        }

        private static void SetupMeshAsFilledRect(in Mesh mesh)
        {
            var vertices = new NativeArray<float3>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            var indices = new NativeArray<ushort>(6, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            var uvs = new NativeArray<float2>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            var colors = new NativeArray<Color32>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            {
                vertices[0] = new float3(0f, 0f, 0f); // 左上
                vertices[1] = new float3(1f, 0f, 0f); // 右上
                vertices[2] = new float3(1f, 1f, 0f); // 右下
                vertices[3] = new float3(0f, 1f, 0f); // 左下

                indices[0] = 0;
                indices[1] = 1;
                indices[2] = 3;
                indices[3] = 3;
                indices[4] = 1;
                indices[5] = 2;

                uvs[0] = new float2(0f, 1f); // 左上
                uvs[1] = new float2(1f, 1f); // 右上
                uvs[2] = new float2(1f, 0f); // 右下
                uvs[3] = new float2(0f, 0f); // 左下

                colors[0] = new Color32(0, 0, 0, 1);
                colors[1] = new Color32(0, 0, 0, 1);
                colors[2] = new Color32(0, 0, 0, 1);
                colors[3] = new Color32(0, 0, 0, 1);

                mesh.name = "FilledRect";
                mesh.SetVertices(vertices);
                mesh.SetIndices(indices, MeshTopology.Triangles, 0);
                mesh.SetUVs(0, uvs);
                mesh.SetColors(colors);
                mesh.RecalculateBounds();
            }
            colors.Dispose();
            uvs.Dispose();
            indices.Dispose();
            vertices.Dispose();
        }

        private static void SetupMeshAsSprite(out Mesh mesh, in Sprite sprite)
        {
#if true
            // VertexAttribute.TexCoord0 が壊れてるのを直すまじない（頼むから直してくれ Unity
            var dummy = sprite.uv;
#endif // true
            var v0 = sprite.GetVertexAttribute<Vector3>(VertexAttribute.Position).SliceConvert<float3>();
            var u0 = sprite.GetVertexAttribute<Vector2>(VertexAttribute.TexCoord0).SliceConvert<float2>();
            var indices = sprite.GetIndices();

            var vertices = new NativeArray<float3>(v0.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            var uvs = new NativeArray<float2>(u0.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            var colors = new NativeArray<Color32>(v0.Length, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            {
                v0.CopyTo(vertices);
                u0.CopyTo(uvs);

                for (var i = 0; i < vertices.Length; i++)
                {
                    vertices[i] *= new float3(1f, -1f, 1f);
                    colors[i] = new Color32(0, 0, 0, 1);
                }

                mesh = new Mesh();
                mesh.SetVertices(vertices);
                mesh.SetIndices(indices, MeshTopology.Triangles, 0);
                mesh.SetUVs(0, uvs);
                mesh.SetColors(colors);
                mesh.RecalculateBounds();
            }
            uvs.Dispose();
            vertices.Dispose();
        }

        private static void SetupMeshAsText(in Mesh mesh, in TextGenerator gen, in List<UIVertex> cache)
        {
            gen.GetVertices(cache);

            var len = cache.Count;
            var vertices = new NativeArray<Vector3>(len, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            var indices = new NativeArray<ushort>(len / 2 * 3, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            var uvs = new NativeArray<Vector2>(len, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            var colors = new NativeArray<Color32>(len, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            {
                for (int i = 0; i != len; i++)
                {
                    var info = cache[i];
                    info.position.y *= -1f;
                    vertices[i] = info.position;
                    colors[i] = info.color;
                    uvs[i] = info.uv0;
                }
                for (int i = 0, j = 0; i < len; i += 4)
                {
                    indices[j++] = (ushort)i;
                    indices[j++] = (ushort)(i + 1);
                    indices[j++] = (ushort)(i + 2);
                    indices[j++] = (ushort)(i + 2);
                    indices[j++] = (ushort)(i + 3);
                    indices[j++] = (ushort)i;
                }

                mesh.Clear();
                mesh.SetVertices(vertices);
                mesh.SetIndices(indices, MeshTopology.Triangles, 0);
                mesh.SetUVs(0, uvs);
                mesh.SetColors(colors);
                mesh.RecalculateBounds();
            }
            colors.Dispose();
            uvs.Dispose();
            indices.Dispose();
            vertices.Dispose();
        }

        private static void SetupMeshAsWiredCircle(in Mesh mesh, in int resolution, in float lineWidth, in float2x3 matrix)
        {
            var vertices = new NativeArray<float3>(resolution * 2, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            var indices = new NativeArray<ushort>(resolution * 6, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            {
                var unit = math.PI * 2 / resolution;
                var center = matrix.Mul(float2.zero);

                for (var i = 0; i < resolution; i++)
                {
                    var j = i * 2;
                    var rad = i * unit;
                    var pos = matrix.Mul(new float2(math.sin(rad), math.cos(rad)));
                    var vec = math.normalize(pos - center) * (lineWidth * 0.5f);
                    vertices[j] = new float3(pos - vec, 0f);
                    vertices[j + 1] = new float3(pos + vec, 0f);

                    var k = i * 6;
                    if (i == resolution - 1)
                    {
                        indices[k] = (ushort)j;
                        indices[k + 1] = (ushort)(j + 1);
                        indices[k + 2] = 1;
                        indices[k + 3] = (ushort)j;
                        indices[k + 4] = 1;
                        indices[k + 5] = 0;
                    }
                    else
                    {
                        indices[k] = (ushort)j;
                        indices[k + 1] = (ushort)(j + 1);
                        indices[k + 2] = (ushort)(j + 3);
                        indices[k + 3] = (ushort)(j + 0);
                        indices[k + 4] = (ushort)(j + 3);
                        indices[k + 5] = (ushort)(j + 2);
                    }
                }

                mesh.Clear();
                mesh.SetVertices(vertices);
                mesh.SetIndices(indices, MeshTopology.Triangles, 0);
                mesh.RecalculateBounds();
            }
            indices.Dispose();
            vertices.Dispose();
        }

        private static void SetupTextGeneratorSettings(in GcStyle style, in Font font, out TextGenerationSettings settings)
        {
            settings = new TextGenerationSettings
            {
                textAnchor = (TextAnchor)style.StringAnchor,
                color = style.Color,
                font = font,
                fontSize = style.FontSize,
                fontStyle = FontStyle.Normal,
                verticalOverflow = VerticalWrapMode.Overflow,
                horizontalOverflow = HorizontalWrapMode.Overflow,
                alignByGeometry = true,
                richText = false,
                lineSpacing = 1f,
                scaleFactor = 1f,
                resizeTextForBestFit = false
            };
        }

        private static bool TrySetupMeshAsLine(in Mesh mesh, in GcLineCap lineCap, in float lineWidth, in float2x3 matrix)
        {
            var p0 = matrix.Mul(float2.zero);
            var p1 = matrix.Mul(new float2(1f, 0f));

            if (GcMath.AlmostZero(math.lengthsq(p1 - p0))) return false;

            var vertices = new NativeArray<float3>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            var indices = new NativeArray<ushort>(6, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            {
                var dir = math.normalize(p1 - p0);
                var nrm = new float2(-dir.y, dir.x) * (lineWidth * 0.5f);
                vertices[0] = new float3(p0 + nrm, 0f);
                vertices[1] = new float3(p1 + nrm, 0f);
                vertices[2] = new float3(p1 - nrm, 0f);
                vertices[3] = new float3(p0 - nrm, 0f);
                if (lineCap == GcLineCap.Square)
                {
                    var offset = new float3(dir, 0f) * (lineWidth * 0.5f);
                    vertices[0] -= offset;
                    vertices[1] += offset;
                    vertices[2] += offset;
                    vertices[3] -= offset;
                }

                indices[0] = 0;
                indices[1] = 1;
                indices[2] = 3;
                indices[3] = 3;
                indices[4] = 1;
                indices[5] = 2;

                mesh.Clear();
                mesh.SetVertices(vertices);
                mesh.SetIndices(indices, MeshTopology.Triangles, 0);
                mesh.RecalculateBounds();
            }
            indices.Dispose();
            vertices.Dispose();
            return true;
        }

        private static bool TrySetupMeshAsWiredRect(in Mesh mesh, in GcAnchor anchor, in float lineWidth, in float2x3 matrix)
        {
            var valid = false;
            var vertices = new NativeArray<float3>(8, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            var indices = new NativeArray<ushort>(24, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            {
                var offset = GetOffset(anchor);

                var pos = new NativeArray<float2>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                var dir = new NativeArray<float2>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                var nrm = new NativeArray<float2>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                var line = new NativeArray<GcLine>(8, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                {
                    pos[0] = matrix.Mul(offset);
                    pos[1] = matrix.Mul(offset + new float2(1f, 0f));
                    pos[2] = matrix.Mul(offset + new float2(1f, 1f));
                    pos[3] = matrix.Mul(offset + new float2(0f, 1f));
                    dir[0] = math.normalize(pos[1] - pos[0]);
                    dir[1] = math.normalize(pos[2] - pos[1]);
                    dir[2] = math.normalize(pos[3] - pos[2]);
                    dir[3] = math.normalize(pos[0] - pos[3]);
                    for (var i = 0; i < 4; i++)
                    {
                        nrm[i] = new float2(-dir[i].y, dir[i].x) * (lineWidth * 0.5f);
                        line[i] = new GcLine(pos[i] - nrm[i], dir[i]);
                        line[i + 4] = new GcLine(pos[i] + nrm[i], dir[i]);
                    }
                    for (var i = 0; i < 8; i += 4)
                    {
                        if (line[i].Intersects(line[i + 1], out var crs1)
                            && line[i + 1].Intersects(line[i + 2], out var crs2)
                            && line[i + 2].Intersects(line[i + 3], out var crs3)
                            && line[i + 3].Intersects(line[i], out var crs0))
                        {
                            vertices[i] = new float3(crs0, 0f);
                            vertices[i + 1] = new float3(crs1, 0f);
                            vertices[i + 2] = new float3(crs2, 0f);
                            vertices[i + 3] = new float3(crs3, 0f);
                            valid = true;
                        }
                    }
                }
                line.Dispose();
                nrm.Dispose();
                dir.Dispose();
                pos.Dispose();

                if (valid)
                {
                    for (var i = 0; i < 3; i++)
                    {
                        var j = i * 6;
                        indices[j] = (ushort)i;
                        indices[j + 1] = (ushort)(i + 4);
                        indices[j + 2] = (ushort)(i + 5);
                        indices[j + 3] = (ushort)i;
                        indices[j + 4] = (ushort)(i + 5);
                        indices[j + 5] = (ushort)(i + 1);
                    }
                    indices[18] = 3;
                    indices[19] = 7;
                    indices[20] = 4;
                    indices[21] = 3;
                    indices[22] = 4;
                    indices[23] = 0;

                    mesh.Clear();
                    mesh.SetVertices(vertices);
                    mesh.SetIndices(indices, MeshTopology.Triangles, 0);
                    mesh.RecalculateBounds();
                }
            }
            indices.Dispose();
            vertices.Dispose();
            return valid;
        }

        private void CreateFrameBuffer()
        {
            if (m_FrameBuffer != null
                && m_FrameBuffer.width == m_DeviceScreenSize.x
                && m_FrameBuffer.height == m_DeviceScreenSize.y)
            {
                return;
            }

            ReleaseFrameBuffer();
            m_FrameBuffer = new RenderTexture(m_DeviceScreenSize.x, m_DeviceScreenSize.y, 0, UnityEngine.Experimental.Rendering.DefaultFormat.LDR);
            m_FrameBuffer.name = "GameCanvas FrameBuffer";
            m_FrameBuffer.Create();
            Graphics.SetRenderTarget(m_FrameBuffer);
            {
                m_MPBlock.Clear();
                m_MPBlock.SetColor(k_ShaderPropColor, m_ColorOutside);
                var matrix = GcAffine.FromTS((float2)m_DeviceScreenSize * -0.5f, m_DeviceScreenSize).ToFloat4x4();
                Graphics.DrawMesh(m_MeshRect, matrix, m_MaterialOpaque, 0, m_Ctx.MainCamera, 0, m_MPBlock);
            }
            Graphics.SetRenderTarget(null);

            m_CommandBufferEndOfFrame.Clear();
            m_CommandBufferEndOfFrame.Blit(BuiltinRenderTextureType.CameraTarget, m_FrameBuffer);
        }

        private void DisposeInternal()
        {
            if (!m_IsInit) return;

            UnityEngine.Font.textureRebuilt -= OnFontTextureRebuild;
            m_Ctx.MainCamera.RemoveCommandBuffer(CameraEvent.AfterEverything, m_CommandBufferEndOfFrame);
            m_Ctx.MainCamera.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, m_CommandBufferTransparent);
            m_Ctx.MainCamera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, m_CommandBufferOpaque);
            ReleaseFrameBuffer();

            m_CommandBufferEndOfFrame?.Dispose();
            m_CommandBufferTransparent?.Dispose();
            m_CommandBufferOpaque?.Dispose();
            if (m_StackStyle.IsCreated) m_StackStyle.Dispose();
            if (m_StackMatrix.IsCreated) m_StackMatrix.Dispose();

            foreach (var font in m_TextFont.Values) font.Unload();
            m_TextFont.Clear();
            m_Atlas.Unload();

            m_IsInit = false;
        }

        private void DrawMesh(in Mesh mesh, in Color color, in float2x3 mtx)
        {
            m_MPBlock.Clear();
            m_MPBlock.SetColor(k_ShaderPropColor, color);

            var hasAlpha = color.a != 1f;
            var buffer = GetCommandBuffer(hasAlpha);
            var material = GetMaterial(hasAlpha);
            var matrix = GcAffine.FromTranslate(new float2(0f, m_CanvasSize.y))
                .Mul(GcAffine.FromScale(new float2(1f, -1f)))
                .Mul(mtx)
                .ToFloat4x4();
            matrix.c3.z = m_DrawCount++ * 0.001f;

            buffer.DrawMesh(mesh, matrix, material, 0, -1, m_MPBlock);
        }

        private void DrawMesh(in Mesh mesh, in Texture tex, in float2x3 mtx)
        {
            m_MPBlock.Clear();
            m_MPBlock.SetTexture(k_ShaderPropMainTex, tex);

            var matrix = GcAffine.FromTranslate(new float2(0f, m_CanvasSize.y))
                .Mul(GcAffine.FromScale(new float2(1f, -1f)))
                .Mul(mtx)
                .ToFloat4x4();
            matrix.c3.z = m_DrawCount++ * 0.001f;

            m_CommandBufferTransparent.DrawMesh(mesh, matrix, m_MaterialImage, 0, -1, m_MPBlock);
        }

        private void DrawMeshDirect(in Mesh mesh, in Color color)
        {
            m_MPBlock.Clear();
            m_MPBlock.SetColor(k_ShaderPropColor, color);

            var hasAlpha = color.a != 1f;
            var buffer = GetCommandBuffer(hasAlpha);
            var material = GetMaterial(hasAlpha);
            var matrix = GcAffine.FromTranslate(new float2(0f, m_CanvasSize.y))
                .Mul(GcAffine.FromScale(new float2(1f, -1f)))
                .ToFloat4x4();
            matrix.c3.z = m_DrawCount++ * 0.001f;

            buffer.DrawMesh(mesh, matrix, material, 0, -1, m_MPBlock);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private CommandBuffer GetCommandBuffer(in bool hasAlpha)
#if UNITY_EDITOR
            => hasAlpha ? m_CommandBufferTransparent : m_CommandBufferOpaque;
#else
            => m_CommandBufferTransparent;
#endif // UNITY_EDITOR

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Material GetMaterial(in bool hasAlpha)
#if UNITY_EDITOR
            => hasAlpha ? m_MaterialTransparent : m_MaterialOpaque;
#else
            => m_MaterialTransparent;
#endif // UNITY_EDITOR

        private void GetOrCreateTextGenerator(in string str, out TextGenerator gen, out TextGenerationSettings settings)
        {
            GetOrLoadFont(m_Font, out var font);
            SetupTextGeneratorSettings(m_CurrentStyle, font, out settings);
            var key = new TextGenKey(str, settings);

            if (!m_TextGenerator.TryGetValue(key, out gen))
            {
                m_TextGenerator.Issue(key, out gen);
            }
        }

        private void GetOrCreateTextMesh(in string str, out Mesh mesh, out Texture texture)
        {
            GetOrLoadFont(m_Font, out var font);
            texture = font.material.mainTexture;

            SetupTextGeneratorSettings(m_CurrentStyle, font, out var settings);
            var key = new TextGenKey(str, settings);

            if (!m_TextGenerator.TryGetValue(key, out var gen))
            {
                m_TextGenerator.Issue(key, out gen);
            }

            if (!m_TextMesh.TryGetValue(key, out mesh))
            {
                m_TextMesh.Issue(key, out mesh);

                gen.Populate(str, settings);
                SetupMeshAsText(mesh, gen, m_TextMeshVerticesCache);
                m_TextMeshVerticesCache.Clear();
            }
        }

        private void GetOrLoadFont(in GcFont fontName, out Font font)
        {
            if (!m_TextFont.TryGetValue(fontName.m_Path, out var value))
            {
                value = GcReferenceFont.Load(fontName.m_Path);
            }
            font = value.Get();
        }

        private void OnFontTextureRebuild(Font font)
        {
            m_TextMesh.ReleaseAll();
            m_TextGenerator.ReleaseAll();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReleaseFrameBuffer()
        {
            m_FrameBuffer?.Release();
            m_FrameBuffer = null;
        }

        private void UpdateBoxAndMtx()
        {
            m_ViewportBox = new GcMinMaxBox3D(
                m_Ctx.MainCamera.ViewportToWorldPoint(new Vector3(0, 0, m_Ctx.MainCamera.nearClipPlane)),
                m_Ctx.MainCamera.ViewportToWorldPoint(new Vector3(1, 1, m_Ctx.MainCamera.farClipPlane))
            );

            var canvasAspect = (float)m_CanvasSize.x / m_CanvasSize.y;
            var viewportAspect = m_ViewportBox.Width / m_ViewportBox.Height;
            if (canvasAspect < viewportAspect)
            {
                // 左右に帯
                m_CanvasBox = new GcMinMaxBox3D(
                    m_ViewportBox.MinY * canvasAspect, m_ViewportBox.MinY, m_ViewportBox.MinZ,
                    m_ViewportBox.MaxY * canvasAspect, m_ViewportBox.MaxY, m_ViewportBox.MaxZ
                );
            }
            else if (canvasAspect > viewportAspect)
            {
                // 上下に帯
                m_CanvasBox = new GcMinMaxBox3D(
                    m_ViewportBox.MinX, m_ViewportBox.MinX / canvasAspect, m_ViewportBox.MinZ,
                    m_ViewportBox.MaxX, m_ViewportBox.MaxX / canvasAspect, m_ViewportBox.MaxZ
                );
            }
            else
            {
                // 帯なし
                m_CanvasBox = m_ViewportBox;
            }

            var screenMin = m_Ctx.MainCamera.WorldToScreenPoint(new Vector3(m_CanvasBox.MinX, m_CanvasBox.MinY));
            var screenMax = m_Ctx.MainCamera.WorldToScreenPoint(new Vector3(m_CanvasBox.MaxX, m_CanvasBox.MaxY));
            m_DeviceScreenRect = new Rect(screenMin, screenMax - screenMin);

            m_ViewportMtx = Matrix4x4.TRS(new Vector3(-1f, -1f, 0f), Quaternion.identity, new Vector3(2f / m_CanvasSize.x, 2f / m_CanvasSize.y, 1f));
            m_ProjectionMtx = Matrix4x4.Ortho(-1f, 1f, -1f, 1f, -100f, 0f);
            m_PixelSizeMin = Mathf.Max(1f, m_CanvasSize.x / m_DeviceScreenRect.width);

            CreateFrameBuffer();
        }
        #endregion
    }
}
