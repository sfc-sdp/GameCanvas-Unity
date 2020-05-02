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
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

namespace GameCanvas.Engine
{
    public sealed class Graphic : System.IDisposable
    {
        //----------------------------------------------------------
        #region フィールド変数
        //----------------------------------------------------------

        private const int cCircleResolution = 24;

        private static readonly Vector2 cVectorRight = Vector2.right;
        private static readonly Color cColorBlack = Color.black;
        private static readonly Color cColorWhite = Color.white;

        private readonly Resource cRes;
        private readonly Camera cCamera;
        private readonly CommandBuffer cBufferOpaque;
        private readonly CommandBuffer cBufferTransparent;
        private readonly CommandBuffer cBufferEndFrame;
        private readonly Material cMaterialOpaque;
        private readonly Material cMaterialTransparentImage;
        private readonly Material cMaterialTransparentColor;
        private readonly MaterialPropertyBlock cBlock;
        private readonly int cShaderPropMainTex;
        private readonly int cShaderPropColor;
        private readonly int cShaderPropClipRect;
        private readonly Mesh cMeshRect;
        private readonly Mesh cMeshCircle;
        private readonly Mesh cMeshCircleBorder;
        private readonly List<Vector3> cVertsCircleBorder;
        private readonly List<Vector3> cVertsText;
        private readonly List<Vector2> cUvsText;
        private readonly List<int> cTrisText;
        private readonly List<Color> cColorsText;
        private readonly List<TextGenerator> cTextGeneratorPool;
        private readonly System.Action<Font> cTextRebuildCallback;
        private readonly List<Mesh> cMeshPoolText;
        private readonly List<Mesh> cMeshPoolDrawRect;
        private readonly List<List<Vector3>> cVertsPool;

        private bool mIsEnable;
        private bool mIsDispose;
        private Vector2Int mScreenSize;
        private Vector2 mCanvasSize;
        private Box mBoxViewport;
        private Box mBoxCanvas;
        private Rect mRectScreen;
        private Matrix4x4 mMatrixView;
        private Matrix4x4 mMatrixProj;
        private float mPixelSizeMin;
        private Color mColor;
        private Color mColorMultiply;
        private Font mFont;
        private FontStyle mFontStyle;
        private int mFontSize;

        private int mCountDraw;
        private int mCountText;
        private int mCountDrawRect;
        private RenderTexture mPrevFrame;

        #endregion

        //----------------------------------------------------------
        #region パブリック関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal Graphic(Resource res, Camera camera)
        {
            Assert.IsNotNull(res);
            Assert.IsNotNull(camera);

            cRes = res;
            cCamera = camera;
            cCamera.clearFlags = CameraClearFlags.SolidColor;
            cCamera.backgroundColor = cColorBlack;
            cCamera.orthographic = true;
            cCamera.orthographicSize = 5;
            cCamera.farClipPlane = 100;
            cCamera.nearClipPlane = 0;
            cCamera.allowMSAA = false;
            cBufferOpaque = new CommandBuffer();
            cBufferOpaque.name = "GameCanvas Opaque";
            cBufferTransparent = new CommandBuffer();
            cBufferTransparent.name = "GameCanvas Transparent ";
            cBufferEndFrame = new CommandBuffer();
            cBufferEndFrame.name = "GameCanvas EndFrame";
            cMaterialOpaque = new Material(res.ShaderOpaque);
            cMaterialTransparentImage = new Material(res.ShaderTransparentImage);
            cMaterialTransparentColor = new Material(res.ShaderTransparentColor);
            cBlock = new MaterialPropertyBlock();
            cShaderPropMainTex = Shader.PropertyToID("_MainTex");
            cShaderPropColor = Shader.PropertyToID("_Color");
            cShaderPropClipRect = Shader.PropertyToID("_ClipRect");
            cTextGeneratorPool = new List<TextGenerator>(20);
            cMeshPoolText = new List<Mesh>(20);
            cMeshPoolDrawRect = new List<Mesh>(20);
            cVertsPool = new List<List<Vector3>>(20);
            cTextRebuildCallback = new System.Action<Font>(OnTextTextureRebuild);
            cVertsText = new List<Vector3>();
            cUvsText = new List<Vector2>();
            cTrisText = new List<int>();
            cColorsText = new List<Color>();
            InitMeshAsRect(out cMeshRect);
            InitMeshAsCircle(out cMeshCircle);
            InitMeshAsCircleBorder(out cMeshCircleBorder, out cVertsCircleBorder);

            mFontStyle = FontStyle.Normal;
            mFontSize = 25;
            mFont = cRes.GetFnt(0).Data ?? Font.CreateDynamicFontFromOSFont(new[] { ".Hiragino Kaku Gothic Interface", "MotoyaLMaru", "MotoyaLCedar", "RobotoSansFallback", "Droid Sans Fallback" }, mFontSize);

            mColor = cColorBlack;
            mColorMultiply = cColorWhite;
            mScreenSize = new Vector2Int(Screen.width, Screen.height);
            mCanvasSize = new Vector2(720, 1280);
            Application.targetFrameRate = 60;
        }

        public void Dispose()
        {
            if (!mIsDispose)
            {
                mIsDispose = true;
                OnDisable();
                cBufferOpaque.Dispose();
                cBufferTransparent.Dispose();
            }
        }

        public void OnEnable()
        {
            if (!mIsEnable && !mIsDispose)
            {
                mIsEnable = true;
                cCamera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, cBufferOpaque);
                cCamera.AddCommandBuffer(CameraEvent.BeforeForwardAlpha, cBufferTransparent);
                cCamera.AddCommandBuffer(CameraEvent.AfterEverything, cBufferEndFrame);
                Font.textureRebuilt += cTextRebuildCallback;
            }
        }

        public void OnDisable()
        {
            if (mIsEnable && !mIsDispose)
            {
                mIsEnable = false;
                Font.textureRebuilt -= cTextRebuildCallback;
                cCamera.RemoveCommandBuffer(CameraEvent.AfterEverything, cBufferEndFrame);
                cCamera.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, cBufferTransparent);
                cCamera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, cBufferOpaque);
                mPrevFrame?.Release();
                mPrevFrame = null;
            }
        }

        public void OnBeforeUpdate()
        {
            if (mIsDispose) return;

            if (mScreenSize.x != Screen.width || mScreenSize.y != Screen.height)
            {
                mScreenSize = new Vector2Int(Screen.width, Screen.height);
                SetResolution((int)mCanvasSize.x, (int)mCanvasSize.y);
            }

            cBufferOpaque.Clear();
#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_IOS
            // DirectX or Metal
            cBufferOpaque.Blit(mPrevFrame, BuiltinRenderTextureType.CameraTarget, new Vector2(1, -1f), new Vector2(0, 1f));
#else
            // OpenGL
            cBufferOpaque.Blit(mPrevFrame, BuiltinRenderTextureType.CameraTarget);
#endif //UNITY_EDITOR_WIN
            cBufferOpaque.SetViewport(mRectScreen);
            cBufferOpaque.SetViewProjectionMatrices(mMatrixView, mMatrixProj);

            cBufferTransparent.Clear();
            cBufferTransparent.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            cBufferTransparent.SetViewport(mRectScreen);
            cBufferTransparent.SetViewProjectionMatrices(mMatrixView, mMatrixProj);

            cBufferEndFrame.Clear();
            cBufferEndFrame.Blit(BuiltinRenderTextureType.CameraTarget, mPrevFrame);

            mCountDraw = 0;
            mCountText = 0;
            mCountDrawRect = 0;
        }

        public void SetResolution(int width, int height)
        {
            mCanvasSize = new Vector2(width, height);

            mBoxViewport = new Box(
                cCamera.ViewportToWorldPoint(new Vector3(0, 0, cCamera.nearClipPlane)),
                cCamera.ViewportToWorldPoint(new Vector3(1, 1, cCamera.farClipPlane))
            );

            var canvasAspect = mCanvasSize.x / mCanvasSize.y;
            var viewportAspect = mBoxViewport.Width / mBoxViewport.Height;
            if (canvasAspect < viewportAspect)
            {
                // 左右に黒帯
                mBoxCanvas = new Box(
                    mBoxViewport.MinY * canvasAspect, mBoxViewport.MinY, mBoxViewport.MinZ,
                    mBoxViewport.MaxY * canvasAspect, mBoxViewport.MaxY, mBoxViewport.MaxZ
                );
            }
            else if (canvasAspect > viewportAspect)
            {
                // 上下に黒帯
                mBoxCanvas = new Box(
                    mBoxViewport.MinX, mBoxViewport.MinX / canvasAspect, mBoxViewport.MinZ,
                    mBoxViewport.MaxX, mBoxViewport.MaxX / canvasAspect, mBoxViewport.MaxZ
                );
            }
            else
            {
                // 黒帯なし
                mBoxCanvas = mBoxViewport;
            }

            var screenMin = cCamera.WorldToScreenPoint(new Vector3(mBoxCanvas.MinX, mBoxCanvas.MinY));
            var screenMax = cCamera.WorldToScreenPoint(new Vector3(mBoxCanvas.MaxX, mBoxCanvas.MaxY));
            mRectScreen = new Rect(screenMin, screenMax - screenMin);

            mMatrixView = Matrix4x4.TRS(new Vector3(-1f, -1f, 0f), Quaternion.identity, new Vector3(2f / mCanvasSize.x, 2f / mCanvasSize.y, 1f));
            mMatrixProj = Matrix4x4.Ortho(-1f, 1f, -1f, 1f, -100f, 0f);
            mPixelSizeMin = Mathf.Max(1f, mCanvasSize.x / mRectScreen.width);

            if (mPrevFrame == null || mPrevFrame.width != mScreenSize.x || mPrevFrame.height != mScreenSize.y)
            {
                mPrevFrame?.Release();
                mPrevFrame = new RenderTexture(mScreenSize.x, mScreenSize.y, 0, UnityEngine.Experimental.Rendering.DefaultFormat.LDR);
                mPrevFrame.name = "PrevFrame";
                mPrevFrame.Create();
                Graphics.SetRenderTarget(mPrevFrame);
                {
                    cBlock.Clear();
                    cBlock.SetColor(cShaderPropColor, cColorWhite);
                    var t = new Vector3(mBoxCanvas.MinX, mBoxCanvas.MaxY, 0f);
                    var s = new Vector3(mBoxCanvas.Width, mBoxCanvas.Height, 1f);
                    var matrix = Matrix4x4.TRS(t, Quaternion.identity, s);
                    Graphics.DrawMesh(cMeshRect, matrix, cMaterialOpaque, 0, cCamera, 0, cBlock);
                }
                Graphics.SetRenderTarget(null);
            }
        }

        public int ScreenToCanvasX(int screenX)
        {
            return Mathf.RoundToInt((screenX - mRectScreen.xMin) * mCanvasSize.x / mRectScreen.width);
        }

        public int ScreenToCanvasY(int screenY)
        {
            return Mathf.RoundToInt(mCanvasSize.y - (screenY - mRectScreen.yMin) * mCanvasSize.y / mRectScreen.height);
        }

        public void ScreenToCanvas(out int canvasX, out int canvasY, ref int screenX, ref int screenY)
        {
            canvasX = ScreenToCanvasX(screenX);
            canvasY = ScreenToCanvasY(screenY);
        }

        public Vector2 ScreenToCanvas(ref Vector2 screen)
        {
            var canvasX = (screen.x - mRectScreen.xMin) * mCanvasSize.x / mRectScreen.width;
            var canvasY = mCanvasSize.y - (screen.y - mRectScreen.yMin) * mCanvasSize.y / mRectScreen.height;
            return new Vector2(canvasX, canvasY);
        }

        public void CanvasToScreen(out float screenX, out float screenY, ref int canvasX, ref int canvasY)
        {
            screenX = canvasX * mRectScreen.width / mCanvasSize.x + mRectScreen.xMin;
            screenY = (mCanvasSize.y - canvasY) * mRectScreen.height / mCanvasSize.y + mRectScreen.yMin;
        }

        public int DeviceScreenWidth { get { return mScreenSize.x; } }

        public int DeviceScreenHeight { get { return mScreenSize.y; }}

        public int CanvasWidth { get { return (int)mCanvasSize.x; } }

        public int CanvasHeight { get { return (int)mCanvasSize.y; } }

        // 互換実装：文字列系

        public void DrawString(ref string str, ref int x, ref int y)
        {
            DrawStringInternal(ref str, ref x, ref y, TextAnchor.UpperLeft);
        }

        public void DrawCenterString(ref string str, ref int x, ref int y)
        {
            DrawStringInternal(ref str, ref x, ref y, TextAnchor.UpperCenter);
        }

        public void DrawRightString(ref string str, ref int x, ref int y)
        {
            DrawStringInternal(ref str, ref x, ref y, TextAnchor.UpperRight);
        }

        public void SetFont(ref int fontId, ref FontStyle fontStyle, ref int fontSize)
        {
            var font = cRes.GetFnt(fontId);
            if (font.Data == null) return;

            mFont = font.Data;
            mFontStyle = fontStyle;
            mFontSize = fontSize;
        }

        public void SetFontSize(ref int fontSize)
        {
            mFontSize = fontSize;
        }

        public int GetStringWidth(ref string str)
        {
            if (mCountText == cTextGeneratorPool.Count)
            {
                cTextGeneratorPool.Add(new TextGenerator(str.Length));
                cMeshPoolText.Add(new Mesh());
                //cMeshPoolText[mCountText].MarkDynamic();
            }

            var generator = cTextGeneratorPool[mCountText];
            var mesh = cMeshPoolText[mCountText];

            TextGenerationSettings settings;
            var anchor = TextAnchor.UpperLeft;
            GenTextSetting(out settings, ref mFont, ref mFontStyle, ref mFontSize, ref mColor, ref anchor);
            return Mathf.CeilToInt(generator.GetPreferredWidth(str, settings));
        }

        // 互換実装：図形系

        public void SetColor(ref Color color)
        {
            mColor = color;
        }

        public void SetColor(ref int color)
        {
            const float n = 1f / 255;
            var r = (color >> 16) & 0xFF;
            var g = (color >> 8) & 0xFF;
            var b = color & 0xFF;
            mColor = new Color(r * n, g * n, b * n);
        }

        public void SetColor(ref int r, ref int g, ref int b)
        {
            const float n = 1f / 255;
            mColor = new Color(r * n, g * n, b * n);
        }

        public void SetColor(ref int r, ref int g, ref int b, ref int a)
        {
            const float n = 1f / 255;
            mColor = new Color(r * n, g * n, b * n, a * n);
        }

        public void SetColorHSV(ref float h, ref float s, ref float v)
        {
            mColor = Color.HSVToRGB(h, s, v);
        }

        public void DrawLine(ref int startX, ref int startY, ref int endX, ref int endY)
        {
            if (mIsDispose) return;

            var p0 = new Vector2(startX, mCanvasSize.y - startY);
            var p1 = new Vector2(endX, mCanvasSize.y - endY);
            var distance = Vector2.Distance(p0, p1);
            var degree = Vector2.SignedAngle(cVectorRight, p1 - p0);

            cBlock.Clear();
            cBlock.SetColor(cShaderPropColor, mColor);

            var matrix = CalcMatrix(mCountDraw++, startX, startY, distance, 1f, degree);
#if !UNITY_EDITOR
            const bool hasAlpha = true;
#else
            var hasAlpha = (mColor.a != 1f);
#endif // !UNITY_EDITOR
            var buffer = hasAlpha ? cBufferTransparent : cBufferOpaque;
            var material = hasAlpha ? cMaterialTransparentColor : cMaterialOpaque;
            buffer.DrawMesh(cMeshRect, matrix, material, 0, -1, cBlock);
        }

        public void DrawRect(ref int x, ref int y, ref int width, ref int height)
        {
            if (mIsDispose) return;

            Mesh mesh;
            List<Vector3> verts;
            if (mCountDrawRect == cMeshPoolDrawRect.Count)
            {
                InitMeshAsRectBorder(out mesh, out verts);
                cMeshPoolDrawRect.Add(mesh);
                cVertsPool.Add(verts);
            }
            else
            {
                mesh = cMeshPoolDrawRect[mCountDrawRect];
                verts = cVertsPool[mCountDrawRect];
            }
            mCountDrawRect++;

            {
                var half = 0.5f * mPixelSizeMin;
                var l0 = -half;
                var l1 = half;
                var t0 = half;
                var t1 = -half;
                var r0 = width + half;
                var r1 = width - half;
                var b0 = -half - height;
                var b1 = half - height;
                verts[0] = new Vector3(l0, t0); // 左上:外
                verts[1] = new Vector3(l1, t1); // 左上:内
                verts[2] = new Vector3(r0, t0); // 右上:外
                verts[3] = new Vector3(r1, t1); // 右上:内
                verts[4] = new Vector3(r0, b0); // 右下:外
                verts[5] = new Vector3(r1, b1); // 右下:内
                verts[6] = new Vector3(l0, b0); // 左下:外
                verts[7] = new Vector3(l1, b1); // 左下:内
                mesh.SetVertices(verts);
            }

            cBlock.Clear();
            cBlock.SetColor(cShaderPropColor, mColor);

            var matrix = CalcMatrix(mCountDraw++, x, y, 1f, 1f);
#if !UNITY_EDITOR
            const bool hasAlpha = true;
#else
            var hasAlpha = (mColor.a != 1f);
#endif // !UNITY_EDITOR
            var buffer = hasAlpha ? cBufferTransparent : cBufferOpaque;
            var material = hasAlpha ? cMaterialTransparentColor : cMaterialOpaque;
            buffer.DrawMesh(mesh, matrix, material, 0, -1, cBlock);
        }

        public void FillRect(ref int x, ref int y, ref int width, ref int height)
        {
            if (mIsDispose) return;

            cBlock.Clear();
            cBlock.SetColor(cShaderPropColor, mColor);

            var matrix = CalcMatrix(mCountDraw++, x, y, width, height);
#if !UNITY_EDITOR
            const bool hasAlpha = true;
#else
            var hasAlpha = (mColor.a != 1f);
#endif // !UNITY_EDITOR
            var buffer = hasAlpha ? cBufferTransparent : cBufferOpaque;
            var material = hasAlpha ? cMaterialTransparentColor : cMaterialOpaque;
            buffer.DrawMesh(cMeshRect, matrix, material, 0, -1, cBlock);
        }

        public void DrawCircle(ref int x, ref int y, ref int radius)
        {
            if (mIsDispose) return;

            var half = 0.5f * mPixelSizeMin;
            var r0 = radius - half;
            var r1 = radius + half;
            for (var i = 0; i < cCircleResolution * 2; i += 2)
            {
                var rad = Mathf.PI * i / cCircleResolution;
                var vec = new Vector3(Mathf.Sin(rad), Mathf.Cos(rad));
                cVertsCircleBorder[i] = vec * r1;
                cVertsCircleBorder[i + 1] = vec * r0;
            }
            cMeshCircleBorder.SetVertices(cVertsCircleBorder);

            cBlock.Clear();
            cBlock.SetColor(cShaderPropColor, mColor);

            var matrix = CalcMatrix(mCountDraw++, x, y, 1f, 1f);
#if !UNITY_EDITOR
            const bool hasAlpha = true;
#else
            var hasAlpha = (mColor.a != 1f);
#endif // !UNITY_EDITOR
            var buffer = hasAlpha ? cBufferTransparent : cBufferOpaque;
            var material = hasAlpha ? cMaterialTransparentColor : cMaterialOpaque;
            buffer.DrawMesh(cMeshCircleBorder, matrix, material, 0, -1, cBlock);
        }

        public void FillCircle(ref int x, ref int y, ref int radius)
        {
            if (mIsDispose) return;

            cBlock.Clear();
            cBlock.SetColor(cShaderPropColor, mColor);

            var matrix = CalcMatrix(mCountDraw++, x, y, radius, radius);
#if !UNITY_EDITOR
            const bool hasAlpha = true;
#else
            var hasAlpha = (mColor.a != 1f);
#endif // !UNITY_EDITOR
            var buffer = hasAlpha ? cBufferTransparent : cBufferOpaque;
            var material = hasAlpha ? cMaterialTransparentColor : cMaterialOpaque;
            buffer.DrawMesh(cMeshCircle, matrix, material, 0, -1, cBlock);
        }

        // 互換実装：画像系

        public void SetImageAlpha(ref int alpha)
        {
            mColorMultiply = new Color(1f, 1f, 1f, alpha / 255f);
        }

        public void SetImageMultiplyColor(ref int r, ref int g, ref int b, ref int a)
        {
            const float n = 1f / 255;
            mColorMultiply = new Color(r * n, g * n, b * n, a * n);
        }

        public void SetImageMultiplyColor(ref Color color)
        {
            mColorMultiply = color;
        }

        public void ClearImageMultiplyColor()
        {
            mColorMultiply = cColorWhite;
        }

        public void DrawImage(ref int imageId, ref int x, ref int y)
        {
            if (mIsDispose) return;

            var img = cRes.GetImg(imageId);
            if (img.Data == null) return;

            cBlock.Clear();
            cBlock.SetTexture(cShaderPropMainTex, img.Texture);
            if (mColorMultiply != cColorWhite) cBlock.SetColor(cShaderPropColor, mColorMultiply);

            var matrix = CalcMatrix(mCountDraw++, x, y, 1f, 1f);
            cBufferTransparent.DrawMesh(img.Mesh, matrix, cMaterialTransparentImage, 0, -1, cBlock);
        }

        public void DrawClipImage(ref int imageId, ref int x, ref int y, ref int u, ref int v, ref int width, ref int height)
        {
            if (mIsDispose) return;

            var img = cRes.GetImg(imageId);
            if (img.Data == null) return;

            x = x - u/2;
            y = y - v/2;
            u = u/2;
            v = v/2;

            var l = Mathf.Clamp01((x + u) / mCanvasSize.x);
            var t = Mathf.Clamp01((y + v) / mCanvasSize.y);
            var r = Mathf.Clamp((x + u + width) / mCanvasSize.x, l, 1f);
            var b = Mathf.Clamp((y + v + height) / mCanvasSize.y, t, 1f);
            var clipRect = new Vector4(l, 1f - b, r, 1f - t);

            cBlock.Clear();
            cBlock.SetTexture(cShaderPropMainTex, img.Texture);
            cBlock.SetVector(cShaderPropClipRect, clipRect);
            if (mColorMultiply != cColorWhite) cBlock.SetColor(cShaderPropColor, mColorMultiply);

            var matrix = CalcMatrix(mCountDraw++, x - u, y - v, 1f, 1f);
            cBufferTransparent.DrawMesh(img.Mesh, matrix, cMaterialTransparentImage, 0, -1, cBlock);
        }

        public void DrawScaledRotateImage(ref int imageId, ref int x, ref int y, ref int xSize, ref int ySize, ref float degree)
        {
            if (mIsDispose) return;

            var img = cRes.GetImg(imageId);
            if (img.Data == null) return;

            cBlock.Clear();
            cBlock.SetTexture(cShaderPropMainTex, img.Texture);
            if (mColorMultiply != cColorWhite) cBlock.SetColor(cShaderPropColor, mColorMultiply);

            var matrix = CalcMatrix(mCountDraw++, x, y, xSize * 0.01f, ySize * 0.01f, 360f - degree);
            cBufferTransparent.DrawMesh(img.Mesh, matrix, cMaterialTransparentImage, 0, -1, cBlock);
        }

        public void DrawScaledRotateImage(ref int imageId, ref int x, ref int y, ref int xSize, ref int ySize, ref float degree, ref float centerX, ref float centerY)
        {
            if (mIsDispose) return;

            var img = cRes.GetImg(imageId);
            if (img.Data == null) return;

            cBlock.Clear();
            cBlock.SetTexture(cShaderPropMainTex, img.Texture);
            if (mColorMultiply != cColorWhite) cBlock.SetColor(cShaderPropColor, mColorMultiply);

            var count = mCountDraw++;
            var t = new Vector3(x, mCanvasSize.y - y, count * 0.001f);
            var r = Mathf.Approximately(degree, 0f) ? Quaternion.identity : Quaternion.Euler(0f, 0f, 360f - degree);
            var s = new Vector3(xSize * 0.01f, ySize * 0.01f, 1f);
            var t2 = new Vector3(-centerX, centerY);
            var matrix = Matrix4x4.Translate(t) * Matrix4x4.Rotate(r) * Matrix4x4.Translate(t2) * Matrix4x4.Scale(s);
            cBufferTransparent.DrawMesh(img.Mesh, matrix, cMaterialTransparentImage, 0, -1, cBlock);
        }

        public int GetImageWidth(ref int imageId)
        {
            var img = cRes.GetImg(imageId);
            if (img.Data == null) return 0;
            return Mathf.RoundToInt(img.Data.rect.width);
        }

        public int GetImageHeight(ref int imageId)
        {
            var img = cRes.GetImg(imageId);
            if (img.Data == null) return 0;
            return Mathf.RoundToInt(img.Data.rect.height);
        }

        // 互換実装：その他

        public void ClearScreen()
        {
            if (mIsDispose) return;
            cBufferOpaque.ClearRenderTarget(true, true, cColorWhite);
        }

        // その他

        public void DrawTexture(Texture texture, ref int x, ref int y)
        {
            if (mIsDispose || texture == null) return;

            cBlock.Clear();
            cBlock.SetTexture(cShaderPropMainTex, texture);
            if (mColorMultiply != cColorWhite) cBlock.SetColor(cShaderPropColor, mColorMultiply);

            var matrix = CalcMatrix(mCountDraw++, x, y, texture.width, texture.height);
            cBufferTransparent.DrawMesh(cMeshRect, matrix, cMaterialTransparentImage, 0, -1, cBlock);
        }

        public void DrawClipTexture(Texture texture, ref int x, ref int y, ref int u, ref int v, ref int width, ref int height)
        {
            if (mIsDispose || texture == null) return;

            var l = Mathf.Clamp01((x + u) / mCanvasSize.x);
            var t = Mathf.Clamp01((y + v) / mCanvasSize.y);
            var r = Mathf.Clamp((x + u + width) / mCanvasSize.x, l, 1f);
            var b = Mathf.Clamp((y + v + height) / mCanvasSize.y, t, 1f);
            var clipRect = new Vector4(l, 1f - b, r, 1f - t);

            cBlock.Clear();
            cBlock.SetTexture(cShaderPropMainTex, texture);
            cBlock.SetVector(cShaderPropClipRect, clipRect);
            if (mColorMultiply != cColorWhite) cBlock.SetColor(cShaderPropColor, mColorMultiply);

            var matrix = CalcMatrix(mCountDraw++, x - u, y - v, texture.width, texture.height);
            cBufferTransparent.DrawMesh(cMeshRect, matrix, cMaterialTransparentImage, 0, -1, cBlock);
        }

        public void DrawScaledRotateTexture(Texture texture, ref int x, ref int y, ref int xSize, ref int ySize, ref float degree)
        {
            if (mIsDispose || texture == null) return;

            cBlock.Clear();
            cBlock.SetTexture(cShaderPropMainTex, texture);
            if (mColorMultiply != cColorWhite) cBlock.SetColor(cShaderPropColor, mColorMultiply);

            var matrix = CalcMatrix(mCountDraw++, x, y, texture.width * xSize * 0.01f, texture.height * ySize * 0.01f, 360f - degree);
            cBufferTransparent.DrawMesh(cMeshRect, matrix, cMaterialTransparentImage, 0, -1, cBlock);
        }

        #endregion

        //----------------------------------------------------------
        #region プライベート関数
        //----------------------------------------------------------

        private static void InitMeshAsRect(out Mesh mesh)
        {
            var verts = new[] {
                new Vector3(0f, -1f), // 左下
                new Vector3(1f, -1f), // 右下
                new Vector3(0f, 0f), // 左上
                new Vector3(1f, 0f)  // 右上
            };
            var tris = new[] {
                0, 2, 1,
                2, 3, 1
            };
            var uvs = new[] {
                new Vector2(0f, 0f), // 左下
                new Vector2(1f, 0f), // 右下
                new Vector2(0f, 1f), // 左上
                new Vector2(1f, 1f)  // 右上
            };
            var colors = new[] {
                cColorBlack,
                cColorBlack,
                cColorBlack,
                cColorBlack
            };

            mesh = new Mesh();
            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.uv = uvs;
            mesh.colors = colors;
        }

        private static void InitMeshAsCircle(out Mesh mesh)
        {
            var vertices = new Vector3[cCircleResolution];
            var triangles = new int[cCircleResolution * 3 - 6];
            for (var i = 0; i < cCircleResolution; ++i)
            {
                var rad = Mathf.PI * 2 * i / cCircleResolution;
                var x = Mathf.Sin(rad);
                var y = Mathf.Cos(rad);
                vertices[i] = new Vector3(x, y);
                if (i < cCircleResolution - 2)
                {
                    triangles[i * 3] = 0;
                    triangles[i * 3 + 1] = i + 1;
                    triangles[i * 3 + 2] = i + 2;
                }
            }

            mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
        }

        private static void InitMeshAsRectBorder(out Mesh mesh, out List<Vector3> verts)
        {
            verts = new List<Vector3>(8);
            for (var i = 0; i < 8; ++i)
            {
                verts.Add(default(Vector3));
            }

            var tris = new int[24]
            {
                0, 2, 1, 2, 3, 1,
                2, 4, 3, 4, 5, 3,
                4, 6, 5, 6, 7, 5,
                6, 0, 7, 0, 1, 7
            };

            mesh = new Mesh();
            mesh.SetVertices(verts);
            mesh.triangles = tris;
        }

        private static void InitMeshAsCircleBorder(out Mesh mesh, out List<Vector3> verts)
        {
            verts = new List<Vector3>(cCircleResolution * 2);
            for (var i = 0; i < cCircleResolution * 2; ++i)
            {
                verts.Add(default(Vector3));
            }

            var tris = new int[cCircleResolution * 6];
            for (var i = 0; i < cCircleResolution; ++i)
            {
                var j = i * 6;
                var k = i * 2;
                tris[j] = k;
                tris[j + 1] = k + 2;
                tris[j + 2] = k + 1;
                tris[j + 3] = k + 2;
                tris[j + 4] = k + 3;
                tris[j + 5] = k + 1;
            }
            tris[cCircleResolution * 6 - 5] = 0;
            tris[cCircleResolution * 6 - 3] = 0;
            tris[cCircleResolution * 6 - 2] = 1;

            mesh = new Mesh();
            mesh.SetVertices(verts);
            mesh.triangles = tris;
        }

        private static void GenTextSetting(out TextGenerationSettings settings, ref Font font, ref FontStyle style, ref int size, ref Color color, ref TextAnchor anchor)
        {
            settings = new TextGenerationSettings()
            {
                textAnchor = anchor,
                color = color,
                font = font,
                fontSize = size,
                fontStyle = style,
                verticalOverflow = VerticalWrapMode.Overflow,
                horizontalOverflow = HorizontalWrapMode.Overflow,
                alignByGeometry = true,
                richText = false,
                lineSpacing = 1f,
                scaleFactor = 1f,
                resizeTextForBestFit = false
            };
        }

        private void ConvertToMesh(ref Mesh mesh, ref TextGenerator generator)
        {
            cVertsText.Clear();
            cUvsText.Clear();
            cTrisText.Clear();
            cColorsText.Clear();

            var vertexCount = generator.vertexCount;
            if (cVertsText.Capacity < vertexCount) cVertsText.Capacity = vertexCount;
            if (cUvsText.Capacity < vertexCount) cUvsText.Capacity = vertexCount;
            if (cTrisText.Capacity < vertexCount * 6) cTrisText.Capacity = vertexCount * 6;
            if (cColorsText.Capacity < vertexCount) cColorsText.Capacity = vertexCount;

            var uiverts = generator.verts;
            for (var i = 0; i < vertexCount; i += 4)
            {
                for (var j = 0; j < 4; ++j)
                {
                    var idx = i + j;
                    cVertsText.Add(uiverts[idx].position);
                    cColorsText.Add(uiverts[idx].color);
                    cUvsText.Add(uiverts[idx].uv0);
                }
                cTrisText.Add(i);
                cTrisText.Add(i + 1);
                cTrisText.Add(i + 2);
                cTrisText.Add(i + 2);
                cTrisText.Add(i + 3);
                cTrisText.Add(i);
            }

            mesh.Clear();
            mesh.SetVertices(cVertsText);
            mesh.SetUVs(0, cUvsText);
            mesh.SetTriangles(cTrisText, 0);
            mesh.SetColors(cColorsText);
            mesh.RecalculateBounds();
        }

        private void DrawStringInternal(ref string str, ref int x, ref int y, TextAnchor anchor)
        {
            if (mCountText == cTextGeneratorPool.Count)
            {
                cTextGeneratorPool.Add(new TextGenerator(str.Length));
                cMeshPoolText.Add(new Mesh());
                //cMeshPoolText[mCountText].MarkDynamic();
            }

            var generator = cTextGeneratorPool[mCountText];
            var mesh = cMeshPoolText[mCountText++];

            TextGenerationSettings settings;
            GenTextSetting(out settings, ref mFont, ref mFontStyle, ref mFontSize, ref mColor, ref anchor);
            generator.Populate(str, settings);
            ConvertToMesh(ref mesh, ref generator);

            cBlock.Clear();
            cBlock.SetTexture(cShaderPropMainTex, mFont.material.mainTexture);

            var matrix = CalcMatrix(mCountDraw++, x, y, 1f, 1f);
            cBufferTransparent.DrawMesh(mesh, matrix, cMaterialTransparentImage, 0, -1, cBlock);
        }

        private Matrix4x4 CalcMatrix(int count, float x, float y, float w, float h, float degree = 0f)
        {
            var t = new Vector3(x,  mCanvasSize.y - y, count * 0.001f);
            var r = Mathf.Approximately(degree, 0f) ? Quaternion.identity : Quaternion.Euler(0f, 0f, degree);
            var s = new Vector3(w, h, 1f);
            return Matrix4x4.TRS(t, r, s);
        }

        private void OnTextTextureRebuild(Font font)
        {
            foreach (var generator in cTextGeneratorPool)
            {
                generator.Invalidate();
            }
        }

        #endregion
    }
}
