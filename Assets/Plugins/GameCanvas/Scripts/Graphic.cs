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
    using UnityEngine.Rendering;

    public sealed class Graphic
    {
        //----------------------------------------------------------
        #region フィールド変数
        //----------------------------------------------------------

        private readonly Camera cCamera;
        private readonly CommandBuffer cBufferOpaque;
        private readonly CommandBuffer cBufferTransparent;

        #endregion

        //----------------------------------------------------------
        #region パブリック関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Graphic(Camera camera)
        {
            cBufferOpaque = new CommandBuffer();
            cBufferTransparent = new CommandBuffer();
            cCamera = camera;
        }

        public void OnEnable()
        {
            cCamera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, cBufferOpaque);
            cCamera.AddCommandBuffer(CameraEvent.AfterForwardAlpha, cBufferTransparent);
        }

        public void OnDisable()
        {
            cCamera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, cBufferOpaque);
            cCamera.RemoveCommandBuffer(CameraEvent.AfterForwardAlpha, cBufferTransparent);
        }

        public void OnBeforeUpdate()
        {
            cBufferOpaque.Clear();
            cBufferTransparent.Clear();
        }

        public void ClearScreen() { }

        public void SetColor(ref int r, ref int g, ref int b) { }

        public void DrawImage(ref int imageId, ref int x, ref int y) { }

        public void DrawString(ref string text, ref int x, ref int y) { }

        #endregion
    }
}
