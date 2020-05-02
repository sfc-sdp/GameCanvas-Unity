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
    public sealed class Mic
    {
#if !GC_DISABLE_MICROPHONE
        //----------------------------------------------------------
        #region プライベート関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal Mic()
        {
            // TODO
        }

        #endregion
#endif //!GC_DISABLE_MICROPHONE
    }
}
