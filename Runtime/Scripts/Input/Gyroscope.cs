/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
namespace GameCanvas.Input
{
    public sealed class Gyroscope
    {
#if !GC_DISABLE_GYROSCOPE
        //----------------------------------------------------------
        #region プライベート関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal Gyroscope()
        {
            // TODO
        }

        #endregion
#endif //!GC_DISABLE_GYROSCOPE
    }
}
