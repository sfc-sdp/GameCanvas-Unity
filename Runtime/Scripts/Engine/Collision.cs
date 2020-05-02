/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using UnityEngine.Assertions;

namespace GameCanvas.Engine
{
    public sealed class Collision
    {
        //----------------------------------------------------------
        #region フィールド変数
        //----------------------------------------------------------

        private readonly Resource cRes;

        #endregion

        //----------------------------------------------------------
        #region パブリック関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal Collision(Resource res)
        {
            Assert.IsNotNull(res);
            cRes = res;
        }

        public bool CheckHitRect(ref int x1, ref int y1, ref int w1, ref int h1, ref int x2, ref int y2, ref int w2, ref int h2)
        {
            return (x1 < x2 + w2 && x1 + w1 > x2 && y1 < y2 + h2 && y1 + h1 > y2);
        }

        public bool CheckHitImage(ref int imageId1, ref int x1, ref int y1, ref int imageId2, ref int x2, ref int y2)
        {
            var img1 = cRes.GetImg(imageId1);
            var img2 = cRes.GetImg(imageId2);
            if (img1.Data == null || img2.Data == null) return false;

            var rect1 = img1.Data.rect;
            var rect2 = img2.Data.rect;
            return (x1 < x2 + rect2.width && x1 + rect1.width > x2 && y1 < y2 + rect2.height && y1 + rect1.height > y2);
        }

        public bool CheckHitCircle(ref int x1, ref int y1, ref int r1, ref int x2, ref int y2, ref int r2)
        {
            var r = r1 + r2;
            var x = x1 - x2;
            var y = y1 - y2;
            return (x * x + y * y <= r * r);
        }

        #endregion
    }
}
