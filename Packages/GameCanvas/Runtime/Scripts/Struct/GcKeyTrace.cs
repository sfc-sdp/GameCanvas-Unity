/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2022 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
#nullable enable
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
        public readonly GcKeyEvent Begin;

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
        #region 公開関数
        //----------------------------------------------------------

        public static bool operator !=(GcKeyTrace left, GcKeyTrace right)
            => !left.Equals(right);

        public static bool operator ==(GcKeyTrace left, GcKeyTrace right)
            => left.Equals(right);

        public readonly bool Equals(GcKeyTrace other)
                            => Begin.Equals(other.Begin)
            && FrameCount.Equals(other.FrameCount)
            && Duration.Equals(other.Duration);

        public readonly override bool Equals(object obj)
            => (obj is GcKeyTrace other) && Equals(other);

        public readonly override int GetHashCode()
        {
            int hashCode = -110147384;
            hashCode = hashCode * -1521134295 + Begin.GetHashCode();
            hashCode = hashCode * -1521134295 + Duration.GetHashCode();
            hashCode = hashCode * -1521134295 + FrameCount.GetHashCode();
            return hashCode;
        }
        #endregion

        //----------------------------------------------------------
        #region 内部関数
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
