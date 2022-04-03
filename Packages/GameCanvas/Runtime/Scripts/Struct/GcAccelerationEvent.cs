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
using Unity.Mathematics;
using UnityEngine.InputSystem;

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
        /// 計測時刻（秒）
        /// </summary>
        public readonly float Time;

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
            => RawAcceleration.Equals(other.RawAcceleration)
            && Time.Equals(other.Time);

        public override bool Equals(object obj) => (obj is GcAccelerationEvent other) && Equals(other);

        public override int GetHashCode()
        {
            int hashCode = -1216137635;
            hashCode = hashCode * -1521134295 + Time.GetHashCode();
            hashCode = hashCode * -1521134295 + RawAcceleration.GetHashCode();
            return hashCode;
        }

        public override string ToString()
            => $"{nameof(GcAccelerationEvent)}: {{ x: {Acceleration.x:0.00}, y: {Acceleration.y:0.00}, z: {Acceleration.z}, dt: {DeltaTime:0.00} }}";
        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        internal static GcAccelerationEvent FromAccelerometer(in Accelerometer e, in float time, in float prevTime)
        {
            var raw = e.acceleration.ReadValue();
            var dt = time - prevTime;
            return new GcAccelerationEvent(raw, time, dt);
        }

        internal GcAccelerationEvent(in float3 raw, in float time, in float dt)
        {
            RawAcceleration = raw;
            Acceleration = new float3(RawAcceleration.x, -RawAcceleration.y, -RawAcceleration.z);
            Time = time;
            DeltaTime = dt;
        }
        #endregion
    }
}
