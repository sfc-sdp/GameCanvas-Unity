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
    using GameCanvas.Engine;
    using GameCanvas.Input;
    using Collision = Engine.Collision;

    public sealed class Proxy
    {
        //----------------------------------------------------------
        #region フィールド変数
        //----------------------------------------------------------

        private readonly Graphic cGraphic;
        private readonly Sound cSound;
        private readonly Collision cCollision;
        private readonly Pointer cPointer;
        private readonly Keyboard cKeyboard;

        private System.Random mRandom;

        // 定数

        public readonly EKeyCode KeyUp = EKeyCode.Up;
        public readonly EKeyCode KeyDown = EKeyCode.Down;
        public readonly EKeyCode KeyLeft = EKeyCode.Left;
        public readonly EKeyCode KeyRight = EKeyCode.Right;
        public readonly EKeyCode KeyZ = EKeyCode.Z;
        public readonly EKeyCode KeyX = EKeyCode.X;
        public readonly EKeyCode KeyC = EKeyCode.C;
        public readonly EKeyCode KeyV = EKeyCode.V;
        public readonly EKeyCode KeyEnter = EKeyCode.Enter;
        public readonly EKeyCode KeySpace = EKeyCode.Space;
        public readonly EKeyCode KeyEscape = EKeyCode.Escape;

        public readonly Color ColorWhite = new Color(1f, 1f, 1f);
        public readonly Color ColorBlack = new Color(0f, 0f, 0f);
        public readonly Color ColorGray = new Color(.5f, .5f, .5f);
        public readonly Color ColorRed = new Color(1f, 0f, 0f);
        public readonly Color ColorBlue = new Color(0f, 0f, 1f);
        public readonly Color ColorGreen = new Color(0f, 1f, 0f);
        public readonly Color ColorYeloow = new Color(1f, 1f, 0f);
        public readonly Color ColorPurple = new Color(1f, 0f, 1f);
        public readonly Color ColorCyan = new Color(0f, 1f, 1f);
        public readonly Color ColorAqua = new Color(.5f, .5f, 1f);

        #endregion

        //----------------------------------------------------------
        #region パブリック関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Proxy(Graphic graphic, Sound sound, Collision collision, Pointer pointer, Keyboard keyboard)
        {
            cGraphic = graphic;
            cSound = sound;
            cCollision = collision;
            cPointer = pointer;
            cKeyboard = keyboard;
            mRandom = new System.Random();
        }

        // 描画：文字列

        public void DrawString(string str, int x, int y) => cGraphic.DrawString(ref str, ref x, ref y);
        public void DrawCenterString(string str, int x, int y) => cGraphic.DrawCenterString(ref str, ref x, ref y);
        public void DrawRightString(string str, int x, int y) => cGraphic.DrawRightString(ref str, ref x, ref y);
        public void SetFont(int fontId, FontStyle fontStyle, int fontSize) => cGraphic.SetFont(ref fontId, ref fontStyle, ref fontSize);
        public void SetFontSize(int fontSize) => cGraphic.SetFontSize(ref fontSize);
        public int GetStringWidth(string str) => cGraphic.GetStringWidth(ref str);

        // 描画：図形

        public void SetColor(int color) => cGraphic.SetColor(ref color);
        public void SetColor(int r, int g, int b) => cGraphic.SetColor(ref r, ref g, ref b);
        public void DrawLine(int startX, int startY, int endX, int endY) => cGraphic.DrawLine(ref startX, ref startY, ref endX, ref endY);
        public void DrawRect(int x, int y, int width, int height) => cGraphic.DrawRect(ref x, ref y, ref width, ref height);
        public void FillRect(int x, int y, int width, int height) => cGraphic.FillRect(ref x, ref y, ref width, ref height);
        public void DrawCircle(int x, int y, int radius) => cGraphic.DrawCircle(ref x, ref y, ref radius);
        public void FillCircle(int x, int y, int radius) => cGraphic.FillCircle(ref x, ref y, ref radius);

        // 描画：画像

        public void DrawImage(int imageId, int x, int y) => cGraphic.DrawImage(ref imageId, ref x, ref y);
        public void DrawClipImage(int imageId, int x, int y, int u, int v, int width, int height) => cGraphic.DrawClipImage(ref imageId, ref x, ref y, ref u, ref v, ref width, ref height);
        public void DrawScaledRotateImage(int imageId, int x, int y, int xSize, int ySize, double degree) => cGraphic.DrawScaledRotateImage(ref imageId, ref x, ref y, ref xSize, ref ySize, ref degree);
        public void DrawScaledRotateImage(int imageId, int x, int y, int xSize, int ySize, double degree, double centerX, double centerY) => cGraphic.DrawScaledRotateImage(ref imageId, ref x, ref y, ref xSize, ref ySize, ref degree, ref centerX, ref centerY);
        public int GetImageWidth(int imageId) => cGraphic.GetImageWidth(ref imageId);
        public int GetImageHeight(int imageId) => cGraphic.GetImageHeight(ref imageId);

        // 描画：その他

        public void ClearScreen() => cGraphic.ClearScreen();
        public bool WriteScreenImage(string file) => cGraphic.WriteScreenImage(ref file);
        public int Width => cGraphic.CanvasWidth;
        public int Height => cGraphic.CanvasHeight;
        public int ConfigFps => Application.targetFrameRate;

        // 音声

        public void Play(int soundId, bool loop = false) => cSound.Play(soundId, loop);
        public void ChangeVolume(int volume) => cSound.ChangeVolume(volume);
        public void Stop() => cSound.Stop();
        public void Pause() => cSound.Pause();
        //public void PlayBGM(int midiId, bool loop = true) { } // TODO
        //public void ChangeBGMVolume(int volume) { } // TODO
        //public void StopBGM() { } // TODO
        //public void PauseBGM() { } // TODO

        // 入力

        public int GetKeyPressLength(EKeyCode key) => cKeyboard.GetPressFrameCount(ref key);
        public bool IsKeyPress(EKeyCode key) => cKeyboard.GetIsPress(ref key);
        public bool IsKeyPushed(EKeyCode key) => cKeyboard.GetIsBegan(ref key);
        public bool IsKeyReleased(EKeyCode key) => cKeyboard.GetIsEnded(ref key);
        public int PointerCount => cPointer.Count;
        public bool HasPointerEvent => cPointer.HasEvent;
        public PointerEvent GetPointerEvent(int i) => cPointer.GetRaw(ref i);
        public int GetPointerX(int i) => cPointer.GetX(ref i);
        public int GetPointerY(int i) => cPointer.GetY(ref i);
        public bool GetIsPointerBegan(int i) => cPointer.GetIsBegan(ref i);
        public bool GetIsPointerEnded(int i) => cPointer.GetIsEnded(ref i);
        public int GetPointerFrameCount(int i) => cPointer.GetFrameCount(ref i);
        public float GetPointerDuration(int i) => cPointer.GetDulation(ref i);
        //public bool ShowYesNoDialog(string message) { return false; } // TODO
        //public string ShowInputDialog(string message, string defaultInput) { return null; } // TODO
        public bool ShowScreenKeyboard() => cKeyboard.Open();

        // 数学

        public void SetSeed(int seed) => mRandom = new System.Random(seed);
        public int Rand(int min, int max) => mRandom.Next(min, max + 1);
        public bool CheckHitRect(int x1, int y1, int w1, int h1, int x2, int y2, int w2, int h2) => cCollision.CheckHitRect(ref x1, ref y1, ref w1, ref h1, ref x2, ref y2, ref w2, ref h2);
        public bool CheckHitImage(int imageId1, int x1, int y1, int imageId2, int x2, int y2) => cCollision.CheckHitImage(ref imageId1, ref x1, ref y1, ref imageId2, ref x2, ref y2);
        public bool CheckHitCircle(int x1, int y1, int r1, int x2, int y2, int r2) => cCollision.CheckHitCircle(ref x1, ref y1, ref r1, ref x2, ref y2, ref r2);
        public double Sqrt(double value) => Mathf.Sqrt((float)value);
        public double Cos(double degree) => Mathf.Cos((float)degree * Mathf.Deg2Rad);
        public double Sin(double degree) => Mathf.Sin((float)degree * Mathf.Deg2Rad);
        public double Atan2(double x, double y) => Mathf.Atan2((float)x, (float)y);

        // その他

        public int Load(int key) => PlayerPrefs.GetInt(key.ToString(), 0);
        public void Save(int key, int value) => PlayerPrefs.SetInt(key.ToString(), value);
        public void ResetGame()
        {
            cSound.Stop();
            cGraphic.ClearScreen();
        }
        public void ExitApp() => Application.Quit();

        // 廃止、改名

        [System.Obsolete, System.Diagnostics.Conditional("ENABLE_GAMECANVAS_JAVA")]
        public void SetWindowTitle(string title) { }
        [System.Obsolete, System.Diagnostics.Conditional("ENABLE_GAMECANVAS_JAVA")]
        public void SetFont(string fontName, int fontStyle, int fontSize) { }
        [System.Obsolete("gc.Play()")]
        public void PlaySE(int soundId, bool loop = false) => cSound.Play(soundId, loop);
        [System.Obsolete("gc.ChangeVolume()")]
        public void ChangeSEVolume(int volume) => cSound.ChangeVolume(volume);
        [System.Obsolete("gc.Stop()")]
        public void StopSE() => cSound.Stop();
        [System.Obsolete("gc.Pause()")]
        public void PauseSE() => cSound.Pause();
        [System.Obsolete("gc.GetPointerX(0)")]
        public int GetMouseX() => cPointer.LastX;
        [System.Obsolete("gc.GetPointerY(0)")]
        public int GetMouseY() => cPointer.LastY;
        [System.Obsolete("gc.GetPointerFrameCount(0)")]
        public int GetMouseClickLength() => cPointer.FrameCount;
        [System.Obsolete("gc.GetIsPointerBegan(0)")]
        public bool IsMousePushed() => cPointer.IsBegan;
        [System.Obsolete("gc.GetIsPointerEnded(0)")]
        public bool IsMouseReleased() => cPointer.IsEnded;
        [System.Obsolete("gc.HasPointerEvent")]
        public bool IsMousePress() => cPointer.HasEvent;

        #endregion
    }
}
