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

        const int k_EventNumMax = 10;
        static readonly bool k_IsScreenKeyboardSupported = TouchScreenKeyboard.isSupported;

        readonly GcContext m_Context;
        readonly InputStateHistory m_History;
        NativeHashMap<int, int> m_KeyCodeToKeyEventIndex;
        NativeList<GcKeyEvent> m_KeyEventList;
        NativeList<GcKeyEvent> m_KeyEventListOnlyDown;
        NativeList<GcKeyEvent> m_KeyEventListOnlyHold;
        NativeList<GcKeyEvent> m_KeyEventListOnlyUp;
        NativeArray<GcKeyTrace> m_KeyTraceArray;
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

        public bool IsKeyDown(in Key key)
            => m_KeyCodeToKeyEventIndex.TryGetValue((int)key, out var index)
            && (m_KeyEventList[index].Phase == GcKeyEventPhase.Down);

        public bool IsKeyHold(in Key key)
            => m_KeyCodeToKeyEventIndex.TryGetValue((int)key, out var index)
            && (m_KeyEventList[index].Phase == GcKeyEventPhase.Hold);

        public bool IsKeyPress(in Key key)
        {
            if (m_KeyCodeToKeyEventIndex.TryGetValue((int)key, out var index))
            {
                var phase = m_KeyEventList[index].Phase;
                return (phase == GcKeyEventPhase.Down || phase == GcKeyEventPhase.Hold);
            }
            return false;
        }

        public bool IsKeyUp(in Key key)
            => m_KeyCodeToKeyEventIndex.TryGetValue((int)key, out var index)
            && (m_KeyEventList[index].Phase == GcKeyEventPhase.Up);

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

        [System.Obsolete]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool TryGetKeyEventArray(out NativeArray<GcKeyEvent>.ReadOnly array, out int count)
        {
            count = m_KeyEventList.Length;
            if (count != 0)
            {
                array = m_KeyEventList.AsArray().AsReadOnly();
                return true;
            }
            array = default;
            return false;
        }

        [System.Obsolete]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool TryGetKeyEventArray(in GcKeyEventPhase phase, out NativeArray<GcKeyEvent>.ReadOnly array, out int count)
        {
            switch (phase)
            {
                case GcKeyEventPhase.Down:
                    count = m_KeyEventListOnlyDown.Length;
                    if (count != 0)
                    {
                        array = m_KeyEventListOnlyDown.AsArray().AsReadOnly();
                        return true;
                    }
                    break;

                case GcKeyEventPhase.Hold:
                    count = m_KeyEventListOnlyHold.Length;
                    if (count != 0)
                    {
                        array = m_KeyEventListOnlyHold.AsArray().AsReadOnly();
                        return true;
                    }
                    break;

                case GcKeyEventPhase.Up:
                    count = m_KeyEventListOnlyUp.Length;
                    if (count != 0)
                    {
                        array = m_KeyEventListOnlyUp.AsArray().AsReadOnly();
                        return true;
                    }
                    break;
            }
            count = 0;
            array = default;
            return false;
        }

        public bool TryGetKeyTrace(in Key key, out GcKeyTrace trace)
            => m_KeyTraceDict.TryGetValue((int)key, out trace);

        public bool TryGetKeyTraceAll(out System.ReadOnlySpan<GcKeyTrace> traces)
        {
            traces = m_KeyTraceArray.AsReadOnlySpan();
            return (m_KeyTraceArray.Length != 0);
        }

        public bool TryGetKeyTraceAll(in GcKeyEventPhase phase, out System.ReadOnlySpan<GcKeyTrace> traces)
        {
            switch (phase)
            {
                case GcKeyEventPhase.Hold:
                    traces = m_KeyTraceListOnlyHold.AsReadOnlySpan();
                    return (m_KeyTraceListOnlyHold.Length != 0);

                case GcKeyEventPhase.Up:
                    traces = m_KeyTraceListOnlyHold.AsReadOnlySpan();
                    return (m_KeyTraceListOnlyUp.Length != 0);
            }
            traces = System.ReadOnlySpan<GcKeyTrace>.Empty;
            return false;
        }

        [System.Obsolete]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool TryGetKeyTraceArray(out NativeArray<GcKeyTrace>.ReadOnly array, out int count)
        {
            count = m_KeyTraceArray.Length;
            if (count != 0)
            {
                array = m_KeyTraceArray.AsReadOnly();
                return true;
            }
            array = default;
            return false;
        }

        [System.Obsolete]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool TryGetKeyTraceArray(in GcKeyEventPhase phase, out NativeArray<GcKeyTrace>.ReadOnly array, out int count)
        {
            switch (phase)
            {
                case GcKeyEventPhase.Hold:
                    count = m_KeyTraceListOnlyHold.Length;
                    if (count != 0)
                    {
                        array = m_KeyTraceListOnlyHold.AsArray().AsReadOnly();
                        return true;
                    }
                    break;

                case GcKeyEventPhase.Up:
                    count = m_KeyTraceListOnlyUp.Length;
                    if (count != 0)
                    {
                        array = m_KeyTraceListOnlyUp.AsArray().AsReadOnly();
                        return true;
                    }
                    break;
            }
            count = 0;
            array = default;
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

            //InputSystem.onEvent += (ptr, dev) =>
            //{
            //    if (ptr.IsA<TextEvent>())
            //    {
            //        unsafe
            //        {
            //            var data = (TextEvent*)ptr.data;
            //            Debug.Log($"{ptr.time}: {(char)data->character}");
            //        }
            //    }
            //};

            m_History = new InputStateHistory(Keyboard.current.allKeys);
            m_History.StartRecording();
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
            if (m_KeyTraceArray.IsCreated) m_KeyTraceArray.Dispose();
            if (m_KeyCodeToKeyEventIndex.IsCreated) m_KeyCodeToKeyEventIndex.Dispose();

            if (m_KeyTraceDict.IsCreated) m_KeyTraceDict.Dispose();

            m_History.Dispose();
        }

        void IEngine.OnAfterDraw()
        {
            if (m_KeyEventList.IsCreated) m_KeyEventList.Dispose();
            if (m_KeyEventListOnlyDown.IsCreated) m_KeyEventListOnlyDown.Dispose();
            if (m_KeyEventListOnlyHold.IsCreated) m_KeyEventListOnlyHold.Dispose();
            if (m_KeyEventListOnlyUp.IsCreated) m_KeyEventListOnlyUp.Dispose();
            if (m_KeyTraceListOnlyHold.IsCreated) m_KeyTraceListOnlyHold.Dispose();
            if (m_KeyTraceListOnlyUp.IsCreated) m_KeyTraceListOnlyUp.Dispose();
            if (m_KeyTraceArray.IsCreated) m_KeyTraceArray.Dispose();
            if (m_KeyCodeToKeyEventIndex.IsCreated) m_KeyCodeToKeyEventIndex.Dispose();
        }

        void IEngine.OnBeforeUpdate(in System.DateTimeOffset now)
        {
            m_KeyCodeToKeyEventIndex = new NativeHashMap<int, int>(k_EventNumMax, Allocator.Temp);
            m_KeyEventList = new NativeList<GcKeyEvent>(k_EventNumMax, Allocator.Temp);
            m_KeyEventListOnlyDown = new NativeList<GcKeyEvent>(k_EventNumMax, Allocator.Temp);
            m_KeyEventListOnlyHold = new NativeList<GcKeyEvent>(k_EventNumMax, Allocator.Temp);
            m_KeyEventListOnlyUp = new NativeList<GcKeyEvent>(k_EventNumMax, Allocator.Temp);
            m_KeyTraceListOnlyHold = new NativeList<GcKeyTrace>(k_EventNumMax, Allocator.Temp);
            m_KeyTraceListOnlyUp = new NativeList<GcKeyTrace>(k_EventNumMax, Allocator.Temp);

            var frame = m_Context.Time.CurrentFrame;

            foreach (var record in m_History)
            {
                var control = (KeyControl)record.control;
                var key = control.keyCode;
                var time = (float)record.time;

                if (control.isPressed)
                {
                    // Down Event
                    var e = new GcKeyEvent(key, GcKeyEventPhase.Down, frame, time);
                    AddKeyEvent(e);
                    m_KeyEventListOnlyDown.Add(e);
                    m_KeyTraceDict.Add((int)key, new GcKeyTrace(e));
                }
                else if (m_KeyTraceDict.TryGetValue((int)key, out var t))
                {
                    // Up Event
                    var e = new GcKeyEvent(key, GcKeyEventPhase.Up, frame, time);
                    AddKeyEvent(e);
                    m_KeyEventListOnlyUp.Add(e);

                    t.Current = e;
                    t.FrameCount = t.Begin.Frame - frame + 1;
                    t.Duration = t.Begin.Time - time;
                    m_KeyTraceDict.Remove((int)key);
                    m_KeyTraceListOnlyUp.Add(t);
                }
                else
                {
                    Debug.LogWarning($"[{nameof(GcInputKeyEngine)}] missed '{key}' press event.\n");
                }
            }
            m_History.Clear();

            using (var activeArray = m_KeyTraceDict.GetValueArray(Allocator.Temp))
            {
                for (var i = 0; i < activeArray.Length; i++)
                {
                    var t = activeArray[i];
                    if (t.Begin.Frame == frame) continue;

                    // Hold Event
                    var key = t.Begin.Key;
                    var time = m_Context.Time.TimeSinceStartup;
                    var e = new GcKeyEvent(key, GcKeyEventPhase.Hold, frame, time);
                    AddKeyEvent(e);
                    m_KeyEventListOnlyHold.Add(e);

                    t.Current = e;
                    t.FrameCount = t.Begin.Frame - frame + 1;
                    t.Duration = t.Begin.Time - time;
                    m_KeyTraceDict[(int)key] = t;
                    m_KeyTraceListOnlyHold.Add(t);
                }
            }

            void AddKeyEvent(GcKeyEvent e)
            {
                m_KeyCodeToKeyEventIndex.Add((int)e.Key, m_KeyEventList.Length);
                m_KeyEventList.Add(e);
            }
        }
        #endregion
    }
}
