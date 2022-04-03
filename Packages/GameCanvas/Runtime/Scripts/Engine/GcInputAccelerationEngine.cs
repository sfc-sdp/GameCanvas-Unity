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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Unity.Collections;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace GameCanvas.Engine
{
    sealed class GcInputAccelerationEngine : IInputAcceleration, IEngine
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        InputStateHistory? m_History = null;
        bool m_IsEnabled = false;
        NativeList<GcAccelerationEvent> m_EventList;
        GcAccelerationEvent m_LastAccelerationEvent;
        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        public int AccelerationEventCount
            => m_EventList.IsCreated ? m_EventList.Length : 0;

        public System.ReadOnlySpan<GcAccelerationEvent> AccelerationEvents
            => m_EventList.AsReadOnlySpan();

        public bool DidUpdateAccelerationThisFrame
            => m_EventList.IsCreated && m_EventList.Length != 0;

        public bool IsAccelerometerSupported
            => Accelerometer.current != null;

        public bool IsAccelerometerEnabled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_IsEnabled;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (m_IsEnabled == value) return;
                if (value)
                {
                    if (!IsAccelerometerSupported)
                    {
                        UnityEngine.Debug.LogWarning($"[{nameof(GcInputAccelerationEngine)}] current device is not supported.\n");
                        return;
                    }
                    m_History?.Dispose();
                    if (!Accelerometer.current.enabled)
                    {
                        UnityEngine.Debug.Log($"[{nameof(GcInputAccelerationEngine)}] EnableDevice\n");
                        InputSystem.EnableDevice(Accelerometer.current);
                    }
                    m_History = new InputStateHistory(Accelerometer.current);
                    m_History.StartRecording();
                    m_IsEnabled = true;
                }
                else
                {
                    if (m_History != null)
                    {
                        m_History.Dispose();
                        m_History = null;
                        if (Accelerometer.current.enabled)
                        {
                            InputSystem.DisableDevice(Accelerometer.current);
                        }
                    }
                    m_IsEnabled = false;
                }
            }
        }

        public float AccelerometerSamplingRate
        {
            get => Accelerometer.current?.samplingFrequency ?? 0f;
            set
            {
                if (value > 0f && Accelerometer.current != null)
                {
                    Accelerometer.current.samplingFrequency = value;
                }
            }
        }

        public GcAccelerationEvent LastAccelerationEvent
            => m_LastAccelerationEvent;

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

        public bool TryGetAccelerationEventAll(out System.ReadOnlySpan<GcAccelerationEvent> events)
        {
            events = m_EventList.AsReadOnlySpan();
            return (m_EventList.Length > 0);
        }

        [System.Obsolete]
        [EditorBrowsable(EditorBrowsableState.Never)]
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
        }

        internal void OnPause()
        {
            if (m_IsEnabled)
            {
                m_History?.Dispose();
                m_History = null;
                InputSystem.DisableDevice(Accelerometer.current);
            }
        }

        internal void OnUnpause()
        {
            if (m_IsEnabled)
            {
                InputSystem.EnableDevice(Accelerometer.current);
                m_History = new InputStateHistory(Accelerometer.current);
                m_History.StartRecording();
            }
        }

        void System.IDisposable.Dispose()
        {
            if (m_EventList.IsCreated) m_EventList.Dispose();

            IsAccelerometerEnabled = false;
        }

        void IEngine.OnAfterDraw()
        {
            if (m_EventList.IsCreated) m_EventList.Dispose();
        }

        void IEngine.OnBeforeUpdate(in System.DateTimeOffset now)
        {
            if (!m_IsEnabled || m_History == null) return;

            m_EventList = new NativeList<GcAccelerationEvent>(Allocator.Temp);

            var prevTime = m_LastAccelerationEvent.Time;
            foreach (var e in m_History)
            {
                var acce = (Accelerometer)e.control;
                var time = (float)e.time;
                UnityEngine.Debug.Log($"[{nameof(GcInputAccelerationEngine)}] {acce.acceleration.ReadValue()}\n");
                m_EventList.Add(GcAccelerationEvent.FromAccelerometer(acce, time, prevTime));
                prevTime = time;
            }
            m_History.Clear();

            if (m_EventList.Length > 0)
            {
                m_LastAccelerationEvent = m_EventList[m_EventList.Length - 1];
            }
        }
        #endregion
    }
}
