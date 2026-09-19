#nullable enable
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace GameCanvas.Engine
{
    sealed class GcInputKeyEngine : IInputKey, IEngine
    {
        static readonly bool k_IsScreenKeyboardSupported = TouchScreenKeyboard.isSupported;
        readonly GcContext m_Context;
        readonly GcKeySource m_Source = new();
        readonly GcKeyState[] m_States = new GcKeyState[GcKeySource.Capacity];
        readonly double[] m_Began = new double[GcKeySource.Capacity];
        readonly List<GcKeyEvent> m_Events = new(32);
        TouchScreenKeyboard? m_ScreenKeyboard;
        bool m_Focused = true, m_Paused;
        public GcKeyState Key(GcKey key) => (uint)key < m_States.Length ? m_States[(int)key] : default;
        public GcReadOnlyList<GcKeyEvent> KeyEvents => new(m_Events);
        internal void SetFocused(bool value) { m_Focused = value; m_Source.SetSuspended(!m_Focused || m_Paused); }
        internal void SetPaused(bool value) { m_Paused = value; m_Source.SetSuspended(!m_Focused || m_Paused); }
        public bool IsScreenKeyboardSupported => k_IsScreenKeyboardSupported;
        public bool IsScreenKeyboardVisible => m_ScreenKeyboard?.status == TouchScreenKeyboard.Status.Visible;
        public void HideScreenKeyboard()
        {
            if (m_ScreenKeyboard != null)
            {
                m_ScreenKeyboard.active = false;
                m_ScreenKeyboard = null;
            }
        }

        public bool ShowScreenKeyboard()
        {
            if (!k_IsScreenKeyboardSupported) return false;

            TouchScreenKeyboard.hideInput = true;
            m_ScreenKeyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, false, false, "", 0);
            return m_ScreenKeyboard.active;
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
        internal GcInputKeyEngine(in GcContext context) { m_Context = context; }
        void IDisposable.Dispose() { HideScreenKeyboard(); m_Source.Dispose(); }
        void IEngine.OnAfterDraw() { }
        void IEngine.OnBeforeUpdate(in DateTimeOffset now)
        {
            m_Events.Clear();
            double inputTime = InputState.currentTime;
            double time = m_Context.Time.TimeSinceStartup;
            int frame = m_Context.Time.CurrentFrame;
            for (int i = 0; i < m_States.Length; i++)
                m_States[i] = new GcKeyState(false, m_States[i].Held, false, false, 0);
            foreach (var record in m_Source.Pending)
            {
                int i = (int)record.Key;
                var previous = m_States[i];
                bool down = record.Phase == GcKeyEventPhase.Down;
                if (down) m_Began[i] = record.Time;
                double duration = Math.Max(0, record.Time - m_Began[i]);
                m_States[i] = new GcKeyState(previous.Down || down, down,
                    previous.Up || record.Phase == GcKeyEventPhase.Up,
                    previous.Cancelled || record.Phase == GcKeyEventPhase.Cancelled, duration);
                m_Events.Add(new GcKeyEvent((GcKey)record.Key, record.Phase, frame,
                    time - Math.Max(0, inputTime - record.Time)));
            }
            m_Source.Pending.Clear();
            for (int i = 0; i < m_States.Length; i++)
            {
                var state = m_States[i];
                if (state.Held)
                    m_States[i] = new GcKeyState(state.Down, true, state.Up, state.Cancelled,
                        Math.Max(0, inputTime - m_Began[i]));
            }
        }
    }
}
