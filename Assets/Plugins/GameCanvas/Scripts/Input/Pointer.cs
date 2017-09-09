/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2017 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/

namespace GameCanvas.Input
{
    using UnityEngine;

    public sealed class Pointer
    {
        //----------------------------------------------------------
        #region フィールド変数
        //----------------------------------------------------------

        // TODO

        #endregion

        //----------------------------------------------------------
        #region パブリック関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public Pointer()
        {
            // TODO
        }

        public int LastX => 0; // TODO
        public int LastY => 0; // TODO

        public bool IsActive => false; // TODO
        public bool IsDown => false; // TODO
        public bool IsUp => false; // TODO
        public bool IsCancel => false; // TODO

        public int ActiveFrameCount => 0; // TODO
        public float ActiveTimeLength => 0; // TODO

        #endregion

        //----------------------------------------------------------
        #region プライベート関数
        //----------------------------------------------------------

        // TODO

        #endregion
    }
}
