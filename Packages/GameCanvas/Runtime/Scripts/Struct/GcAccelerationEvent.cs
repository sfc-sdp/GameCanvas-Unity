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
    /// 加速度イベント
    /// </summary>
    public readonly struct GcAccelerationEvent : System.IEquatable<GcAccelerationEvent>
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        public static readonly GcAccelerationEvent Null = default;

        /// <summary>
        /// 加速度計の値
        /// </summary>
        /// <remarks>
        /// キャンバス座標系で扱いやすいよう、Y軸, Z軸 の値をそれぞれ反転しています
        /// </remarks>
        public readonly float3 Acceleration;

        /// <summary>
        /// 前回の計測からの経過時間（秒）
        /// </summary>
        public readonly float DeltaTime;

        /// <summary>
        /// 加速度計の実際の値
        /// </summary>
        public readonly float3 RawAcceleration;
        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        public static bool operator !=(GcAccelerationEvent lh, GcAccelerationEvent rh) => !lh.Equals(rh);

        public static bool operator ==(GcAccelerationEvent lh, GcAccelerationEvent rh) => lh.Equals(rh);

        public bool Equals(GcAccelerationEvent other)
        {
            return RawAcceleration.Equals(other.RawAcceleration)
                && DeltaTime.Equals(other.DeltaTime);
        }

        public override bool Equals(object obj) => (obj is GcAccelerationEvent other) && Equals(other);

        public override int GetHashCode()
        {
            return RawAcceleration.GetHashCode()
                & DeltaTime.GetHashCode();
        }

        public override string ToString()
            => $"{nameof(GcAccelerationEvent)}: {{ x: {Acceleration.x:0.00}, y: {Acceleration.y:0.00}, z: {Acceleration.z}, dt: {DeltaTime:0.00} }}";
        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        internal GcAccelerationEvent(in AccelerationEvent e)
        {
            Acceleration = new float3(e.acceleration.x, -e.acceleration.y, -e.acceleration.z);
            RawAcceleration = e.acceleration;
            DeltaTime = e.deltaTime;
        }
        #endregion
    }
}
