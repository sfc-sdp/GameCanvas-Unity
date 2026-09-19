/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2026 Smart Device Programming.
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
using System.Collections.Generic;
using UnityEngine.InputSystem.LowLevel;

namespace GameCanvas.Engine
{
    sealed class GcInputPointerEngine : IInputPointer, IEngine
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        const int k_EventNumMax = 16;
        readonly GcContext m_Context;
        readonly GcPointerSource m_Source;
        readonly GcPointerTracker m_Tracker = new();
        readonly List<int> m_ActiveIds = new(16);
        bool m_Paused, m_Focused = true;
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

        public bool IsTouchPressureSupported => IsTouchSupported && Input.touchPressureSupported;

        public bool IsTouchSupported => Touchscreen.current != null;

        public GcPointer Pointer => m_Tracker.Pointer;
        public GcReadOnlyList<GcPointer> Pointers => m_Tracker.Pointers;
        public GcReadOnlyList<GcPointerEvent> PointerEvents => m_Tracker.Events;
        internal void SetPaused(bool paused) { m_Paused = paused; m_Source.SetSuspended(m_Paused || !m_Focused); }
        internal void SetFocused(bool focused) { m_Focused = focused; m_Source.SetSuspended(m_Paused || !m_Focused); }

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

        [System.Obsolete("Will be removed in v8.0.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool TryGetPointerEventArray(out NativeArray<GcPointerEvent>.ReadOnly array, out int count)
        {
            count = m_PointerList.Length;
            if (count != 0)
            {
                array = m_PointerList.AsArray().AsReadOnly();
                return true;
            }
            array = default;
            return false;
        }

        [System.Obsolete("Will be removed in v8.0.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool TryGetPointerEventArray(in GcPointerEventPhase phase, out NativeArray<GcPointerEvent>.ReadOnly array, out int count)
        {
            switch (phase)
            {
                case GcPointerEventPhase.Begin:
                    count = m_PointerListBegin.Length;
                    if (count != 0)
                    {
                        array = m_PointerListBegin.AsArray().AsReadOnly();
                        return true;
                    }
                    break;

                case GcPointerEventPhase.Hold:
                    count = m_PointerListHold.Length;
                    if (count != 0)
                    {
                        array = m_PointerListHold.AsArray().AsReadOnly();
                        return true;
                    }
                    break;

                case GcPointerEventPhase.End:
                    count = m_PointerListEnd.Length;
                    if (count != 0)
                    {
                        array = m_PointerListEnd.AsArray().AsReadOnly();
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

        [System.Obsolete("Will be removed in v8.0.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool TryGetPointerTapPointArray(out NativeArray<float2>.ReadOnly array, out int count)
        {
            count = m_TapPointList.Length;
            if (count != 0)
            {
                array = m_TapPointList.AsArray().AsReadOnly();
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

        [System.Obsolete("Will be removed in v8.0.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool TryGetPointerTraceArray(out NativeArray<GcPointerTrace>.ReadOnly array, out int count)
        {
            count = m_PointerTraceList.Length;
            if (count != 0)
            {
                array = m_PointerTraceList.AsArray().AsReadOnly();
                return true;
            }
            array = default;
            return false;
        }

        [System.Obsolete("Will be removed in v8.0.")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool TryGetPointerTraceArray(in GcPointerEventPhase phase, out NativeArray<GcPointerTrace>.ReadOnly array, out int count)
        {
            switch (phase)
            {
                case GcPointerEventPhase.Hold:
                    count = m_PointerTraceListHold.Length;
                    if (count != 0)
                    {
                        array = m_PointerTraceListHold.AsArray().AsReadOnly();
                        return true;
                    }
                    break;

                case GcPointerEventPhase.End:
                    count = m_PointerTraceListEnd.Length;
                    if (count != 0)
                    {
                        array = m_PointerTraceListEnd.AsArray().AsReadOnly();
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

            // Persistent allocations — reused every frame via Clear() instead of
            // per-frame new/dispose (avoids GC pressure at 60fps).
            m_PointerList = new NativeList<GcPointerEvent>(k_EventNumMax, Allocator.Persistent);
            m_PointerListBegin = new NativeList<GcPointerEvent>(k_EventNumMax, Allocator.Persistent);
            m_PointerListHold = new NativeList<GcPointerEvent>(k_EventNumMax, Allocator.Persistent);
            m_PointerListEnd = new NativeList<GcPointerEvent>(k_EventNumMax, Allocator.Persistent);
            m_PointerTraceList = new NativeList<GcPointerTrace>(k_EventNumMax, Allocator.Persistent);
            m_PointerTraceListHold = new NativeList<GcPointerTrace>(k_EventNumMax, Allocator.Persistent);
            m_PointerTraceListEnd = new NativeList<GcPointerTrace>(k_EventNumMax, Allocator.Persistent);
            m_TapPointList = new NativeList<float2>(k_EventNumMax, Allocator.Persistent);

            m_Source = new GcPointerSource();
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

            m_Source.Dispose();
        }

        void IEngine.OnAfterDraw()
        {
            // No-op — persistent collections are cleared at the start of each frame in OnBeforeUpdate.
        }

        void IEngine.OnBeforeUpdate(in System.DateTimeOffset now)
        {
            m_PointerList.Clear();
            m_PointerListBegin.Clear();
            m_PointerListHold.Clear();
            m_PointerListEnd.Clear();
            m_PointerTraceList.Clear();
            m_PointerTraceListHold.Clear();
            m_PointerTraceListEnd.Clear();
            m_TapPointList.Clear();

            double time = m_Context.Time.TimeSinceStartup;
            double inputTime = InputState.currentTime;
            m_Tracker.BeginFrame(m_Context.Time.CurrentFrame, time);
            for (int i = 0; i < m_Source.Pending.Count; i++)
            {
                var raw = m_Source.Pending[i];
                m_Context.Graphics.ScreenToCanvasPoint(raw.Screen, out float2 point);
                var record = new GcPointerRecord(raw.Device, raw.Contact, raw.Kind, raw.Phase,
                    raw.Screen, time - System.Math.Max(0, inputTime - raw.Time), raw.Present, raw.CancelDevice);
                var size = m_Context.Graphics.CanvasSize;
                m_Tracker.Accept(record, new GcPoint(point.x, point.y),
                    point.x >= 0 && point.y >= 0 && point.x < size.x && point.y < size.y);
            }
            m_Source.Pending.Clear();
            m_Tracker.EndFrame();

            // 従来のイベント・軌跡APIも同じ入力記録から更新する。
            for (var i = 0; i < PointerEvents.Count; i++)
            {
                var e = PointerEvents[i];
                switch (e.Phase)
                {
                    case GcPointerEventPhase.Begin:
                    {
                        var t = new GcPointerTrace(e);
                        if (m_PointerTraceDict.TryAdd(e.Id, t))
                        {
                            m_PointerList.Add(e);
                            m_ActiveIds.Add(e.Id);
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

                    case GcPointerEventPhase.Cancelled:
                    case GcPointerEventPhase.End:
                    {
                        if (m_PointerTraceDict.TryGetValue(e.Id, out var t))
                        {
                            UpdateTrace(ref t, e);
                            m_PointerList.Add(e);
                            if (e.Phase == GcPointerEventPhase.End) m_PointerListEnd.Add(e);
                            m_PointerTraceList.Add(t);
                            if (e.Phase == GcPointerEventPhase.End) m_PointerTraceListEnd.Add(t);
                            m_PointerTraceDict.Remove(e.Id);
                            m_ActiveIds.Remove(e.Id);

                            if (e.Phase == GcPointerEventPhase.End && m_TapSettings.IsTap(t))
                            {
                                m_TapPointList.Add(t.Begin.Point);
                            }
                        }
                    }
                    break;
                }
            }

            // 欠損したHoldイベントを補う
            for (var i = 0; i < m_ActiveIds.Count; i++)
            {
                var t = m_PointerTraceDict[m_ActiveIds[i]];
                if (t.Current.Frame == m_Context.Time.CurrentFrame) continue;
                var e = GcPointerEvent.FromTrace(m_Context, t);
                UpdateTrace(ref t, e);
                m_PointerList.Add(e);
                m_PointerListHold.Add(e);
                m_PointerTraceList.Add(t);
                m_PointerTraceListHold.Add(t);
                m_PointerTraceDict[e.Id] = t;
            }

            if (m_PointerList.Length != 0)
            {
                //m_PointerList.Sort();
                m_LastPointer = m_PointerList[^1];
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
