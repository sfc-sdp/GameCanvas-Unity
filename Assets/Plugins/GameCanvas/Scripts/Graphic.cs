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
    using UnityEngine;
    using UnityEngine.Assertions;
    using UnityEngine.Rendering;

    public sealed class Graphic : System.IDisposable
    {
        //----------------------------------------------------------
        #region フィールド変数
        //----------------------------------------------------------

        private static readonly Color cColorWhite = Color.white;
        private static readonly Matrix4x4 cMatrixIdentity = Matrix4x4.identity;

        private readonly Camera cCamera;
        private readonly CommandBuffer cBufferOpaque;
        private readonly CommandBuffer cBufferTransparent;
        private readonly Material cMaterialOpaque;
        private readonly Material cMaterialTransparent;
        private readonly MaterialPropertyBlock cBlock;
        private readonly int cShaderPropColor;
        private readonly Mesh cMeshQuad;

        private bool mIsEnable;
        private bool mIsDispose;
        private Vector2 mCanvasSize;
        private Box mBoxViewport;
        private Box mBoxCanvas;
        private Rect mRectScreen;
        private Matrix4x4 mMatrixView;
        private Color mColor;

        #endregion

        //----------------------------------------------------------
        #region パブリック関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Graphic(Camera camera, Shader shaderOpaque, Shader shaderTransparent)
        {
            Assert.IsNotNull(camera);
            Assert.IsNotNull(shaderOpaque);
            Assert.IsNotNull(shaderTransparent);

            cCamera = camera;
            cBufferOpaque = new CommandBuffer();
            cBufferOpaque.name = "GameCanvas Opaque";
            cBufferTransparent = new CommandBuffer();
            cBufferTransparent.name = "GameCanvas Transparent ";
            cMaterialOpaque = new Material(shaderOpaque);
            cMaterialTransparent = new Material(shaderTransparent);
            cBlock = new MaterialPropertyBlock();
            cShaderPropColor = Shader.PropertyToID("_Color");
            initMeshAsQuad(out cMeshQuad);

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
                cCamera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, cBufferOpaque);
                cCamera.AddCommandBuffer(CameraEvent.AfterForwardAlpha, cBufferTransparent);
            }
        }

        public void OnDisable()
        {
            if (mIsEnable && !mIsDispose)
            {
                mIsEnable = false;
                cCamera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, cBufferOpaque);
                cCamera.RemoveCommandBuffer(CameraEvent.AfterForwardAlpha, cBufferTransparent);
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

        public void ClearScreen()
        {
            if (mIsDispose) return;
            cBufferOpaque.ClearRenderTarget(true, true, cColorWhite);
        }

        public void SetColor(ref int r, ref int g, ref int b)
        {
            const float n = 1f / 255;
            mColor = new Color(r * n, g * n, b * n);
        }

        public void DrawImage(ref int imageId, ref int x, ref int y)
        {
            if (mIsDispose) return;

            cBlock.Clear();
            cBlock.SetColor(cShaderPropColor, new Color(1f, 0f, 0f));

            var matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(100f, 100f, 1f));
            cBufferOpaque.DrawMesh(cMeshQuad, matrix, cMaterialOpaque, 0, -1, cBlock);
        }

        public void DrawString(ref string text, ref int x, ref int y) { }

        #endregion

        //----------------------------------------------------------
        #region プライベート関数
        //----------------------------------------------------------

        private static void initMeshAsQuad(out Mesh mesh)
        {
            mesh = new Mesh();
            mesh.vertices = new[] {
                new Vector3(0f, 0f), // 左下
                new Vector3(1f, 0f), // 右下
                new Vector3(0f, 1f), // 左上
                new Vector3(1f, 1f)  // 右上
            };
            mesh.triangles = new[] {
                0, 2, 1,
                2, 3, 1
            };
            mesh.normals = new[] {
                new Vector3(0f, 0f, -1f), // 左下
                new Vector3(0f, 0f, -1f), // 右下
                new Vector3(0f, 0f, -1f), // 左上
                new Vector3(0f, 0f, -1f)  // 右上
            };
            mesh.uv = new[] {
                new Vector2(0f, 0f), // 左下
                new Vector2(1f, 0f), // 右下
                new Vector2(0f, 1f), // 左上
                new Vector2(1f, 1f)  // 右上
            };
        }

        #endregion
    }
}
