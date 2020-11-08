/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine;

namespace GameCanvas.Engine
{
    sealed class GcInputAccelerationEngine : IInputAcceleration, IEngine
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

#pragma warning disable IDE0032
        NativeList<GcAccelerationEvent> m_EventList;
        GcAccelerationEvent m_LastAccelerationEvent;
        bool m_IsEnabled;
#pragma warning restore IDE0032
        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        public int AccelerationEventCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_EventList.Length;
        }

        public GcAccelerationEvent.Enumerable AccelerationEvents
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new GcAccelerationEvent.Enumerable(m_EventList);
        }

        public bool DidUpdateAccelerationThisFrame
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_EventList.Length != 0;
        }

        public bool IsAccelerometerEnabled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_IsEnabled;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => m_IsEnabled = value;
        }

        public GcAccelerationEvent LastAccelerationEvent
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_LastAccelerationEvent;
        }

        public bool TryGetAccelerationEvent(int i, out GcAccelerationEvent e)
        {
            if (i >= 0 && i < m_EventList.Length)
            {
                e = m_EventList[i];
                return true;
            }
            e = GcAccelerationEvent.Null;
            return false;
        }

        public bool TryGetAccelerationEvents(out NativeArray<GcAccelerationEvent>.ReadOnly array, out int count)
        {
            count = m_EventList.Length;

            if (count > 0)
            {
                array = m_EventList.AsArray().AsReadOnly();
                return true;
            }
            array = default;
            return false;
        }
        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal GcInputAccelerationEngine()
        {
            m_IsEnabled = true;
            m_EventList = new NativeList<GcAccelerationEvent>(Allocator.Persistent);
        }

        void System.IDisposable.Dispose()
        {
            if (m_EventList.IsCreated) m_EventList.Dispose();
        }

        void IEngine.OnAfterDraw() { }

        void IEngine.OnBeforeUpdate(in System.DateTimeOffset now)
        {
            m_EventList.Length = 0;
            if (!m_IsEnabled) return;

            var count = Input.accelerationEventCount;
            for (var i = 0; i != count; i++)
            {
                var e = Input.GetAccelerationEvent(i);
                m_EventList.Add(new GcAccelerationEvent(e));
            }

            if (count > 0)
            {
                m_LastAccelerationEvent = m_EventList[count - 1];
            }
        }
        #endregion
    }
}
