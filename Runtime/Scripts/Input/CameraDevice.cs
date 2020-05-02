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
using UnityEngine.Assertions;

namespace GameCanvas.Input
{
    public sealed class CameraDevice
    {
#if !GC_DISABLE_CAMERAINPUT
        //----------------------------------------------------------
        #region フィールド変数
        //----------------------------------------------------------

        private readonly Engine.Graphic cGraphic;

        private WebCamDevice[] mDevices;
        private WebCamTexture mTexture = null;
        private int mSelectedIndex = -1;

        #endregion

        //----------------------------------------------------------
        #region パブリック関数
        //----------------------------------------------------------

        /// <summary>
        /// 有効なデバイス数
        /// </summary>
        public int Count => mDevices.Length;
        /// <summary>
        /// 再生中のカメラ映像の幅
        /// </summary>
        public int CurrentWidth => (mTexture == null) ? 0 : mTexture.width;
        /// <summary>
        /// 再生中のカメラ映像の高さ
        /// </summary>
        public int CurrentHeight => (mTexture == null) ? 0 : mTexture.height;
        /// <summary>
        /// 再生中のカメラデバイス名
        /// </summary>
        public string CurrentDeviceName => mTexture?.deviceName;
        /// <summary>
        /// 再生中のカメラ映像の回転角度 (度数法)
        /// </summary>
        public int CurrentRotation => (mTexture == null) ? 0 : mTexture.videoRotationAngle;
        /// <summary>
        /// 再生中のカメラ映像の上下反転有無
        /// </summary>
        public bool IsMirrored => (mTexture != null && mTexture.videoVerticallyMirrored);
        /// <summary>
        /// 再生中のカメラ映像が現在フレームで更新されたかどうか
        /// </summary>
        public bool DidUpdate => (mTexture != null && mTexture.didUpdateThisFrame);

        /// <summary>
        /// デバイス一覧の更新
        /// </summary>
        public void UpdateList()
        {
            mDevices = WebCamTexture.devices ?? new WebCamDevice[0];
        }
        /// <summary>
        /// デバイス名の取得
        /// </summary>
        public string GetName(ref int index)
        {
            if (mDevices == null || mDevices.Length <= index)
            {
                Debug.LogWarningFormat("存在しないカメラデバイスを要求しました");
                return null;
            }
            return mDevices[index].name;
        }
        /// <summary>
        /// 前面カメラかどうか
        /// </summary>
        public bool GetIsFront(ref int index)
        {
            if (mDevices == null || mDevices.Length <= index)
            {
                Debug.LogWarningFormat("存在しないカメラデバイスを要求しました");
                return false;
            }
            return mDevices[index].isFrontFacing;
        }

        /// <summary>
        /// カメラデバイスの選択と再生
        /// </summary>
        public void Start(ref int index)
        {
            if (mTexture != null && mSelectedIndex == index)
            {
                Unpause();
                return;
            }
            if (mDevices == null || mDevices.Length == 0)
            {
                Debug.LogWarning("カメラデバイスを検出できません");
                return;
            }
            if (mDevices.Length <= index)
            {
                Debug.LogWarningFormat("存在しないカメラデバイスを要求しました");
                return;
            }

            Stop();
            mSelectedIndex = index;
            mTexture = new WebCamTexture(mDevices[index].name);
            mTexture.Play();
        }
        /// <summary>
        /// 映像の停止
        /// </summary>
        public void Stop()
        {
            if (mTexture != null)
            {
                mTexture.Stop();
                mTexture = null;
                mSelectedIndex = -1;
            }
        }
        /// <summary>
        /// 映像の一時停止
        /// </summary>
        public void Pause()
        {
            if (mTexture != null && mTexture.isPlaying)
            {
                mTexture.Pause();
            }
        }
        /// <summary>
        /// 映像の一時停止解除
        /// </summary>
        public void Unpause()
        {
            if (mTexture != null && !mTexture.isPlaying)
            {
                mTexture.Play();
            }
        }

        /// <summary>
        /// 描画
        /// </summary>
        public void Draw(ref int x, ref int y)
        {
            if (mTexture != null)
            {
                cGraphic.DrawTexture(mTexture, ref x, ref y);
            }
        }
        /// <summary>
        /// 描画
        /// </summary>
        public void Draw(ref int x, ref int y, ref int u, ref int v, ref int width, ref int height)
        {
            if (mTexture != null)
            {
                cGraphic.DrawClipTexture(mTexture, ref x, ref y, ref u, ref v, ref width, ref height);
            }
        }
        /// <summary>
        /// 描画
        /// </summary>
        public void Draw(ref int x, ref int y, ref int xSize, ref int ySize, ref float degree)
        {
            if (mTexture != null)
            {
                cGraphic.DrawScaledRotateTexture(mTexture, ref x, ref y, ref xSize, ref ySize, ref degree);
            }
        }

        #endregion

        //----------------------------------------------------------
        #region プライベート関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal CameraDevice(Engine.Graphic graphic)
        {
            Assert.IsNotNull(graphic);
            cGraphic = graphic;
            UpdateList();
        }

        #endregion
#else
        internal CameraDevice(Engine.Graphic graphic) { }
#endif //!GC_DISABLE_CAMERAINPUT
    }
}
