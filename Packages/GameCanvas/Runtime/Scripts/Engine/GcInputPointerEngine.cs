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
using Unity.Mathematics;
using UnityEngine;

namespace GameCanvas.Engine
{
    sealed class GcInputPointerEngine : IInputPointer, IEngine
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        static readonly int[] k_IdMouseButton = new[] { 1, 2, 3 };
        static readonly bool k_IsTouchSupported = Input.touchSupported && (Application.platform != RuntimePlatform.WindowsEditor);
        static readonly bool k_IsTouchPressureSupported = k_IsTouchSupported && Input.touchPressureSupported;
        static readonly int k_EventNumMax = (k_IsTouchSupported && Input.multiTouchEnabled) ? 10 : k_IdMouseButton.Length;

        readonly GcContext m_Context;
        GcPointerEvent m_LastPointer;
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
            if (k_IsTouchSupported)
            {
                var count = Input.touchCount;
                m_PointerList = new NativeList<GcPointerEvent>(count, Allocator.Temp);
                for (var i = 0; i < count; i++)
                {
                    m_PointerList.Add(new GcPointerEvent(m_Context, Input.GetTouch(i)));
                }
            }
            else
            {
                m_PointerList = new NativeList<GcPointerEvent>(k_IdMouseButton.Length, Allocator.Temp);
                var point = Input.mousePosition;

                for (var i = 0; i < k_IdMouseButton.Length; i++)
                {
                    var on = Input.GetMouseButton(i);
                    var up = Input.GetMouseButtonUp(i);
                    if (!on && !up) continue;

                    var id = k_IdMouseButton[i];
                    var screen = new float2(point.x, point.y);
                    var phase = up ? GcPointerEventPhase.End
                        : m_PointerTraceDict.ContainsKey(id) ? GcPointerEventPhase.Hold
                        : GcPointerEventPhase.Begin;
                    m_PointerList.Add(new GcPointerEvent(m_Context, id, screen, phase, GcPointerType.Others));
                }
            }

            m_PointerListBegin = new NativeList<GcPointerEvent>(k_EventNumMax, Allocator.Temp);
            m_PointerListHold = new NativeList<GcPointerEvent>(k_EventNumMax, Allocator.Temp);
            m_PointerListEnd = new NativeList<GcPointerEvent>(k_EventNumMax, Allocator.Temp);
            m_PointerTraceList = new NativeList<GcPointerTrace>(k_EventNumMax, Allocator.Temp);
            m_PointerTraceListHold = new NativeList<GcPointerTrace>(k_EventNumMax, Allocator.Temp);
            m_PointerTraceListEnd = new NativeList<GcPointerTrace>(k_EventNumMax, Allocator.Temp);
            m_TapPointList = new NativeList<float2>(k_EventNumMax, Allocator.Temp);

            if (m_PointerList.Length != 0)
            {
                // イベント時刻でソート
                m_PointerList.Sort();
                m_LastPointer = m_PointerList[m_PointerList.Length - 1];

                for (var i = 0; i < m_PointerList.Length; i++)
                {
                    var pointer = m_PointerList[i];

                    switch (pointer.Phase)
                    {
                        case GcPointerEventPhase.Begin:
                            m_PointerListBegin.Add(pointer);
                            var trace = new GcPointerTrace(pointer);
                            m_PointerTraceDict.Add(pointer.Id, trace);
                            m_PointerTraceList.Add(trace);
                            continue;

                        case GcPointerEventPhase.Hold:
                            m_PointerListHold.Add(pointer);
                            break;

                        case GcPointerEventPhase.End:
                            m_PointerListEnd.Add(pointer);
                            break;
                    }

                    if (m_PointerTraceDict.TryGetValue(pointer.Id, out var history))
                    {
                        var prev = history.Current;
                        history.Current = pointer;
                        history.FrameCount = pointer.Frame - history.Begin.Frame + 1;
                        history.Duration = pointer.Time - history.Begin.Time;
                        history.Distance += math.distance(pointer.Point, prev.Point);
                        m_PointerTraceList.Add(history);

                        switch (pointer.Phase)
                        {
                            case GcPointerEventPhase.Hold:
                                m_PointerTraceListHold.Add(history);
                                m_PointerTraceDict[pointer.Id] = history;
                                break;

                            case GcPointerEventPhase.End:
                                m_PointerTraceListEnd.Add(history);
                                m_PointerTraceDict.Remove(pointer.Id);

                                if (m_TapSettings.IsTap(history))
                                {
                                    m_TapPointList.Add(history.Begin.Point);
                                }
                                break;
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[{nameof(GcInputPointerEngine)}] {pointer.Id} is unknown pointer. ({pointer.Id}, {pointer.Phase})\n");
                    }
                }
            }
        }
        #endregion
    }
}
