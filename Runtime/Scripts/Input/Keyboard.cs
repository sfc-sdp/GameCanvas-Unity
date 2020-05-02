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

namespace GameCanvas.Input
{
    public sealed class Keyboard
    {
        //----------------------------------------------------------
        #region フィールド変数
        //----------------------------------------------------------

        private readonly EKeyCode[] cKeyCodes;
        private readonly KeyInfo[] cKeyInfo;
        private readonly Dictionary<EKeyCode, int> cCodeToIndex;

        private TouchScreenKeyboard mScreenKeyboard;

        #endregion

        //----------------------------------------------------------
        #region 内部定義
        //----------------------------------------------------------

        private struct KeyInfo
        {
            public EState State;
            public int FrameCount;
            public float Duration;
        }

        private enum EState
        {
            None,
            Began,
            Ended,
            Stationary
        }

        #endregion

        //----------------------------------------------------------
        #region パブリック関数
        //----------------------------------------------------------

        public void OnBeforeUpdate()
        {
            for (var i = 0; i < cKeyCodes.Length; ++i)
            {
                cKeyInfo[i].State
                    = !UnityEngine.Input.GetKey((KeyCode)cKeyCodes[i]) ? EState.None
                    : UnityEngine.Input.GetKeyDown((KeyCode)cKeyCodes[i]) ? EState.Began
                    : UnityEngine.Input.GetKeyUp((KeyCode)cKeyCodes[i]) ? EState.Ended
                    : EState.Stationary;

                switch (cKeyInfo[i].State)
                {
                    case EState.None:
                        cKeyInfo[i].FrameCount = 0;
                        cKeyInfo[i].Duration = 0f;
                        break;

                    case EState.Began:
                        cKeyInfo[i].FrameCount = 1;
                        cKeyInfo[i].Duration = 0f;
                        break;

                    case EState.Stationary:
                    case EState.Ended:
                        cKeyInfo[i].FrameCount++;
                        cKeyInfo[i].Duration += Time.unscaledDeltaTime;
                        break;
                }
            }

            if (mScreenKeyboard != null)
            {
                switch (mScreenKeyboard.status)
                {
                    case TouchScreenKeyboard.Status.Canceled:
                    case TouchScreenKeyboard.Status.Done:
                    case TouchScreenKeyboard.Status.LostFocus:
                        mScreenKeyboard = null;
                        break;

                    case TouchScreenKeyboard.Status.Visible:
                        if (!mScreenKeyboard.active) mScreenKeyboard = null;
                        break;
                }
            }
        }

        public bool Open()
        {
            if (mScreenKeyboard == null && TouchScreenKeyboard.isSupported)
            {
                TouchScreenKeyboard.hideInput = true; // TODO: Android対応
                mScreenKeyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default);
            }
            return (mScreenKeyboard != null);
        }

        public bool IsVisible { get { return (mScreenKeyboard != null); } }
        public bool IsPressBackButton { get { return (cKeyInfo[cCodeToIndex[EKeyCode.Escape]].State != EState.None); } }

        public int GetPressFrameCount(ref EKeyCode key)
        {
            return cKeyInfo[cCodeToIndex[key]].FrameCount;
        }
        public float GetPressDuration(ref EKeyCode key)
        {
            return cKeyInfo[cCodeToIndex[key]].Duration;
        }
        public bool GetIsPress(ref EKeyCode key)
        {
            return (cKeyInfo[cCodeToIndex[key]].State != EState.None);
        }
        public bool GetIsBegan(ref EKeyCode key)
        {
            return (cKeyInfo[cCodeToIndex[key]].State == EState.Began);
        }
        public bool GetIsEnded(ref EKeyCode key)
        {
            return (cKeyInfo[cCodeToIndex[key]].State == EState.Ended);
        }

        #endregion

        //----------------------------------------------------------
        #region プライベート関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal Keyboard()
        {
            cKeyCodes = (EKeyCode[])System.Enum.GetValues(typeof(EKeyCode));
            cKeyInfo = new KeyInfo[cKeyCodes.Length];
            cCodeToIndex = new Dictionary<EKeyCode, int>(cKeyCodes.Length);
            for (var i = 0; i < cKeyCodes.Length; ++i) cCodeToIndex.Add(cKeyCodes[i], i);
        }

        #endregion
    }
}
