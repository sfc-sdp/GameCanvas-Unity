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
    /// 描線の端点の形状
    /// </summary>
    public enum GcLineCap
    {
        /// <summary>
        /// 端点は四角く切られます
        /// </summary>
        Butt,
#if false
        /// <summary>
        /// 端点は丸くなります
        /// </summary>
        Round,
#endif // false
        /// <summary>
        /// 端点に線幅と同じ幅で高さが半分の四角形が描き加えられます
        /// </summary>
        Square
    }
}
