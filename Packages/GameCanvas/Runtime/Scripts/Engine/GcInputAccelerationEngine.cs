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
using UnityEngine;

namespace GameCanvas.Engine
{
    sealed class GcInputAccelerationEngine : IInputAcceleration, IEngine, IEnumerable<GcAccelerationEvent>
    {
        //----------------------------------------------------------
        #region 構造体
        //----------------------------------------------------------

        [EditorBrowsable(EditorBrowsableState.Never)]
        public struct Enumerator : IEnumerator<GcAccelerationEvent>
        {
            NativeArray<GcAccelerationEvent>.ReadOnly m_Array;
            int m_Count;
            int m_Index;

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
            public void Dispose()
            {
                m_Array = default;
                m_Count = 0;
            }

            public bool MoveNext() => ++m_Index < m_Count;

            public void Reset() => m_Index = -1;
        }
        #endregion

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

        public IEnumerable<GcAccelerationEvent> AccelerationEvents
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Enumerator GetEnumerator() => new Enumerator(m_EventList);

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

        IEnumerator<GcAccelerationEvent> IEnumerable<GcAccelerationEvent>.GetEnumerator()
            => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

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
