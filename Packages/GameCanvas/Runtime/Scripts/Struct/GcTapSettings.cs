/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2024 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
#nullable enable
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

        public static readonly GcTapSettings Default = new(25f, 0.125f);

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

        public readonly bool Equals(GcTapSettings other)
            => MaxDistance.Equals(other.MaxDistance) && MaxDuration.Equals(other.MaxDuration);

        public override readonly bool Equals(object obj)
            => (obj is GcTapSettings other) && Equals(other);

        public override readonly int GetHashCode()
            => MaxDistance.GetHashCode() ^ MaxDuration.GetHashCode();

        public override readonly string ToString()
            => $"{nameof(GcTapSettings)}: {{ {nameof(MaxDistance)}: {MaxDistance}, {nameof(MaxDuration)}: {MaxDuration} }}";
        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        internal readonly bool IsTap(in GcPointerTrace trace)
            => (trace.Duration <= MaxDuration) && (trace.Distance <= MaxDistance);

        #endregion
    }
}
