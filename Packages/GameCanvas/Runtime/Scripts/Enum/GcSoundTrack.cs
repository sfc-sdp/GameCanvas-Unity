/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/

namespace GameCanvas
{
    /// <summary>
    /// 音声トラック
    /// </summary>
    public enum GcSoundTrack : byte
    {
        /// <summary>
        /// BGM1トラック
        /// </summary>
        BGM1 = 0,
        /// <summary>
        /// BGM2トラック
        /// </summary>
        BGM2 = 1,
        /// <summary>
        /// BGM3トラック
        /// </summary>
        BGM3 = 2,
        /// <summary>
        /// SEトラック
        /// </summary>
        SE = 3,
        /// <summary>
        /// マスタートラック
        /// </summary>
        Master = 255
    }
}
