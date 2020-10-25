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
    /// キー入力の軌跡
    /// </summary>
    public struct GcKeyTrace : System.IEquatable<GcKeyTrace>
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        /// <summary>
        /// 開始時の <see cref="GcKeyEvent"/>
        /// </summary>
        public GcKeyEvent Begin;

        /// <summary>
        /// 最新の <see cref="GcKeyEvent"/>
        /// </summary>
        public GcKeyEvent Current;

        /// <summary>
        /// 継続時間（秒）
        /// </summary>
        public float Duration;

        /// <summary>
        /// 継続フレーム数
        /// </summary>
        public int FrameCount;
        #endregion

        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        public bool Equals(GcKeyTrace other)
            => Begin.Equals(other.Begin)
            && FrameCount.Equals(other.FrameCount)
            && Duration.Equals(other.Duration);
        #endregion

        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal GcKeyTrace(in GcKeyEvent begin)
        {
            Begin = begin;
            Current = begin;
            FrameCount = 1;
            Duration = 0f;
        }
        #endregion
    }
}
