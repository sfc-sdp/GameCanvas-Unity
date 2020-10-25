/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using Unity.Mathematics;
using UnityEngine;

namespace GameCanvas
{
    /// <summary>
    /// GNSSによる測位データ
    /// </summary>
    public readonly struct GcGeolocationEvent : System.IEquatable<GcGeolocationEvent>
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        public static readonly GcGeolocationEvent Null = default;
        internal static readonly System.DateTimeOffset EpocTime = new System.DateTimeOffset(1970, 1, 1, 0, 0, 0, System.TimeSpan.Zero);

        /// <summary>
        /// 水平精度（メートル単位）
        /// </summary>
        public readonly float AccuracyHorizontal;
        /// <summary>
        /// 垂直精度（メートル単位）
        /// </summary>
        public readonly float AccuracyVertical;
        /// <summary>
        /// 高度
        /// </summary>
        public readonly float Altitude;
        /// <summary>
        /// 経度
        /// </summary>
        public readonly float Latitude;
        /// <summary>
        /// 緯度
        /// </summary>
        public readonly float Longitude;
        /// <summary>
        /// 計測時刻
        /// </summary>
        public readonly System.DateTimeOffset Time;
        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        public static bool operator !=(GcGeolocationEvent lh, GcGeolocationEvent rh) => !lh.Equals(rh);

        public static bool operator ==(GcGeolocationEvent lh, GcGeolocationEvent rh) => lh.Equals(rh);

        public bool Equals(GcGeolocationEvent other)
            => Time.Equals(other.Time)
            && Longitude.Equals(other.Longitude)
            && Latitude.Equals(other.Latitude)
            && Altitude.Equals(other.Altitude);

        public override bool Equals(object obj) => (obj is GcGeolocationEvent other) && Equals(other);

        public override int GetHashCode()
            => Altitude.GetHashCode()
            ^ Latitude.GetHashCode()
            ^ Longitude.GetHashCode()
            ^ Time.GetHashCode();

        public override string ToString()
            => $"{nameof(GcGeolocationEvent)}: {{ Lat: {Latitude}, Lng: {Longitude}, Alt: {Altitude} }}";

        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal GcGeolocationEvent(in LocationInfo src)
        {
            AccuracyHorizontal = src.horizontalAccuracy;
            AccuracyVertical = src.verticalAccuracy;
            Altitude = src.altitude;
            Latitude = src.latitude;
            Longitude = src.longitude;
            Time = EpocTime.AddMinutes(src.timestamp);
        }

        #endregion
    }
}
