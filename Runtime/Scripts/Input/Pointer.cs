/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using GameCanvas.Engine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace GameCanvas.Input
{
    public sealed class Pointer
    {
        //----------------------------------------------------------
        #region フィールド変数
        //----------------------------------------------------------

        private const int k_EventNumMax = 10;
        private readonly int[] k_IdMouseButton = new[] { 1, 2, 3 };

        private readonly Engine.Time Time;
        private readonly Graphic Graphic;
        private readonly bool IsTouchSupported;
        private readonly bool IsTouchPressureSupported;
        private readonly PointerEvent[] CurrEvents;
        private readonly PointerEvent[] PrevEvents;
        private readonly Dictionary<int, PointerEvent> BeganEventDict;
        private readonly Dictionary<int, PointerEvent> PrevEventDict;
        private readonly int[] EventFrameCounts;
        private readonly float[] EventDurations;
        private readonly float[] EventDistances;

        private int m_EventNum = 0;
        private bool m_IsTapped = false;
        private Vector2Int m_LastPoint;
        private Vector2Int m_LastTappedPoint;
        private float m_PressureMax = 1f;
        private float m_TapDurationMax = 0.125f;
        private float m_TapDistanceMax = 25f;

        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal Pointer(Engine.Time time, Graphic graphic)
        {
            Assert.IsNotNull(time);
            Assert.IsNotNull(graphic);
            Time = time;
            Graphic = graphic;

            IsTouchSupported = UnityEngine.Input.touchSupported && (Application.platform != RuntimePlatform.WindowsEditor);
            IsTouchPressureSupported = IsTouchPressureSupported && UnityEngine.Input.touchPressureSupported;

            CurrEvents = new PointerEvent[k_EventNumMax];
            PrevEvents = new PointerEvent[k_EventNumMax];
            BeganEventDict = new Dictionary<int, PointerEvent>(k_EventNumMax);
            PrevEventDict = new Dictionary<int, PointerEvent>(k_EventNumMax);
            EventFrameCounts = new int[k_EventNumMax];
            EventDurations = new float[k_EventNumMax];
            EventDistances = new float[k_EventNumMax];
        }

        public void OnBeforeUpdate()
        {
            m_IsTapped = false;
            PrevEventDict.Clear();
            for (var i = 0; i < m_EventNum; i++)
            {
                PrevEvents[i] = CurrEvents[i];
                PrevEventDict.Add(CurrEvents[i].Id, CurrEvents[i]);
            }
            
            if (IsTouchSupported)
            {
                m_EventNum = Mathf.Min(UnityEngine.Input.touchCount, k_EventNumMax);
                for (var i = 0; i < m_EventNum; ++i)
                {
                    var touch = UnityEngine.Input.GetTouch(i);
                    CurrEvents[i] = new PointerEvent(Time, Graphic, touch);
                    if (IsTouchPressureSupported)
                    {
                        m_PressureMax = Mathf.Max(m_PressureMax, touch.maximumPossiblePressure);
                    }
                }
            }
            else
            {
                m_EventNum = 0;
                Vector2 mousePos = UnityEngine.Input.mousePosition;

                for (var i = 0; i < k_IdMouseButton.Length; i++)
                {
                    var on = UnityEngine.Input.GetMouseButton(i);
                    var up = UnityEngine.Input.GetMouseButtonUp(i);
                    if (up || on)
                    {
                        var id = k_IdMouseButton[i];
                        PointerEvent.EPhase phase;
                        if (up)
                        {
                            phase = PointerEvent.EPhase.Ended;
                        }
                        else if (PrevEventDict.TryGetValue(id, out var prev))
                        {
                            phase = (prev.Phase == PointerEvent.EPhase.Ended) ? PointerEvent.EPhase.Began
                                : (Mathf.Approximately(prev.Screen.x, mousePos.x) && Mathf.Approximately(prev.Screen.y, mousePos.y)) ? PointerEvent.EPhase.Stationary
                                : PointerEvent.EPhase.Moved;
                        }
                        else
                        {
                            phase = PointerEvent.EPhase.Began;
                        }
                        CurrEvents[m_EventNum++] = new PointerEvent(Time, Graphic, id, mousePos, phase, PointerEvent.EType.Others);
                    }
                }
            }

            if (m_EventNum > 0)
            {
                m_LastPoint = new Vector2Int(Mathf.RoundToInt(CurrEvents[0].Canvas.x), Mathf.RoundToInt(CurrEvents[0].Canvas.y));

                var currentFrame = Time.FrameCount;
                var currentTime = Time.SinceStartup;
                for (var i = 0; i < m_EventNum; ++i)
                {
                    var id = CurrEvents[i].Id;

                    if (CurrEvents[i].Phase == PointerEvent.EPhase.Began)
                    {
                        EventFrameCounts[i] = 1;
                        EventDurations[i] = 0f;
                        EventDistances[i] = 0f;
                        BeganEventDict.Add(id, CurrEvents[i]);
                        continue;
                    }

                    if (!BeganEventDict.ContainsKey(id))
                    {
                        Debug.LogWarning($"[{nameof(Pointer)}] {id} is unknown pointer. phase: {CurrEvents[i].Phase}");
                        continue;
                    }

                    EventFrameCounts[i] = currentFrame - BeganEventDict[id].Frame + 1;
                    EventDurations[i] = currentTime - BeganEventDict[id].Time;
                    EventDistances[i] += Vector2.Distance(CurrEvents[i].Screen, PrevEventDict[id].Screen);

                    if (CurrEvents[i].Phase == PointerEvent.EPhase.Ended)
                    {
                        if ((EventDurations[i] < m_TapDurationMax) && (EventDistances[i] < m_TapDistanceMax))
                        {
                            m_IsTapped |= true;
                            m_LastTappedPoint = new Vector2Int(Mathf.RoundToInt(CurrEvents[i].Canvas.x), Mathf.RoundToInt(CurrEvents[i].Canvas.y));
                        }
                        BeganEventDict.Remove(id);
                    }
                }
            }

            for (var i = m_EventNum; i < CurrEvents.Length; i++)
            {
                CurrEvents[i] = default;
            }
        }

        public int Count { get { return m_EventNum; } }
        public bool HasEvent { get { return (m_EventNum > 0); } }
        public bool IsTapped(out int canvasX, out int canvasY)
        {
            canvasX = m_LastTappedPoint.x;
            canvasY = m_LastTappedPoint.y;
            return m_IsTapped;
        }
        public bool IsTapped(in RectInt canvasRect)
        {
            return m_IsTapped && canvasRect.Contains(m_LastTappedPoint);
        }

        public PointerEvent GetRaw(in int i)
        {
            return (i < m_EventNum) ? CurrEvents[i] : default;
        }
        public int GetX(in int i)
        {
            return (i < m_EventNum) ? Mathf.RoundToInt(CurrEvents[i].Canvas.x) : 0;
        }
        public int GetY(in int i)
        {
            return (i < m_EventNum) ? Mathf.RoundToInt(CurrEvents[i].Canvas.y) : 0;
        }
        public bool GetIsBegan(in int i)
        {
            return (i < m_EventNum) && (CurrEvents[i].Phase == PointerEvent.EPhase.Began);
        }
        public bool GetIsEnded(in int i)
        {
            return (i < m_EventNum) && (CurrEvents[i].Phase == PointerEvent.EPhase.Ended);
        }
        public int GetFrameCount(in int i)
        {
            return (i < m_EventNum) ? EventFrameCounts[i] : 0;
        }
        public float GetDulation(in int i)
        {
            return (i < m_EventNum) ? EventDurations[i] : 0f;
        }

        public void SetTapSensitivity(in float maxDuration, in float maxDistance)
        {
            m_TapDurationMax = maxDuration;
            m_TapDistanceMax = maxDistance;
        }

        public int X { get { return (0 < m_EventNum) ? m_LastPoint.x : 0; } }
        public int Y { get { return (0 < m_EventNum) ? m_LastPoint.y : 0; } }
        public bool IsBegan { get { return (0 < m_EventNum) && (CurrEvents[0].Phase == PointerEvent.EPhase.Began); } }
        public bool IsEnded { get { return (0 < m_EventNum) && (CurrEvents[0].Phase == PointerEvent.EPhase.Ended); } }
        public int FrameCount { get { return (0 < m_EventNum) ? EventFrameCounts[0] : 0; } }
        public float Duration { get { return (0 < m_EventNum) ? EventDurations[0] : 0f; } }

        public int LastX { get { return m_LastPoint.x; } }
        public int LastY { get { return m_LastPoint.y; } }

        public float PressureMax { get { return m_PressureMax; } }

        #endregion
    }
}
