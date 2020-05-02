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

        private const int cEventNumMax = 10;
        private const int cIdMouseButton0 = 1;
        private const int cIdMouseButton1 = 2;
        private const int cIdMouseButton2 = 3;

        private readonly Graphic cGraphic;
        private readonly bool cIsTouchSupported;
        private readonly bool cIsTouchPressureSupported;
        private readonly PointerEvent[] cEvents;
        private readonly PointerEvent[] cPrevEvents;
        private readonly Dictionary<int, PointerEvent> cBeganEventDict;
        private readonly int[] cEventFrameCounts;
        private readonly float[] cEventDulations;

        private int mEventNum = 0;
        private int mLastX = 0;
        private int mLastY = 0;
        private float mPressureMax = 1f;

        #endregion

        //----------------------------------------------------------
        #region パブリック関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal Pointer(Graphic graphic)
        {
            Assert.IsNotNull(graphic);
            cGraphic = graphic;

            cIsTouchSupported = UnityEngine.Input.touchSupported && (Application.platform != RuntimePlatform.WindowsEditor);
            cIsTouchPressureSupported = cIsTouchPressureSupported && UnityEngine.Input.touchPressureSupported;

            cEvents = new PointerEvent[cEventNumMax];
            cPrevEvents = new PointerEvent[cEventNumMax];
            cBeganEventDict = new Dictionary<int, PointerEvent>(cEventNumMax);
            cEventFrameCounts = new int[cEventNumMax];
            cEventDulations = new float[cEventNumMax];
        }

        public void OnBeforeUpdate()
        {
            if (cIsTouchSupported)
            {
                mEventNum = Mathf.Min(UnityEngine.Input.touchCount, cEventNumMax);
                for (var i = 0; i < mEventNum; ++i)
                {
                    var touch = UnityEngine.Input.GetTouch(i);
                    cEvents[i] = new PointerEvent(ref touch);
                    if (cIsTouchPressureSupported)
                    {
                        mPressureMax = Mathf.Max(mPressureMax, touch.maximumPossiblePressure);
                    }
                }
            }
            else
            {
                mEventNum = 0;
                Vector2 mousePos = UnityEngine.Input.mousePosition;

                if (UnityEngine.Input.GetMouseButton(0) || UnityEngine.Input.GetMouseButtonUp(0))
                {
                    var prevEvent = (cPrevEvents[0].Id == cIdMouseButton0)
                        ? (PointerEvent?)cPrevEvents[0]
                        : null;
                    createMouseEvent(out cEvents[mEventNum++], 0, cIdMouseButton0, ref mousePos, ref prevEvent);
                }
                if (UnityEngine.Input.GetMouseButton(1) || UnityEngine.Input.GetMouseButtonUp(1))
                {
                    var prevEvent = (cPrevEvents[0].Id == cIdMouseButton1)
                        ? (PointerEvent?)cPrevEvents[0] : (cPrevEvents[1].Id == cIdMouseButton1)
                        ? (PointerEvent?)cPrevEvents[1]
                        : null;
                    createMouseEvent(out cEvents[mEventNum++], 1, cIdMouseButton1, ref mousePos, ref prevEvent);
                }
                if (UnityEngine.Input.GetMouseButton(2) || UnityEngine.Input.GetMouseButtonUp(2))
                {
                    var prevEvent = (cPrevEvents[0].Id == cIdMouseButton2)
                        ? (PointerEvent?)cPrevEvents[0] : (cPrevEvents[1].Id == cIdMouseButton2)
                        ? (PointerEvent?)cPrevEvents[1] : (cPrevEvents[2].Id == cIdMouseButton2)
                        ? (PointerEvent?)cPrevEvents[2]
                        : null;
                    createMouseEvent(out cEvents[mEventNum++], 2, cIdMouseButton2, ref mousePos, ref prevEvent);
                }
            }

            if (mEventNum > 0)
            {
                mLastX = cGraphic.ScreenToCanvasX(cEvents[0].ScreenX);
                mLastY = cGraphic.ScreenToCanvasY(cEvents[0].ScreenY);

                var currentFrame = UnityEngine.Time.frameCount;
                var currentTime = UnityEngine.Time.unscaledTime;
                for (var i = 0; i < mEventNum; ++i)
                {
                    var id = cEvents[i].Id;

                    if (cEvents[i].Phase == PointerEvent.EPhase.Began)
                    {
                        cEventFrameCounts[i] = 1;
                        cEventDulations[i] = 0;
                        cBeganEventDict.Add(id, cEvents[i]);
                        continue;
                    }

                    if (!cBeganEventDict.ContainsKey(id))
                    {
                        Debug.LogWarningFormat("[Pointer] {0} is unknown pointer. phase: {1}", id, cEvents[i].Phase);
                        continue;
                    }

                    cEventFrameCounts[i] = currentFrame - cBeganEventDict[id].Frame + 1;
                    cEventDulations[i] = currentTime - cBeganEventDict[id].Time;

                    if (cEvents[i].Phase == PointerEvent.EPhase.Ended)
                    {
                        cBeganEventDict.Remove(id);
                    }
                }
            }

            for (var i = mEventNum; i < cEventNumMax; ++i)
            {
                cEvents[i] = default(PointerEvent);
            }
            for (var i = 0; i < cEvents.Length; ++i)
            {
                cPrevEvents[i] = cEvents[i];
            }
        }

        public int Count { get { return mEventNum; } }
        public bool HasEvent { get { return (mEventNum > 0); } }

        public PointerEvent GetRaw(ref int i)
        {
            return (i < mEventNum) ? cEvents[i] : default(PointerEvent);
        }
        public int GetX(ref int i)
        {
            return (i < mEventNum) ? cGraphic.ScreenToCanvasX(cEvents[i].ScreenX) : 0;
        }
        public int GetY(ref int i)
        {
            return (i < mEventNum) ? cGraphic.ScreenToCanvasY(cEvents[i].ScreenY) : 0;
        }
        public bool GetIsBegan(ref int i)
        {
            return (i < mEventNum) ? (cEvents[i].Phase == PointerEvent.EPhase.Began) : false;
        }
        public bool GetIsEnded(ref int i)
        {
            return (i < mEventNum) ? (cEvents[i].Phase == PointerEvent.EPhase.Ended) : false;
        }
        public int GetFrameCount(ref int i)
        {
            return (i < mEventNum) ? cEventFrameCounts[i] : 0;
        }
        public float GetDulation(ref int i)
        {
            return (i < mEventNum) ? cEventDulations[i] : 0f;
        }

        public int X { get { return HasEvent ? cGraphic.ScreenToCanvasX(cEvents[0].ScreenX) : 0; } }
        public int Y { get { return HasEvent ? cGraphic.ScreenToCanvasY(cEvents[0].ScreenY) : 0; } }
        public bool IsBegan { get { return (mEventNum > 0 && cEvents[0].Phase == PointerEvent.EPhase.Began); } }
        public bool IsEnded { get { return (mEventNum > 0 && cEvents[0].Phase == PointerEvent.EPhase.Ended); } }
        public int FrameCount { get { return HasEvent ? cEventFrameCounts[0] : 0; } }
        public float Duration { get { return HasEvent ? cEventDulations[0] : 0f; } }

        public int LastX { get { return mLastX; } }
        public int LastY { get { return mLastY; } }

        public float PressureMax { get { return mPressureMax; } }

        #endregion

        //----------------------------------------------------------
        #region プライベート関数
        //----------------------------------------------------------

        private static void createMouseEvent(out PointerEvent e, int button, int id, ref Vector2 mousePos, ref PointerEvent? prev)
        {
            var phase = PointerEvent.EPhase.Began;
            if (prev.HasValue)
            {
                var v = prev.Value;
                phase = (v.Phase == PointerEvent.EPhase.Ended)
                    ? PointerEvent.EPhase.Began : (Mathf.Approximately(v.ScreenX, mousePos.x) && Mathf.Approximately(v.ScreenY, mousePos.y))
                    ? PointerEvent.EPhase.Stationary
                    : PointerEvent.EPhase.Moved;
            }
            if (UnityEngine.Input.GetMouseButtonUp(button)) phase = PointerEvent.EPhase.Ended;
            e = new PointerEvent(id, (int)mousePos.x, (int)mousePos.y, phase, PointerEvent.EType.Others);
        }

        #endregion
    }
}
