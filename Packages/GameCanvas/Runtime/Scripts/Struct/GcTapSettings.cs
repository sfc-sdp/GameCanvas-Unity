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
    /// タップ感度の設定
    /// </summary>
    public struct GcTapSettings : System.IEquatable<GcTapSettings>
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        public static readonly GcTapSettings Default = new GcTapSettings(25f, 0.125f);

        public float MaxDistance;

        public float MaxDuration;
        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        public GcTapSettings(in float maxDistance, in float maxDuration)
        {
            MaxDuration = maxDuration;
            MaxDistance = maxDistance;
        }

        public bool Equals(GcTapSettings other)
            => MaxDistance.Equals(other.MaxDistance) && MaxDuration.Equals(other.MaxDuration);

        public override bool Equals(object obj)
            => (obj is GcTapSettings other) && Equals(other);

        public override int GetHashCode()
            => MaxDistance.GetHashCode() ^ MaxDuration.GetHashCode();

        public override string ToString()
            => $"{nameof(GcTapSettings)}: {{ {nameof(MaxDistance)}: {MaxDistance}, {nameof(MaxDuration)}: {MaxDuration} }}";
        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        internal bool IsTap(in GcPointerTrace trace)
            => (trace.Duration <= MaxDuration) && (trace.Distance <= MaxDistance);

        #endregion
    }
}
