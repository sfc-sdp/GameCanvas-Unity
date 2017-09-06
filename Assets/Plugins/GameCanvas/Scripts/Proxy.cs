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

    public sealed class Proxy
    {
        //----------------------------------------------------------
        #region フィールド変数
        //----------------------------------------------------------

        private readonly Graphic cGraphic;

        #endregion

        //----------------------------------------------------------
        #region パブリック関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Proxy(Graphic graphic)
        {
            cGraphic = graphic;
        }

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
        public int CONFIG_FPS => 30;

        // 音声

        public void playBGM(int soundId, bool loop = true) { }
        public void changeBGMVolume(int volume) { }
        public void stopBGM() { }
        public void pauseBGM() { }
        public void playSE(int soundId, bool loop = false) { }
        public void changeSEVolume(int volume) { }
        public void stopSE() { }
        public void pauseSE() { }

        // 入力

        public int getMouseX() { return 0; }
        public int getMouseY() { return 0; }
        public int getMouseClickLength() { return 0; }
        public bool isMousePushed() { return false; }
        public bool isMouseReleased() { return false; }
        public bool isMousePress() { return false; }
        public bool showYesNoDialog(string message) { return false; }
        public string showInputDialog(string message, string defaultInput) { return null; }

        // 数学

        public void setSeed(int seed) { }
        public int rand(int min, int max) { return 0; }
        public bool checkHitRect(int x1, int y1, int w1, int h1, int x2, int y2, int w2, int h2) { return false; }
        public bool checkHitImage(int imageId1, int x1, int y1, int imageId2, int x2, int y2) { return false; }
        public bool checkHitCircle(int x1, int y1, int r1, int x2, int y2, int r2) { return false; }
        public double sqrt(double value) { return 0; }
        public double cos(double degree) { return 0; }
        public double sin(double degree) { return 0; }
        public double atan2(double x, double y) { return 0; }

        // その他

        public int load(int key) { return 0; }
        public void save(int key, int value) { }
        public void resetGame() { }
        public void exitApp() { }

        // 廃止

        [System.Obsolete]
        public readonly int KEY_UP = 0;
        [System.Obsolete]
        public readonly int KEY_DOWN = 0;
        [System.Obsolete]
        public readonly int KEY_LEFT = 0;
        [System.Obsolete]
        public readonly int KEY_RIGHT = 0;
        [System.Obsolete]
        public readonly int KEY_Z = 0;
        [System.Obsolete]
        public readonly int KEY_X = 0;
        [System.Obsolete]
        public readonly int KEY_C = 0;
        [System.Obsolete]
        public readonly int KEY_V = 0;
        [System.Obsolete]
        public readonly int KEY_ENTER = 0;
        [System.Obsolete]
        public readonly int KEY_SPACE = 0;

        [System.Obsolete, System.Diagnostics.Conditional("ENABLE_GAMECANVAS_JAVA")]
        public void setWindowTitle(string title) { }
        [System.Obsolete, System.Diagnostics.Conditional("ENABLE_GAMECANVAS_JAVA")]
        public void setFont(string fontName, int fontStyle, int fontSize) { }
        [System.Obsolete]
        public int getKeyPressLength() { return 0; }
        [System.Obsolete]
        public bool isKeyPress() { return false; }
        [System.Obsolete]
        public bool isKeyPushed() { return false; }
        [System.Obsolete]
        public bool isKeyReleased() { return false; }

        #endregion
    }
}
