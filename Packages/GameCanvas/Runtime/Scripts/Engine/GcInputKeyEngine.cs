/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using Unity.Collections;
using UnityEngine;
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
        static readonly KeyCode[] k_KeyCodeArray = (KeyCode[])System.Enum.GetValues(typeof(KeyCode));

        readonly GcContext m_Context;
        NativeHashMap<int, int> m_KeyCodeToKeyEventIndex;
        NativeList<GcKeyEvent> m_KeyEventList;
        NativeList<GcKeyEvent> m_KeyEventListOnlyDown;
        NativeList<GcKeyEvent> m_KeyEventListOnlyHold;
        NativeList<GcKeyEvent> m_KeyEventListOnlyUp;
        NativeArray<GcKeyTrace> m_KeyTraceArray;
        NativeHashMap<int, GcKeyTrace> m_KeyTraceDict;
        NativeList<GcKeyTrace> m_KeyTraceListOnlyHold;
        NativeList<GcKeyTrace> m_KeyTraceListOnlyUp;
        TouchScreenKeyboard m_ScreenKeyboard;
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

        public bool IsKeyDown(in KeyCode key)
            => m_KeyCodeToKeyEventIndex.TryGetValue((int)key, out var index)
            && (m_KeyEventList[index].Phase == GcKeyEventPhase.Down);

        public bool IsKeyHold(in KeyCode key)
            => m_KeyCodeToKeyEventIndex.TryGetValue((int)key, out var index)
            && (m_KeyEventList[index].Phase == GcKeyEventPhase.Hold);

        public bool IsKeyPress(in KeyCode key)
        {
            if (m_KeyCodeToKeyEventIndex.TryGetValue((int)key, out var index))
            {
                var phase = m_KeyEventList[index].Phase;
                return (phase == GcKeyEventPhase.Down || phase == GcKeyEventPhase.Hold);
            }
            return false;
        }

        public bool IsKeyUp(in KeyCode key)
            => m_KeyCodeToKeyEventIndex.TryGetValue((int)key, out var index)
            && (m_KeyEventList[index].Phase == GcKeyEventPhase.Up);

        public bool ShowScreenKeyboard()
        {
            if (!k_IsScreenKeyboardSupported) return false;

            TouchScreenKeyboard.hideInput = true;
            m_ScreenKeyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.Default, false, false, false, false, "", 0);
            return m_ScreenKeyboard.active;
        }

        public bool TryGetKeyEvent(in KeyCode key, out GcKeyEvent e)
        {
            if (m_KeyCodeToKeyEventIndex.TryGetValue((int)key, out var index))
            {
                e = m_KeyEventList[index];
                return true;
            }
            e = default;
            return false;
        }

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

        public bool TryGetKeyTrace(in KeyCode key, out GcKeyTrace trace)
            => m_KeyTraceDict.TryGetValue((int)key, out trace);

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
            var time = m_Context.Time.TimeSinceStartup;

            if (m_KeyTraceDict.Count() != 0)
            {
                using (var keyCodeArray = m_KeyTraceDict.GetKeyArray(Allocator.Temp))
                {
                    for (int i = 0, len = keyCodeArray.Length; i != len; i++)
                    {
                        var key = keyCodeArray[i];
                        var t = m_KeyTraceDict[key];

                        if (Input.GetKey((KeyCode)key))
                        {
                            var e = new GcKeyEvent((KeyCode)key, GcKeyEventPhase.Hold, frame, time);
                            m_KeyCodeToKeyEventIndex.Add(key, m_KeyEventList.Length);
                            m_KeyEventList.Add(e);
                            m_KeyEventListOnlyHold.Add(e);

                            t.Current = e;
                            t.FrameCount = t.Begin.Frame - frame + 1;
                            t.Duration = t.Begin.Time - time;
                            m_KeyTraceDict[key] = t;
                            m_KeyTraceListOnlyHold.Add(t);
                        }
                        else
                        {
                            if (!Input.GetKeyUp((KeyCode)key))
                            {
                                Debug.LogWarning($"[{nameof(GcInputKeyEngine)}] dropped keyup event: {(KeyCode)key}.\n");
                            }
                            var e = new GcKeyEvent((KeyCode)key, GcKeyEventPhase.Up, frame, time);
                            m_KeyCodeToKeyEventIndex.Add(key, m_KeyEventList.Length);
                            m_KeyEventList.Add(e);
                            m_KeyEventListOnlyUp.Add(e);

                            t.Current = e;
                            t.FrameCount = t.Begin.Frame - frame + 1;
                            t.Duration = t.Begin.Time - time;
                            m_KeyTraceDict.Remove(key);
                            m_KeyTraceListOnlyUp.Add(t);
                        }
                    }
                }
            }

            if (Input.anyKeyDown)
            {
                foreach (var key in k_KeyCodeArray)
                {
                    if (Input.GetKeyDown(key))
                    {
                        var e = new GcKeyEvent(key, GcKeyEventPhase.Down, frame, time);
                        if (m_KeyCodeToKeyEventIndex.TryAdd((int)key, m_KeyEventList.Length))
                        {
                            m_KeyEventList.Add(e);
                            m_KeyEventListOnlyDown.Add(e);
                            m_KeyTraceDict.TryAdd((int)key, new GcKeyTrace(e));
                        }
                    }
                }
            }

            m_KeyTraceArray = m_KeyTraceDict.GetValueArray(Allocator.Temp);

            if (m_ScreenKeyboard != null)
            {
                if (m_ScreenKeyboard.status != TouchScreenKeyboard.Status.Visible)
                {
                    m_ScreenKeyboard.active = false;
                    m_ScreenKeyboard = null;
                }
            }
        }
        #endregion
    }
}
