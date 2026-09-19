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
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
#if !UNITY_ANDROID
using Unity.Mathematics;
#endif // !UNITY_ANDROID

namespace GameCanvas.Engine
{
    sealed class GcInputKeyEngine : IInputKey, IEngine
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        const int k_EventNumMax = 32;
        static readonly bool k_IsScreenKeyboardSupported = TouchScreenKeyboard.isSupported;

        readonly GcContext m_Context;
        readonly GcKeySource m_Source = new();
        readonly GcKeyState[] m_States = new GcKeyState[GcKeySource.Capacity];
        readonly GcKeyTrace[] m_Traces = new GcKeyTrace[GcKeySource.Capacity];
        bool m_Focused = true, m_Paused;
        public GcKeyState Key(GcKey key) => (uint)key < m_States.Length ? m_States[(int)key] : default;
        internal void SetFocused(bool value) { m_Focused = value; m_Source.SetSuspended(!m_Focused || m_Paused); }
        internal void SetPaused(bool value) { m_Paused = value; m_Source.SetSuspended(!m_Focused || m_Paused); }
        NativeHashMap<int, int> m_KeyCodeToKeyEventIndex;
        NativeList<GcKeyEvent> m_KeyEventList;
        NativeList<GcKeyEvent> m_KeyEventListOnlyDown;
        NativeList<GcKeyEvent> m_KeyEventListOnlyHold;
        NativeList<GcKeyEvent> m_KeyEventListOnlyUp;
        NativeList<GcKeyTrace> m_KeyTraceList;
        NativeHashMap<int, GcKeyTrace> m_KeyTraceDict;
        NativeList<GcKeyTrace> m_KeyTraceListOnlyHold;
        NativeList<GcKeyTrace> m_KeyTraceListOnlyUp;
        TouchScreenKeyboard? m_ScreenKeyboard;
        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        public bool IsScreenKeyboardSupported => k_IsScreenKeyboardSupported;

        public bool IsScreenKeyboardVisible
            => m_ScreenKeyboard?.status == TouchScreenKeyboard.Status.Visible;

        public int KeyDownCount => m_KeyEventListOnlyDown.Length;

        public int KeyHoldCount => m_KeyEventListOnlyHold.Length;

        public int KeyUpCount => m_KeyEventListOnlyUp.Length;

        public void HideScreenKeyboard()
        {
            if (m_ScreenKeyboard != null)
            {
                m_ScreenKeyboard.active = false;
                m_ScreenKeyboard = null;
            }
        }

        public bool IsKeyDown(in Key key) => Key((GcKey)key).Down;
        public bool IsKeyHold(in Key key) => Key((GcKey)key).Held && !Key((GcKey)key).Down;
        public bool IsKeyPress(in Key key) => Key((GcKey)key).Held;
        public bool IsKeyUp(in Key key) => Key((GcKey)key).Up;

        public bool ShowScreenKeyboard()
        {
            if (!k_IsScreenKeyboardSupported) return false;

            TouchScreenKeyboard.hideInput = true;
            m_ScreenKeyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, false, false, "", 0);
            return m_ScreenKeyboard.active;
        }

        public bool TryGetKeyEvent(in Key key, out GcKeyEvent e)
        {
            if (m_KeyCodeToKeyEventIndex.TryGetValue((int)key, out var index))
            {
                e = m_KeyEventList[index];
                return true;
            }
            e = default;
            return false;
        }

        public bool TryGetKeyEventAll(out System.ReadOnlySpan<GcKeyEvent> events)
        {
            events = m_KeyEventList.AsReadOnlySpan();
            return (m_KeyEventList.Length != 0);
        }

        public bool TryGetKeyEventAll(in GcKeyEventPhase phase, out System.ReadOnlySpan<GcKeyEvent> events)
        {
            switch (phase)
            {
                case GcKeyEventPhase.Down:
                    events = m_KeyEventListOnlyDown.AsReadOnlySpan();
                    return (m_KeyEventListOnlyDown.Length != 0);

                case GcKeyEventPhase.Hold:
                    events = m_KeyEventListOnlyHold.AsReadOnlySpan();
                    return (m_KeyEventListOnlyHold.Length != 0);

                case GcKeyEventPhase.Up:
                    events = m_KeyEventListOnlyUp.AsReadOnlySpan();
                    return (m_KeyEventListOnlyUp.Length != 0);
            }
            events = System.ReadOnlySpan<GcKeyEvent>.Empty;
            return false;
        }

        public bool TryGetKeyTrace(in Key key, out GcKeyTrace trace)
            => m_KeyTraceDict.TryGetValue((int)key, out trace);

        public bool TryGetKeyTraceAll(out System.ReadOnlySpan<GcKeyTrace> traces)
        {
            traces = m_KeyTraceList.AsReadOnlySpan();
            return (m_KeyTraceList.Length != 0);
        }

        public bool TryGetKeyTraceAll(in GcKeyEventPhase phase, out System.ReadOnlySpan<GcKeyTrace> traces)
        {
            switch (phase)
            {
                case GcKeyEventPhase.Hold:
                    traces = m_KeyTraceListOnlyHold.AsReadOnlySpan();
                    return (m_KeyTraceListOnlyHold.Length != 0);

                case GcKeyEventPhase.Up:
                    traces = m_KeyTraceListOnlyUp.AsReadOnlySpan();
                    return (m_KeyTraceListOnlyUp.Length != 0);
            }
            traces = System.ReadOnlySpan<GcKeyTrace>.Empty;
            return false;
        }

        public bool TryGetScreenKeyboardArea(out GcAABB area)
        {
#if !UNITY_ANDROID
            if (m_ScreenKeyboard != null && m_ScreenKeyboard.active)
            {
                var screenRect = TouchScreenKeyboard.area;
                m_Context.Graphics.ScreenToCanvasPoint(screenRect.min, out float2 canvasMin);
                m_Context.Graphics.ScreenToCanvasPoint(screenRect.max, out float2 canvasMax);
                area = GcAABB.MinMax(canvasMin, canvasMax);
                return true;
            }
#endif // !UNITY_ANDROID
            area = default;
            return false;
        }
        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        internal GcInputKeyEngine(in GcContext context)
        {
            m_Context = context;
            m_KeyTraceDict = new NativeHashMap<int, GcKeyTrace>(k_EventNumMax, Allocator.Persistent);

            // Persistent allocations — reused every frame via Clear() instead of
            // per-frame new/dispose (avoids ~900 alloc/sec GC pressure at 60fps).
            m_KeyCodeToKeyEventIndex = new NativeHashMap<int, int>(k_EventNumMax, Allocator.Persistent);
            m_KeyEventList = new NativeList<GcKeyEvent>(k_EventNumMax, Allocator.Persistent);
            m_KeyEventListOnlyDown = new NativeList<GcKeyEvent>(k_EventNumMax, Allocator.Persistent);
            m_KeyEventListOnlyHold = new NativeList<GcKeyEvent>(k_EventNumMax, Allocator.Persistent);
            m_KeyEventListOnlyUp = new NativeList<GcKeyEvent>(k_EventNumMax, Allocator.Persistent);
            m_KeyTraceListOnlyHold = new NativeList<GcKeyTrace>(k_EventNumMax, Allocator.Persistent);
            m_KeyTraceListOnlyUp = new NativeList<GcKeyTrace>(k_EventNumMax, Allocator.Persistent);
            m_KeyTraceList = new NativeList<GcKeyTrace>(k_EventNumMax, Allocator.Persistent);


        }

        void System.IDisposable.Dispose()
        {
            m_ScreenKeyboard = null;

            if (m_KeyEventList.IsCreated) m_KeyEventList.Dispose();
            if (m_KeyEventListOnlyDown.IsCreated) m_KeyEventListOnlyDown.Dispose();
            if (m_KeyEventListOnlyHold.IsCreated) m_KeyEventListOnlyHold.Dispose();
            if (m_KeyEventListOnlyUp.IsCreated) m_KeyEventListOnlyUp.Dispose();
            if (m_KeyTraceListOnlyHold.IsCreated) m_KeyTraceListOnlyHold.Dispose();
            if (m_KeyTraceListOnlyUp.IsCreated) m_KeyTraceListOnlyUp.Dispose();
            if (m_KeyTraceList.IsCreated) m_KeyTraceList.Dispose();
            if (m_KeyCodeToKeyEventIndex.IsCreated) m_KeyCodeToKeyEventIndex.Dispose();

            if (m_KeyTraceDict.IsCreated) m_KeyTraceDict.Dispose();

            m_Source.Dispose();
        }

        void IEngine.OnAfterDraw()
        {
            // No-op — persistent collections are cleared at the start of each frame in OnBeforeUpdate.
        }

        void IEngine.OnBeforeUpdate(in System.DateTimeOffset now)
        {
            m_KeyCodeToKeyEventIndex.Clear();
            m_KeyEventList.Clear(); m_KeyEventListOnlyDown.Clear();
            m_KeyEventListOnlyHold.Clear(); m_KeyEventListOnlyUp.Clear();
            m_KeyTraceList.Clear(); m_KeyTraceListOnlyHold.Clear(); m_KeyTraceListOnlyUp.Clear();
            m_KeyTraceDict.Clear();
            int frame = m_Context.Time.CurrentFrame;
            float time = (float)InputState.currentTime;
            for (int i = 0; i < m_States.Length; i++)
                m_States[i] = new GcKeyState(false, m_States[i].Held, false, false, 0);

            foreach (var record in m_Source.Pending)
            {
                int i = (int)record.Key;
                var previous = m_States[i];
                var e = new GcKeyEvent(record.Key, record.Phase, frame, (float)record.Time);
                bool down = record.Phase == GcKeyEventPhase.Down;
                bool up = record.Phase == GcKeyEventPhase.Up;
                bool cancelled = record.Phase == GcKeyEventPhase.Cancelled;
                if (down) m_Traces[i] = new GcKeyTrace(e);
                var trace = m_Traces[i];
                trace.Current = e;
                trace.Duration = System.Math.Max(0, e.Time - trace.Begin.Time);
                trace.FrameCount = System.Math.Max(1, frame - trace.Begin.Frame + 1);
                m_Traces[i] = trace;
                m_States[i] = new GcKeyState(previous.Down || down, down,
                    previous.Up || up, previous.Cancelled || cancelled, trace.Duration);
                AddEvent(e);
                if (down) m_KeyEventListOnlyDown.Add(e);
                else if (up) { m_KeyEventListOnlyUp.Add(e); m_KeyTraceListOnlyUp.Add(trace); }
            }
            m_Source.Pending.Clear();
            for (int i = 0; i < m_States.Length; i++)
            {
                var state = m_States[i];
                if (!state.Held && !state.Up && !state.Cancelled) continue;
                var trace = m_Traces[i];
                if (state.Held)
                {
                    trace.Duration = System.Math.Max(0, time - trace.Begin.Time);
                    trace.FrameCount = System.Math.Max(1, frame - trace.Begin.Frame + 1);
                    m_States[i] = new GcKeyState(state.Down, true, state.Up, state.Cancelled, trace.Duration);
                    if (!state.Down)
                    {
                        trace.Current = new GcKeyEvent((Key)i, GcKeyEventPhase.Hold, frame, time);
                        AddEvent(trace.Current); m_KeyEventListOnlyHold.Add(trace.Current); m_KeyTraceListOnlyHold.Add(trace);
                    }
                    m_KeyTraceDict[i] = trace;
                }
                m_Traces[i] = trace;
                m_KeyTraceList.Add(trace);
            }
        }
        void AddEvent(GcKeyEvent e)
        {
            m_KeyCodeToKeyEventIndex[(int)e.Key] = m_KeyEventList.Length;
            m_KeyEventList.Add(e);
        }
        #endregion
    }
}
