/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2017 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/

namespace GameCanvas
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
        private readonly List<TextGenerator> cTextGeneratorPool;
        private readonly List<Mesh> cTextMeshPool;

        //private readonly List<Mesh[]> cMeshPool;
        //private readonly Queue<UIVertex> cVertsPool;
        //private readonly List<UIVertex> cVertsOpaque;
        //private readonly Dictionary<Texture2D, List<UIVertex>> cVertsTransparent;

        private bool mIsEnable;
        private bool mIsDispose;
        private Vector2 mCanvasSize;
        private Box mBoxViewport;
        private Box mBoxCanvas;
        private Rect mRectScreen;
        private Matrix4x4 mMatrixView;
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
            initMeshAsRect(out cMeshRect);
            initMeshAsCircle(out cMeshCircle);

            mFontStyle = FontStyle.Normal;
            mFontSize = 25;
            mFont = cRes.GetFnt(0).Data ?? Font.CreateDynamicFontFromOSFont(new[] { ".Hiragino Kaku Gothic Interface", "MotoyaLMaru", "MotoyaLCedar", "RobotoSansFallback", "Droid Sans Fallback" }, mFontSize);

            mColor = cColorWhite;
            mCanvasSize = new Vector2(720, 1280);
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

            cBufferOpaque.Clear();
            cBufferOpaque.SetViewport(mRectScreen);
            cBufferOpaque.SetViewMatrix(mMatrixView);

            cBufferTransparent.Clear();
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

        public void DrawLine(ref int startX, ref int startY, ref int endX, ref int endY) { }

        public void DrawRect(ref int x, ref int y, ref int width, ref int height) { }

        public void FillRect(ref int x, ref int y, ref int width, ref int height)
        {
            if (mIsDispose) return;

            cBlock.Clear();
            cBlock.SetColor(cShaderPropColor, mColor);

            var matrix = calcMatrix(mCountDraw++, x, y, width, height);
            cBufferTransparent.DrawMesh(cMeshRect, matrix, cMaterialOpaque, 0, -1, cBlock);
        }

        public void DrawCircle(ref int x, ref int y, ref int radius) { }

        public void FillCircle(ref int x, ref int y, ref int radius)
        {
            if (mIsDispose) return;

            cBlock.Clear();
            cBlock.SetColor(cShaderPropColor, mColor);

            var matrix = calcMatrix(mCountDraw++, x, y, radius, radius);
            cBufferTransparent.DrawMesh(cMeshCircle, matrix, cMaterialOpaque, 0, -1, cBlock);
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

        public void DrawClipImage(ref int imageId, ref int x, ref int y, ref int u, ref int v, ref int width, ref int height) { }

        public void DrawScaledRotateImage(ref int imageId, ref int x, ref int y, ref int xSize, ref int ySize, ref double degree) { }

        public void DrawScaledRotateImage(ref int imageId, ref int x, ref int y, ref int xSize, ref int ySize, ref double degree, ref double centerX, ref double centerY) { }

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

        public bool WriteScreenImage(ref string file) { return false; }

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
            //mesh.normals = new[] {
            //    new Vector3(0f, 0f, -1f), // 左下
            //    new Vector3(0f, 0f, -1f), // 右下
            //    new Vector3(0f, 0f, -1f), // 左上
            //    new Vector3(0f, 0f, -1f)  // 右上
            //};
            mesh.uv = new[] {
                new Vector2(0f, 0f), // 左下
                new Vector2(1f, 0f), // 右下
                new Vector2(0f, 1f), // 左上
                new Vector2(1f, 1f)  // 右上
            };
        }

        private static void initMeshAsCircle(out Mesh mesh)
        {
            const int len = 20;
            var vertices = new Vector3[len];
            var triangles = new int[len * 3 - 6];
            var normals = new Vector3[len];
            for (var i = 0; i < len; ++i)
            {
                var rad = Mathf.PI * 2 * i / len;
                var x = Mathf.Sin(rad);
                var y = Mathf.Cos(rad);
                vertices[i] = new Vector3(x, y);
                if (i < len - 2)
                {
                    triangles[i * 3] = 0;
                    triangles[i * 3 + 1] = i + 1;
                    triangles[i * 3 + 2] = i + 2;
                }
                normals[i] = new Vector3(0f, 0f, -1f);
            }

            mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.normals = normals;
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

        private static void convertToMesh(ref Mesh mesh, ref TextGenerator generator)
        {
            var vertices = mesh.vertices;
            var colors = mesh.colors;
            //var normals = mesh.normals;
            var uv = mesh.uv;
            var triangles = mesh.triangles;
            var vertexCount = generator.vertexCount;

            if (vertices == null || vertices.Length != vertexCount) vertices = new Vector3[vertexCount];
            if (colors == null || colors.Length != vertexCount) colors = new Color[vertexCount];
            //if (normals == null || normals.Length != vertexCount) normals = new Vector3[vertexCount];
            if (uv == null || uv.Length != vertexCount) uv = new Vector2[vertexCount];
            if (triangles == null || triangles.Length != vertexCount * 6) triangles = new int[vertexCount * 6];

            var uiverts = generator.verts;
            for (var i = 0; i < vertices.Length; i += 4)
            {
                for (var j = 0; j < 4; ++j)
                {
                    var idx = i + j;
                    vertices[idx] = uiverts[idx].position;
                    colors[idx] = uiverts[idx].color;
                    //normals[idx] = uiverts[idx].normal;
                    uv[idx] = uiverts[idx].uv0;
                }
                triangles[i * 6] = i;
                triangles[i * 6 + 1] = i + 1;
                triangles[i * 6 + 2] = i + 2;
                triangles[i * 6 + 3] = i + 2;
                triangles[i * 6 + 4] = i + 3;
                triangles[i * 6 + 5] = i;
            }

            mesh.vertices = vertices;
            mesh.colors = colors;
            //mesh.normals = normals;
            mesh.uv = uv;
            mesh.triangles = triangles;
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

        private Matrix4x4 calcMatrix(int count, float x, float y, float w, float h)
        {
            var t = new Vector3(x, mCanvasSize.y - y, 1f - count * 0.001f);
            var r = Quaternion.identity;
            var s = new Vector3(w, h, 1f);
            return Matrix4x4.TRS(t, r, s);
        }

        #endregion
    }
}
