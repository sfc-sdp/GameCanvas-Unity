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

        public void clearScreen() => cGraphic.ClearScreen();

        public void setColor(int r, int g, int b) => cGraphic.SetColor(ref r, ref g, ref b);

        public void drawImage(int imageId, int x, int y) => cGraphic.DrawImage(ref imageId, ref x, ref y);

        public void drawString(string text, int x, int y) => cGraphic.DrawString(ref text, ref x, ref y);

        #endregion
    }
}
