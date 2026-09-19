#nullable enable
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace GameCanvas.Engine
{
    sealed class GcInputPointerEngine : IInputPointer, IEngine
    {
        readonly GcContext m_Context;
        readonly GcPointerSource m_Source = new();
        readonly GcPointerTracker m_Tracker = new();
        bool m_Paused, m_Focused = true;
        public bool IsTouchPressureSupported => IsTouchSupported && Input.touchPressureSupported;
        public bool IsTouchSupported => Touchscreen.current != null;
        public GcPointer Pointer => m_Tracker.Pointer;
        public GcReadOnlyList<GcPointer> Pointers => m_Tracker.Pointers;
        public GcReadOnlyList<GcPointerEvent> PointerEvents => m_Tracker.Events;
        public GcReadOnlyList<GcPoint> Taps => m_Tracker.Taps;
        public GcTapSettings TapSettings { get => m_Tracker.TapSettings; set => m_Tracker.TapSettings = value; }
        internal void SetPaused(bool paused) { m_Paused = paused; m_Source.SetSuspended(m_Paused || !m_Focused); }
        internal void SetFocused(bool focused) { m_Focused = focused; m_Source.SetSuspended(m_Paused || !m_Focused); }
        internal GcInputPointerEngine(in GcContext context) { m_Context = context; }
        void System.IDisposable.Dispose() => m_Source.Dispose();
        void IEngine.OnAfterDraw() { }
        void IEngine.OnBeforeUpdate(in System.DateTimeOffset now)
        {
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

        }
    }
}
