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

        public readonly EKeyCode KEY_UP = EKeyCode.Up;
        public readonly EKeyCode KEY_DOWN = EKeyCode.Down;
        public readonly EKeyCode KEY_LEFT = EKeyCode.Left;
        public readonly EKeyCode KEY_RIGHT = EKeyCode.Right;
        public readonly EKeyCode KEY_Z = EKeyCode.Z;
        public readonly EKeyCode KEY_X = EKeyCode.X;
        public readonly EKeyCode KEY_C = EKeyCode.C;
        public readonly EKeyCode KEY_V = EKeyCode.V;
        public readonly EKeyCode KEY_ENTER = EKeyCode.Enter;
        public readonly EKeyCode KEY_SPACE = EKeyCode.Space;

        public readonly Color COLOR_WHITE = new Color(1f, 1f, 1f);
        public readonly Color COLOR_BLACK = new Color(0f, 0f, 0f);
        public readonly Color COLOR_GRAY = new Color(.5f, .5f, .5f);
        public readonly Color COLOR_RED = new Color(1f, 0f, 0f);
        public readonly Color COLOR_BLUE = new Color(0f, 0f, 1f);
        public readonly Color COLOR_GREEN = new Color(0f, 1f, 0f);
        public readonly Color COLOR_YELLOW = new Color(1f, 1f, 0f);
        public readonly Color COLOR_PURPLE = new Color(1f, 0f, 1f);
        public readonly Color COLOR_CYAN = new Color(0f, 1f, 1f);
        public readonly Color COLOR_AQUA = new Color(.5f, .5f, 1f);

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

        public void drawString(string str, int x, int y) => cGraphic.DrawString(ref str, ref x, ref y);
        public void drawCenterString(string str, int x, int y) => cGraphic.DrawCenterString(ref str, ref x, ref y);
        public void drawRightString(string str, int x, int y) => cGraphic.DrawRightString(ref str, ref x, ref y);
        public void setFont(int fontId, FontStyle fontStyle, int fontSize) => cGraphic.SetFont(ref fontId, ref fontStyle, ref fontSize);
        public void setFontSize(int fontSize) => cGraphic.SetFontSize(ref fontSize);
        public int getStringWidth(string str) => cGraphic.GetStringWidth(ref str);

        // 描画：図形

        public void setColor(int color) => cGraphic.SetColor(ref color);
        public void setColor(int r, int g, int b) => cGraphic.SetColor(ref r, ref g, ref b);
        public void drawLine(int startX, int startY, int endX, int endY) => cGraphic.DrawLine(ref startX, ref startY, ref endX, ref endY);
        public void drawRect(int x, int y, int width, int height) => cGraphic.DrawRect(ref x, ref y, ref width, ref height);
        public void fillRect(int x, int y, int width, int height) => cGraphic.FillRect(ref x, ref y, ref width, ref height);
        public void drawCircle(int x, int y, int radius) => cGraphic.DrawCircle(ref x, ref y, ref radius);
        public void fillCircle(int x, int y, int radius) => cGraphic.FillCircle(ref x, ref y, ref radius);

        // 描画：画像

        public void drawImage(int imageId, int x, int y) => cGraphic.DrawImage(ref imageId, ref x, ref y);
        public void drawClipImage(int imageId, int x, int y, int u, int v, int width, int height) => cGraphic.DrawClipImage(ref imageId, ref x, ref y, ref u, ref v, ref width, ref height);
        public void drawScaledRotateImage(int imageId, int x, int y, int xSize, int ySize, double degree) => cGraphic.DrawScaledRotateImage(ref imageId, ref x, ref y, ref xSize, ref ySize, ref degree);
        public void drawScaledRotateImage(int imageId, int x, int y, int xSize, int ySize, double degree, double centerX, double centerY) => cGraphic.DrawScaledRotateImage(ref imageId, ref x, ref y, ref xSize, ref ySize, ref degree, ref centerX, ref centerY);
        public int getImageWidth(int imageId) => cGraphic.GetImageWidth(ref imageId);
        public int getImageHeight(int imageId) => cGraphic.GetImageHeight(ref imageId);

        // 描画：その他

        public void clearScreen() => cGraphic.ClearScreen();
        public bool writeScreenImage(string file) => cGraphic.WriteScreenImage(ref file);
        public int WIDTH => cGraphic.CanvasWidth;
        public int HEIGHT => cGraphic.CanvasHeight;
        public int CONFIG_FPS => Application.targetFrameRate;

        // 音声

        public void play(int soundId, bool loop = false) => cSound.Play(soundId, loop);
        public void changeVolume(int volume) => cSound.ChangeVolume(volume);
        public void stop() => cSound.Stop();
        public void pause() => cSound.Pause();
        public void playBGM(int midiId, bool loop = true) { } // TODO
        public void changeBGMVolume(int volume) { } // TODO
        public void stopBGM() { } // TODO
        public void pauseBGM() { } // TODO
        public void playSE(int soundId, bool loop = false) => cSound.Play(soundId, loop);
        public void changeSEVolume(int volume) => cSound.ChangeVolume(volume);
        public void stopSE() => cSound.Stop();
        public void pauseSE() => cSound.Pause();

        // 入力

        public int getKeyPressLength(EKeyCode key) => cKeyboard.GetPressFrameCount(ref key);
        public bool isKeyPress(EKeyCode key) => cKeyboard.GetIsPress(ref key);
        public bool isKeyPushed(EKeyCode key) => cKeyboard.GetIsBegan(ref key);
        public bool isKeyReleased(EKeyCode key) => cKeyboard.GetIsEnded(ref key);
        public int getMouseX() => cPointer.LastX;
        public int getMouseY() => cPointer.LastY;
        public int getMouseClickLength() => cPointer.FrameCount;
        public bool isMousePushed() => cPointer.IsBegan;
        public bool isMouseReleased() => cPointer.IsEnded;
        public bool isMousePress() => cPointer.HasEvent;
        public bool showYesNoDialog(string message) { return false; } // TODO
        public string showInputDialog(string message, string defaultInput) { return null; } // TODO
        public bool showScreenKeyboard() => cKeyboard.Open();

        // 数学

        public void setSeed(int seed) => mRandom = new System.Random(seed);
        public int rand(int min, int max) => mRandom.Next(min, max + 1);
        public bool checkHitRect(int x1, int y1, int w1, int h1, int x2, int y2, int w2, int h2) => cCollision.CheckHitRect(ref x1, ref y1, ref w1, ref h1, ref x2, ref y2, ref w2, ref h2);
        public bool checkHitImage(int imageId1, int x1, int y1, int imageId2, int x2, int y2) => cCollision.CheckHitImage(ref imageId1, ref x1, ref y1, ref imageId2, ref x2, ref y2);
        public bool checkHitCircle(int x1, int y1, int r1, int x2, int y2, int r2) => cCollision.CheckHitCircle(ref x1, ref y1, ref r1, ref x2, ref y2, ref r2);
        public double sqrt(double value) => Mathf.Sqrt((float)value);
        public double cos(double degree) => Mathf.Cos((float)degree * Mathf.Deg2Rad);
        public double sin(double degree) => Mathf.Sin((float)degree * Mathf.Deg2Rad);
        public double atan2(double x, double y) => Mathf.Atan2((float)x, (float)y);

        // その他

        public int load(int key) => PlayerPrefs.GetInt(key.ToString(), 0);
        public void save(int key, int value) => PlayerPrefs.SetInt(key.ToString(), value);
        public void resetGame()
        {
            cSound.Stop();
            cGraphic.ClearScreen();
        }
        public void exitApp() => Application.Quit();

        // 廃止

        [System.Obsolete, System.Diagnostics.Conditional("ENABLE_GAMECANVAS_JAVA")]
        public void setWindowTitle(string title) { }
        [System.Obsolete, System.Diagnostics.Conditional("ENABLE_GAMECANVAS_JAVA")]
        public void setFont(string fontName, int fontStyle, int fontSize) { }

        #endregion
    }
}
