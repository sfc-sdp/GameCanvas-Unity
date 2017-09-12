/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2017 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/

namespace GameCanvas.Engine
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Assertions;
    using UnityEngine.Rendering;

    public sealed class Graphic : System.IDisposable
    {
        //----------------------------------------------------------
        #region フィールド変数
        //----------------------------------------------------------

        private const int cCircleResolution = 24;

        private static readonly Vector2 cVectorRight = Vector2.right;
        private static readonly Color cColorWhite = Color.white;

        private readonly Resource cRes;
        private readonly Camera cCamera;
        private readonly CommandBuffer cBufferOpaque;
        private readonly CommandBuffer cBufferTransparent;
        private readonly Material cMaterialOpaque;
        private readonly Material cMaterialTransparent;
        private readonly MaterialPropertyBlock cBlock;
        private readonly int cShaderPropColor;
        private readonly int cShaderPropMainTex;
        private readonly Mesh cMeshRect;
        private readonly Mesh cMeshCircle;
        private readonly Mesh cMeshRectBorder;
        private readonly Mesh cMeshCircleBorder;
        private readonly List<Vector3> cVertexRectBorder;
        private readonly List<Vector3> cVertexCircleBorder;
        private readonly List<Vector3> cVertexText;
        private readonly List<Vector2> cUvText;
        private readonly List<int> cTriangleText;
        private readonly List<Color> cColorText;
        private readonly List<TextGenerator> cTextGeneratorPool;
        private readonly List<Mesh> cTextMeshPool;

        //private readonly List<Mesh[]> cMeshPool;
        //private readonly Queue<UIVertex> cVertsPool;
        //private readonly List<UIVertex> cVertsOpaque;
        //private readonly Dictionary<Texture2D, List<UIVertex>> cVertsTransparent;

        private bool mIsEnable;
        private bool mIsDispose;
        private Vector2 mScreenSize;
        private Vector2 mCanvasSize;
        private Box mBoxViewport;
        private Box mBoxCanvas;
        private Rect mRectScreen;
        private Matrix4x4 mMatrixView;
        private float mPixelSizeMin;
        private Color mColor;
        private Font mFont;
        private FontStyle mFontStyle;
        private int mFontSize;

        private int mCountDraw;
        private int mCountText;

        #endregion

        //----------------------------------------------------------
        #region パブリック関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Graphic(Resource res, Camera camera)
        {
            Assert.IsNotNull(res);
            Assert.IsNotNull(camera);

            cRes = res;
            cCamera = camera;
            cCamera.clearFlags = CameraClearFlags.Depth;
            cCamera.orthographic = true;
            cCamera.orthographicSize = 5;
            cCamera.farClipPlane = 100;
            cCamera.nearClipPlane = 0;
            cBufferOpaque = new CommandBuffer();
            cBufferOpaque.name = "GameCanvas Opaque";
            cBufferTransparent = new CommandBuffer();
            cBufferTransparent.name = "GameCanvas Transparent ";
            cMaterialOpaque = new Material(res.ShaderOpaque);
            cMaterialTransparent = new Material(res.ShaderTransparent);
            cBlock = new MaterialPropertyBlock();
            cShaderPropColor = Shader.PropertyToID("_Color");
            cShaderPropMainTex = Shader.PropertyToID("_MainTex");
            cTextGeneratorPool = new List<TextGenerator>(20);
            cTextMeshPool = new List<Mesh>(20);
            cVertexText = new List<Vector3>();
            cUvText = new List<Vector2>();
            cTriangleText = new List<int>();
            cColorText = new List<Color>();
            initMeshAsRect(out cMeshRect);
            initMeshAsCircle(out cMeshCircle);
            initMeshAsRectBorder(out cMeshRectBorder, out cVertexRectBorder);
            initMeshAsCircleBorder(out cMeshCircleBorder, out cVertexCircleBorder);

            mFontStyle = FontStyle.Normal;
            mFontSize = 25;
            mFont = cRes.GetFnt(0).Data ?? Font.CreateDynamicFontFromOSFont(new[] { ".Hiragino Kaku Gothic Interface", "MotoyaLMaru", "MotoyaLCedar", "RobotoSansFallback", "Droid Sans Fallback" }, mFontSize);

            mColor = cColorWhite;
            mScreenSize = new Vector2(Screen.width, Screen.height);
            mCanvasSize = new Vector2(720, 1280);
            Application.targetFrameRate = 30;
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
            }
        }

        public void OnDisable()
        {
            if (mIsEnable && !mIsDispose)
            {
                mIsEnable = false;
                cCamera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, cBufferOpaque);
                cCamera.RemoveCommandBuffer(CameraEvent.BeforeForwardAlpha, cBufferTransparent);
            }
        }

        public void OnBeforeUpdate()
        {
            if (mIsDispose) return;

            if (mScreenSize.x != Screen.width || mScreenSize.y != Screen.height)
            {
                mScreenSize = new Vector2(Screen.width, Screen.height);
                SetResolution((int)mCanvasSize.x, (int)mCanvasSize.y);
            }

            cBufferOpaque.Clear();
            cBufferOpaque.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            cBufferOpaque.SetViewport(mRectScreen);
            cBufferOpaque.SetViewMatrix(mMatrixView);

            cBufferTransparent.Clear();
            cBufferTransparent.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            cBufferTransparent.SetViewport(mRectScreen);
            cBufferTransparent.SetViewMatrix(mMatrixView);

            mCountDraw = 0;
            mCountText = 0;
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

            mPixelSizeMin = Mathf.Max(1f, mCanvasSize.x / mRectScreen.width);
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

        public int CanvasWidth => (int)mCanvasSize.x;

        public int CanvasHeight => (int)mCanvasSize.y;

        // 互換実装：文字列系

        public void DrawString(ref string str, ref int x, ref int y)
        {
            drawStringInternal(ref str, ref x, ref y, TextAnchor.UpperLeft);
        }

        public void DrawCenterString(ref string str, ref int x, ref int y)
        {
            drawStringInternal(ref str, ref x, ref y, TextAnchor.UpperCenter);
        }

        public void DrawRightString(ref string str, ref int x, ref int y)
        {
            drawStringInternal(ref str, ref x, ref y, TextAnchor.UpperRight);
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
                cTextMeshPool.Add(new Mesh());
                //cTextMeshPool[mCountText].MarkDynamic();
            }

            var generator = cTextGeneratorPool[mCountText];
            var mesh = cTextMeshPool[mCountText];

            TextGenerationSettings settings;
            var anchor = TextAnchor.UpperLeft;
            genTextSetting(out settings, ref mFont, ref mFontStyle, ref mFontSize, ref mColor, ref anchor);
            return Mathf.CeilToInt(generator.GetPreferredWidth(str, settings));
        }

        // 互換実装：図形系

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

        public void DrawLine(ref int startX, ref int startY, ref int endX, ref int endY)
        {
            if (mIsDispose) return;

            cBlock.Clear();
            cBlock.SetColor(cShaderPropColor, mColor);

            var p0 = new Vector2(startX, mCanvasSize.y - startY);
            var p1 = new Vector2(endX, mCanvasSize.y - endY);
            var distance = Vector2.Distance(p0, p1);
            var degree = Vector2.SignedAngle(cVectorRight, p1 - p0);
            var matrix = calcMatrix(mCountDraw++, startX, startY, distance, 1f, degree);
            cBufferOpaque.DrawMesh(cMeshRect, matrix, cMaterialOpaque, 0, -1, cBlock);
        }

        public void DrawRect(ref int x, ref int y, ref int width, ref int height)
        {
            if (mIsDispose) return;

            var half = 0.5f * mPixelSizeMin;
            var l0 = -half;
            var l1 = half;
            var t0 = half;
            var t1 = -half;
            var r0 = width + half;
            var r1 = width - half;
            var b0 = -half - height;
            var b1 = half - height;
            cVertexRectBorder[0] = new Vector3(l0, t0); // 左上:外
            cVertexRectBorder[1] = new Vector3(l1, t1); // 左上:内
            cVertexRectBorder[2] = new Vector3(r0, t0); // 右上:外
            cVertexRectBorder[3] = new Vector3(r1, t1); // 右上:内
            cVertexRectBorder[4] = new Vector3(r0, b0); // 右下:外
            cVertexRectBorder[5] = new Vector3(r1, b1); // 右下:内
            cVertexRectBorder[6] = new Vector3(l0, b0); // 左下:外
            cVertexRectBorder[7] = new Vector3(l1, b1); // 左下:内
            cMeshRectBorder.SetVertices(cVertexRectBorder);

            cBlock.Clear();
            cBlock.SetColor(cShaderPropColor, mColor);

            var matrix = calcMatrix(mCountDraw++, x, y, 1f, 1f);
            cBufferOpaque.DrawMesh(cMeshRectBorder, matrix, cMaterialOpaque, 0, -1, cBlock);
        }

        public void FillRect(ref int x, ref int y, ref int width, ref int height)
        {
            if (mIsDispose) return;

            cBlock.Clear();
            cBlock.SetColor(cShaderPropColor, mColor);

            var matrix = calcMatrix(mCountDraw++, x, y, width, height);
            cBufferOpaque.DrawMesh(cMeshRect, matrix, cMaterialOpaque, 0, -1, cBlock);
        }

        public void DrawCircle(ref int x, ref int y, ref int radius)
        {
            if (mIsDispose) return;

            var half = 0.5f * mPixelSizeMin;
            var r0 = radius - half;
            var r1 = radius + half;
            for (var i = 0; i < cCircleResolution; ++i)
            {
                var rad = Mathf.PI * 2 * i / cCircleResolution;
                var vec = new Vector3(Mathf.Sin(rad), Mathf.Cos(rad));
                cVertexCircleBorder[i * 2] = vec * r1;
                cVertexCircleBorder[i * 2 + 1] = vec * r0;
            }
            cMeshCircleBorder.SetVertices(cVertexCircleBorder);

            cBlock.Clear();
            cBlock.SetColor(cShaderPropColor, mColor);

            var matrix = calcMatrix(mCountDraw++, x, y, 1f, 1f);
            cBufferOpaque.DrawMesh(cMeshCircleBorder, matrix, cMaterialOpaque, 0, -1, cBlock);
        }

        public void FillCircle(ref int x, ref int y, ref int radius)
        {
            if (mIsDispose) return;

            cBlock.Clear();
            cBlock.SetColor(cShaderPropColor, mColor);

            var matrix = calcMatrix(mCountDraw++, x, y, radius, radius);
            cBufferOpaque.DrawMesh(cMeshCircle, matrix, cMaterialOpaque, 0, -1, cBlock);
        }

        // 互換実装：画像系

        public void DrawImage(ref int imageId, ref int x, ref int y)
        {
            if (mIsDispose) return;

            var img = cRes.GetImg(imageId);
            if (img.Data == null) return;

            cBlock.Clear();
            cBlock.SetTexture(cShaderPropMainTex, img.Texture);

            var matrix = calcMatrix(mCountDraw++, x, y, 1f, 1f);
            cBufferTransparent.DrawMesh(img.Mesh, matrix, cMaterialTransparent, 0, -1, cBlock);
        }

        public void DrawClipImage(ref int imageId, ref int x, ref int y, ref int u, ref int v, ref int width, ref int height)
        {
            // TODO
        }

        public void DrawScaledRotateImage(ref int imageId, ref int x, ref int y, ref int xSize, ref int ySize, ref double degree)
        {
            // TODO
        }

        public void DrawScaledRotateImage(ref int imageId, ref int x, ref int y, ref int xSize, ref int ySize, ref double degree, ref double centerX, ref double centerY)
        {
            // TODO
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

        public bool WriteScreenImage(ref string file)
        {
            var filename = file.EndsWith(".png") ? file : file + ".png";
            ScreenCapture.CaptureScreenshot(filename);
            return true; // TODO 仮実装
        }

        #endregion

        //----------------------------------------------------------
        #region プライベート関数
        //----------------------------------------------------------

        private static void initMeshAsRect(out Mesh mesh)
        {
            mesh = new Mesh();
            mesh.vertices = new[] {
                new Vector3(0f, -1f), // 左下
                new Vector3(1f, -1f), // 右下
                new Vector3(0f, 0f), // 左上
                new Vector3(1f, 0f)  // 右上
            };
            mesh.triangles = new[] {
                0, 2, 1,
                2, 3, 1
            };
            mesh.uv = new[] {
                new Vector2(0f, 0f), // 左下
                new Vector2(1f, 0f), // 右下
                new Vector2(0f, 1f), // 左上
                new Vector2(1f, 1f)  // 右上
            };
        }

        private static void initMeshAsCircle(out Mesh mesh)
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

        private static void initMeshAsRectBorder(out Mesh mesh, out List<Vector3> verts)
        {
            verts = new List<Vector3>(8);
            for (var i = 0; i < 8; ++i) verts.Add(default(Vector3));
            mesh = new Mesh();
            mesh.SetVertices(verts);
            mesh.triangles = new int[24]
            {
                0, 2, 1, 2, 3, 1,
                2, 4, 3, 4, 5, 3,
                4, 6, 5, 6, 7, 5,
                6, 0, 7, 0, 1, 7
            };
        }

        private static void initMeshAsCircleBorder(out Mesh mesh, out List<Vector3> verts)
        {
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
            verts = new List<Vector3>(cCircleResolution * 2);
            for (var i = 0; i < cCircleResolution * 2; ++i) verts.Add(default(Vector3));
            mesh.SetVertices(verts);
            mesh.triangles = tris;
        }

        private static void genTextSetting(out TextGenerationSettings settings, ref Font font, ref FontStyle style, ref int size, ref Color color, ref TextAnchor anchor)
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

        private void convertToMesh(ref Mesh mesh, ref TextGenerator generator)
        {
            cVertexText.Clear();
            cUvText.Clear();
            cTriangleText.Clear();
            cColorText.Clear();

            var vertexCount = generator.vertexCount;
            if (cVertexText.Capacity < vertexCount) cVertexText.Capacity = vertexCount;
            if (cUvText.Capacity < vertexCount) cUvText.Capacity = vertexCount;
            if (cTriangleText.Capacity < vertexCount * 6) cTriangleText.Capacity = vertexCount * 6;
            if (cColorText.Capacity < vertexCount) cColorText.Capacity = vertexCount;

            var uiverts = generator.verts;
            for (var i = 0; i < vertexCount; i += 4)
            {
                for (var j = 0; j < 4; ++j)
                {
                    var idx = i + j;
                    cVertexText.Add(uiverts[idx].position);
                    cColorText.Add(uiverts[idx].color);
                    cUvText.Add(uiverts[idx].uv0);
                }
                cTriangleText.Add(i);
                cTriangleText.Add(i + 1);
                cTriangleText.Add(i + 2);
                cTriangleText.Add(i + 2);
                cTriangleText.Add(i + 3);
                cTriangleText.Add(i);
            }

            mesh.SetVertices(cVertexText);
            mesh.SetUVs(0, cUvText);
            mesh.SetTriangles(cTriangleText, 0);
            mesh.SetColors(cColorText);
            mesh.RecalculateBounds();
        }

        private void drawStringInternal(ref string str, ref int x, ref int y, TextAnchor anchor)
        {
            if (mCountText == cTextGeneratorPool.Count)
            {
                cTextGeneratorPool.Add(new TextGenerator(str.Length));
                cTextMeshPool.Add(new Mesh());
                //cTextMeshPool[mCountText].MarkDynamic();
            }

            var generator = cTextGeneratorPool[mCountText];
            var mesh = cTextMeshPool[mCountText++];

            TextGenerationSettings settings;
            genTextSetting(out settings, ref mFont, ref mFontStyle, ref mFontSize, ref mColor, ref anchor);
            generator.Populate(str, settings);
            convertToMesh(ref mesh, ref generator);

            cBlock.Clear();
            cBlock.SetTexture(cShaderPropMainTex, mFont.material.mainTexture);

            var matrix = calcMatrix(mCountDraw++, x, y, 1f, 1f);
            cBufferTransparent.DrawMesh(mesh, matrix, cMaterialTransparent, 0, -1, cBlock);
        }

        private Matrix4x4 calcMatrix(int count, float x, float y, float w, float h, float degree = 0f)
        {
            var t = new Vector3(x, mCanvasSize.y - y, 1f - count * 0.001f);
            var r = degree != 0f ? Quaternion.Euler(0f, 0f, degree) : Quaternion.identity;
            var s = new Vector3(w, h, 1f);
            return Matrix4x4.TRS(t, r, s);
        }

        #endregion
    }
}
