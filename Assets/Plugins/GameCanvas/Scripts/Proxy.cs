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

        public static readonly Color COLOR_WHITE = new Color(1f, 1f, 1f);
        public static readonly Color COLOR_BLACK = new Color(0f, 0f, 0f);
        public static readonly Color COLOR_GRAY = new Color(.5f, .5f, .5f);
        public static readonly Color COLOR_RED = new Color(1f, 0f, 0f);
        public static readonly Color COLOR_BLUE = new Color(0f, 0f, 1f);
        public static readonly Color COLOR_GREEN = new Color(0f, 1f, 0f);
        public static readonly Color COLOR_YELLOW = new Color(1f, 1f, 0f);
        public static readonly Color COLOR_PURPLE = new Color(1f, 0f, 1f);
        public static readonly Color COLOR_CYAN = new Color(0f, 1f, 1f);
        public static readonly Color COLOR_AQUA = new Color(.5f, .5f, 1f);

        // 描画：文字列

        public void drawString(string str, int x, int y) => cGraphic.DrawString(ref str, ref x, ref y);
        public void drawCenterString(string str, int x, int y) => cGraphic.DrawCenterString(ref str, ref x, ref y);
        public void drawRightString(string str, int x, int y) => cGraphic.DrawRightString(ref str, ref x, ref y);
        public void setFont(Font font, int fontStyle, int fontSize) => cGraphic.SetFont(ref font, ref fontStyle, ref fontSize);
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

        #endregion
    }
}
