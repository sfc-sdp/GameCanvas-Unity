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
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.LowLevel;

namespace GameCanvas.Engine
{
    sealed class GcInputPointerEngine : IInputPointer, IEngine
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        static readonly bool k_IsTouchSupported = (Touchscreen.current != null) && (Application.platform != RuntimePlatform.WindowsEditor);
        static readonly bool k_IsTouchPressureSupported = k_IsTouchSupported && Input.touchPressureSupported;
        static readonly int k_EventNumMax = (k_IsTouchSupported && Input.multiTouchEnabled) ? 10 : 3;

        readonly GcContext m_Context;
        readonly InputStateHistory m_History;
        GcPointerEvent m_LastPointer = GcPointerEvent.Null;
        NativeList<GcPointerEvent> m_PointerList;
        NativeList<GcPointerEvent> m_PointerListBegin;
        NativeList<GcPointerEvent> m_PointerListEnd;
        NativeList<GcPointerEvent> m_PointerListHold;
        NativeHashMap<int, GcPointerTrace> m_PointerTraceDict;
        NativeList<GcPointerTrace> m_PointerTraceList;
        NativeList<GcPointerTrace> m_PointerTraceListEnd;
        NativeList<GcPointerTrace> m_PointerTraceListHold;
        NativeList<float2> m_TapPointList;
        GcTapSettings m_TapSettings;
        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        public bool IsTouchPressureSupported => k_IsTouchPressureSupported;

        public bool IsTouchSupported => k_IsTouchSupported;

        public GcPointerEvent LastPointerEvent => m_LastPointer;

        public int PointerBeginCount => m_PointerListBegin.Length;

        public int PointerCount => m_PointerList.Length;

        public int PointerEndCount => m_PointerListEnd.Length;

        public int PointerTapCount => m_TapPointList.Length;

        public GcTapSettings TapSettings
        {
            get => m_TapSettings;
            set => m_TapSettings = value;
        }

        public bool TryGetPointerEvent(in int i, out GcPointerEvent e)
        {
            if (i >= 0 && i < m_PointerList.Length)
            {
                e = m_PointerList[i];
                return true;
            }
            e = default;
            return false;
        }

        public bool TryGetPointerEvent(in GcPointerEventPhase phase, in int i, out GcPointerEvent e)
        {
            switch (phase)
            {
                case GcPointerEventPhase.Begin:
                    if (i >= 0 && i < m_PointerListBegin.Length)
                    {
                        e = m_PointerListBegin[i];
                        return true;
                    }
                    break;

                case GcPointerEventPhase.Hold:
                    if (i >= 0 && i < m_PointerListHold.Length)
                    {
                        e = m_PointerListHold[i];
                        return true;
                    }
                    break;

                case GcPointerEventPhase.End:
                    if (i >= 0 && i < m_PointerListEnd.Length)
                    {
                        e = m_PointerListEnd[i];
                        return true;
                    }
                    break;
            }
            e = default;
            return false;
        }

        public bool TryGetPointerEventAll(out System.ReadOnlySpan<GcPointerEvent> events)
        {
            events = m_PointerList.AsReadOnlySpan();
            return (m_PointerList.Length != 0);
        }

        public bool TryGetPointerEventAll(in GcPointerEventPhase phase, out System.ReadOnlySpan<GcPointerEvent> events)
        {
            switch (phase)
            {
                case GcPointerEventPhase.Begin:
                    events = m_PointerListBegin.AsReadOnlySpan();
                    return (m_PointerListBegin.Length != 0);

                case GcPointerEventPhase.Hold:
                    events = m_PointerListHold.AsReadOnlySpan();
                    return (m_PointerListHold.Length != 0);

                case GcPointerEventPhase.End:
                    events = m_PointerListEnd.AsReadOnlySpan();
                    return (m_PointerListEnd.Length != 0);
            }
            events = System.ReadOnlySpan<GcPointerEvent>.Empty;
            return false;
        }

        [System.Obsolete]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool TryGetPointerEventArray(out NativeArray<GcPointerEvent>.ReadOnly array, out int count)
        {
            count = m_PointerList.Length;
            if (count != 0)
            {
                array = m_PointerList.AsParallelReader();
                return true;
            }
            array = default;
            return false;
        }

        [System.Obsolete]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool TryGetPointerEventArray(in GcPointerEventPhase phase, out NativeArray<GcPointerEvent>.ReadOnly array, out int count)
        {
            switch (phase)
            {
                case GcPointerEventPhase.Begin:
                    count = m_PointerListBegin.Length;
                    if (count != 0)
                    {
                        array = m_PointerListBegin.AsParallelReader();
                        return true;
                    }
                    break;

                case GcPointerEventPhase.Hold:
                    count = m_PointerListHold.Length;
                    if (count != 0)
                    {
                        array = m_PointerListHold.AsParallelReader();
                        return true;
                    }
                    break;

                case GcPointerEventPhase.End:
                    count = m_PointerListEnd.Length;
                    if (count != 0)
                    {
                        array = m_PointerListEnd.AsParallelReader();
                        return true;
                    }
                    break;
            }
            array = default;
            count = 0;
            return false;
        }

        public bool TryGetPointerTapPoint(in int i, out float2 point)
        {
            if (i >= 0 && i < m_TapPointList.Length)
            {
                point = m_TapPointList[i];
                return true;
            }
            point = default;
            return false;
        }

        public bool TryGetPointerTapPointAll(out System.ReadOnlySpan<float2> points)
        {
            points = m_TapPointList.AsReadOnlySpan();
            return (m_TapPointList.Length != 0);
        }

        [System.Obsolete]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool TryGetPointerTapPointArray(out NativeArray<float2>.ReadOnly array, out int count)
        {
            count = m_TapPointList.Length;
            if (count != 0)
            {
                array = m_TapPointList.AsParallelReader();
                return true;
            }
            array = default;
            return false;
        }

        public bool TryGetPointerTrace(in int i, out GcPointerTrace trace)
        {
            if (i >= 0 && i < m_PointerTraceList.Length)
            {
                trace = m_PointerTraceList[i];
                return true;
            }
            trace = default;
            return false;
        }

        public bool TryGetPointerTrace(in GcPointerEventPhase phase, in int i, out GcPointerTrace trace)
        {
            switch (phase)
            {
                case GcPointerEventPhase.Hold:
                    if (i >= 0 && i < m_PointerTraceListHold.Length)
                    {
                        trace = m_PointerTraceListHold[i];
                        return true;
                    }
                    break;

                case GcPointerEventPhase.End:
                    if (i >= 0 && i < m_PointerTraceListEnd.Length)
                    {
                        trace = m_PointerTraceListEnd[i];
                        return true;
                    }
                    break;
            }
            trace = default;
            return false;
        }

        public bool TryGetPointerTraceAll(out System.ReadOnlySpan<GcPointerTrace> traces)
        {
            traces = m_PointerTraceList.AsReadOnlySpan();
            return (m_PointerTraceList.Length != 0);
        }

        public bool TryGetPointerTraceAll(in GcPointerEventPhase phase, out System.ReadOnlySpan<GcPointerTrace> traces)
        {
            switch (phase)
            {
                case GcPointerEventPhase.Hold:
                    traces = m_PointerTraceListHold.AsReadOnlySpan();
                    return (m_PointerTraceListHold.Length != 0);

                case GcPointerEventPhase.End:
                    traces = m_PointerTraceListEnd.AsReadOnlySpan();
                    return (m_PointerTraceListEnd.Length != 0);
            }
            traces = System.ReadOnlySpan<GcPointerTrace>.Empty;
            return false;
        }

        [System.Obsolete]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool TryGetPointerTraceArray(out NativeArray<GcPointerTrace>.ReadOnly array, out int count)
        {
            count = m_PointerTraceList.Length;
            if (count != 0)
            {
                array = m_PointerTraceList.AsParallelReader();
                return true;
            }
            array = default;
            return false;
        }

        [System.Obsolete]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool TryGetPointerTraceArray(in GcPointerEventPhase phase, out NativeArray<GcPointerTrace>.ReadOnly array, out int count)
        {
            switch (phase)
            {
                case GcPointerEventPhase.Hold:
                    count = m_PointerTraceListHold.Length;
                    if (count != 0)
                    {
                        array = m_PointerTraceListHold.AsParallelReader();
                        return true;
                    }
                    break;

                case GcPointerEventPhase.End:
                    count = m_PointerTraceListEnd.Length;
                    if (count != 0)
                    {
                        array = m_PointerTraceListEnd.AsParallelReader();
                        return true;
                    }
                    break;
            }
            array = default;
            count = 0;
            return false;
        }
        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        internal GcInputPointerEngine(in GcContext context)
        {
            m_Context = context;
            m_PointerTraceDict = new NativeHashMap<int, GcPointerTrace>(k_EventNumMax, Allocator.Persistent);
            m_TapSettings = GcTapSettings.Default;

            if (!k_IsTouchSupported)
            {
                TouchSimulation.Enable();
            }

            GcAssert.IsNotNull(Touchscreen.current);
            m_History = new InputStateHistory(Touchscreen.current.touches);
            m_History.StartRecording();
        }

        void System.IDisposable.Dispose()
        {
            if (m_PointerList.IsCreated) m_PointerList.Dispose();
            if (m_PointerListBegin.IsCreated) m_PointerListBegin.Dispose();
            if (m_PointerListHold.IsCreated) m_PointerListHold.Dispose();
            if (m_PointerListEnd.IsCreated) m_PointerListEnd.Dispose();
            if (m_PointerTraceList.IsCreated) m_PointerTraceList.Dispose();
            if (m_PointerTraceListHold.IsCreated) m_PointerTraceListHold.Dispose();
            if (m_PointerTraceListEnd.IsCreated) m_PointerTraceListEnd.Dispose();
            if (m_TapPointList.IsCreated) m_TapPointList.Dispose();

            if (m_PointerTraceDict.IsCreated) m_PointerTraceDict.Dispose();

            m_History.Dispose();
        }

        void IEngine.OnAfterDraw()
        {
            if (m_PointerList.IsCreated) m_PointerList.Dispose();
            if (m_PointerListBegin.IsCreated) m_PointerListBegin.Dispose();
            if (m_PointerListHold.IsCreated) m_PointerListHold.Dispose();
            if (m_PointerListEnd.IsCreated) m_PointerListEnd.Dispose();
            if (m_PointerTraceList.IsCreated) m_PointerTraceList.Dispose();
            if (m_PointerTraceListHold.IsCreated) m_PointerTraceListHold.Dispose();
            if (m_PointerTraceListEnd.IsCreated) m_PointerTraceListEnd.Dispose();
            if (m_TapPointList.IsCreated) m_TapPointList.Dispose();
        }

        void IEngine.OnBeforeUpdate(in System.DateTimeOffset now)
        {
            using var canditates = new NativeList<GcPointerEvent>(Allocator.Temp);

            foreach (var record in m_History)
            {
                var touch = (TouchControl)record.control;
                var time = (float)record.time;
                canditates.Add(GcPointerEvent.FromTouch(m_Context, touch, time));
            }
            m_History.Clear();

            var capacity = math.min(canditates.Length, k_EventNumMax);
            m_PointerList = new NativeList<GcPointerEvent>(canditates.Length, Allocator.Temp);
            m_PointerListBegin = new NativeList<GcPointerEvent>(capacity, Allocator.Temp);
            m_PointerListHold = new NativeList<GcPointerEvent>(capacity, Allocator.Temp);
            m_PointerListEnd = new NativeList<GcPointerEvent>(capacity, Allocator.Temp);
            m_PointerTraceList = new NativeList<GcPointerTrace>(capacity, Allocator.Temp);
            m_PointerTraceListHold = new NativeList<GcPointerTrace>(capacity, Allocator.Temp);
            m_PointerTraceListEnd = new NativeList<GcPointerTrace>(capacity, Allocator.Temp);
            m_TapPointList = new NativeList<float2>(capacity, Allocator.Temp);

            for (var i = 0; i < canditates.Length; i++)
            {
                var e = canditates[i];
                switch (e.Phase)
                {
                    case GcPointerEventPhase.Begin:
                    {
                        var t = new GcPointerTrace(e);
                        if (m_PointerTraceDict.TryAdd(e.Id, t))
                        {
                            m_PointerList.Add(e);
                            m_PointerListBegin.Add(e);
                            m_PointerTraceList.Add(t);
                        }
                    }
                    break;

                    case GcPointerEventPhase.Hold:
                    {
                        if (m_PointerTraceDict.TryGetValue(e.Id, out var t))
                        {
                            UpdateTrace(ref t, e);
                            m_PointerList.Add(e);
                            m_PointerListHold.Add(e);
                            m_PointerTraceList.Add(t);
                            m_PointerTraceListHold.Add(t);
                            m_PointerTraceDict[e.Id] = t;
                        }
                    }
                    break;

                    case GcPointerEventPhase.End:
                    {
                        if (m_PointerTraceDict.TryGetValue(e.Id, out var t))
                        {
                            UpdateTrace(ref t, e);
                            m_PointerList.Add(e);
                            m_PointerListEnd.Add(e);
                            m_PointerTraceList.Add(t);
                            m_PointerTraceListEnd.Add(t);
                            m_PointerTraceDict.Remove(e.Id);

                            if (m_TapSettings.IsTap(t))
                            {
                                m_TapPointList.Add(t.Begin.Point);
                            }
                        }
                    }
                    break;
                }
            }

            // 欠損したHoldイベントを補う
            using (var traceArray = m_PointerTraceDict.GetValueArray(Allocator.Temp))
            {
                for (var i = 0; i < traceArray.Length; i++)
                {
                    var t = traceArray[i];
                    if (t.Current.Frame == m_Context.Time.CurrentFrame) continue;

                    var e = GcPointerEvent.FromTrace(m_Context, t);
                    UpdateTrace(ref t, e);
                    m_PointerList.Add(e);
                    m_PointerListHold.Add(e);
                    m_PointerTraceList.Add(t);
                    m_PointerTraceListHold.Add(t);
                    m_PointerTraceDict[e.Id] = t;
                }
            }

            if (m_PointerList.Length != 0)
            {
                //m_PointerList.Sort();
                m_LastPointer = m_PointerList[m_PointerList.Length - 1];
            }

            static void UpdateTrace(ref GcPointerTrace t, in GcPointerEvent curr)
            {
                var prev = t.Current;
                t.Current = curr;
                t.FrameCount = curr.Frame - t.Begin.Frame + 1;
                t.Duration = curr.Time - t.Begin.Time;
                t.Distance += math.distance(curr.Point, prev.Point);
            }
        }
        #endregion
    }
}
