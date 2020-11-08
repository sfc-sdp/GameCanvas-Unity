/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Unity.Collections;
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
        #region 構造体
        //----------------------------------------------------------

        [EditorBrowsable(EditorBrowsableState.Never)]
        public readonly struct Enumerable : IEnumerable<GcAccelerationEvent>
        {
            readonly NativeList<GcAccelerationEvent> m_EventList;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerable(in NativeList<GcAccelerationEvent> eventList)
            {
                m_EventList = eventList;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator GetEnumerator()
                => new Enumerator(m_EventList);

            IEnumerator<GcAccelerationEvent> IEnumerable<GcAccelerationEvent>.GetEnumerator()
                => throw new System.NotSupportedException();

            IEnumerator IEnumerable.GetEnumerator()
                => throw new System.NotSupportedException();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public struct Enumerator : IEnumerator<GcAccelerationEvent>
        {
            NativeArray<GcAccelerationEvent>.ReadOnly m_Array;
            int m_Count;
            int m_Index;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(in NativeList<GcAccelerationEvent> eventList)
            {
                m_Array = eventList.AsParallelReader();
                m_Count = eventList.Length;
                m_Index = -1;
            }

            public GcAccelerationEvent Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => (m_Index < m_Count) ? m_Array[m_Index] : default;
            }

            object IEnumerator.Current
                => throw new System.NotSupportedException();

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose()
            {
                m_Array = default;
                m_Count = 0;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() => ++m_Index < m_Count;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset() => m_Index = -1;
        }
        #endregion

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
