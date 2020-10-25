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
    /// ポインターの軌跡
    /// </summary>
    public struct GcPointerTrace : System.IEquatable<GcPointerTrace>
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        /// <summary>
        /// 開始時の <see cref="GcPointerEvent"/>
        /// </summary>
        public GcPointerEvent Begin;

        /// <summary>
        /// 最新の <see cref="GcPointerEvent"/>
        /// </summary>
        public GcPointerEvent Current;

        /// <summary>
        /// 累計移動距離（キャンバス座標系）
        /// </summary>
        public float Distance;

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
        #region 公開関数
        //----------------------------------------------------------

        public bool Equals(GcPointerTrace other)
            => Begin.Equals(other.Begin)
            && FrameCount.Equals(other.FrameCount)
            && Distance.Equals(other.Distance)
            && Duration.Equals(other.Duration);
        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal GcPointerTrace(in GcPointerEvent begin)
        {
            Begin = begin;
            Current = begin;
            FrameCount = 1;
            Distance = 0f;
            Duration = 0f;
        }
        #endregion
    }
}
