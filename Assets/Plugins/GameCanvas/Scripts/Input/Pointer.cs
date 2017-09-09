﻿/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2017 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/

namespace GameCanvas.Input
{
    using System.Collections.Generic;
    using UnityEngine;

    public sealed class Pointer
    {
        //----------------------------------------------------------
        #region フィールド変数
        //----------------------------------------------------------

        private const int cEventNumMax = 10;
        private const int cIdMouseButton0 = 1;
        private const int cIdMouseButton1 = 2;
        private const int cIdMouseButton2 = 3;

        private readonly bool cIsTouchSupported;
        private readonly bool cIsTouchPressureSupported;
        private readonly PointerEvent[] cEvents;
        private readonly PointerEvent[] cPrevEvents;
        private readonly Dictionary<int, PointerEvent> cBeganEventDict;
        private int[] cEventFrameCounts;
        private float[] cEventDulations;

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
        public Pointer()
        {
            cIsTouchSupported = Input.touchSupported && (Application.platform != RuntimePlatform.WindowsEditor);
            cIsTouchPressureSupported = cIsTouchPressureSupported && Input.touchPressureSupported;

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
                mEventNum = Mathf.Min(Input.touchCount, cEventNumMax);
                for (var i = 0; i < mEventNum; ++i)
                {
                    var touch = Input.GetTouch(i);
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
                Vector2 mousePos = Input.mousePosition;

                if (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0))
                {
                    var prevEvent
                        = cPrevEvents[0].Id == cIdMouseButton0 ? (PointerEvent?)cPrevEvents[0]
                        : null;
                    createMouseEvent(out cEvents[mEventNum++], 0, cIdMouseButton0, ref mousePos, ref prevEvent);
                }
                if (Input.GetMouseButton(1) || Input.GetMouseButtonUp(1))
                {
                    var prevEvent
                        = cPrevEvents[0].Id == cIdMouseButton1 ? (PointerEvent?)cPrevEvents[0]
                        : cPrevEvents[1].Id == cIdMouseButton1 ? (PointerEvent?)cPrevEvents[1]
                        : null;
                    createMouseEvent(out cEvents[mEventNum++], 1, cIdMouseButton1, ref mousePos, ref prevEvent);
                }
                if (Input.GetMouseButton(2) || Input.GetMouseButtonUp(2))
                {
                    var prevEvent
                        = cPrevEvents[0].Id == cIdMouseButton2 ? (PointerEvent?)cPrevEvents[0]
                        : cPrevEvents[1].Id == cIdMouseButton2 ? (PointerEvent?)cPrevEvents[1]
                        : cPrevEvents[2].Id == cIdMouseButton2 ? (PointerEvent?)cPrevEvents[2]
                        : null;
                    createMouseEvent(out cEvents[mEventNum++], 2, cIdMouseButton2, ref mousePos, ref prevEvent);
                }
            }

            if (mEventNum > 0)
            {
                mLastX = cEvents[0].X;
                mLastY = cEvents[0].Y;

                var currentFrame = Time.frameCount;
                var currentTime = Time.unscaledTime;
                for (var i = 0; i < mEventNum; ++i)
                {
                    var id = cEvents[i].Id;

                    if (cEvents[i].Phase == PointerEvent.EPhase.Began)
                    {
                        cEventFrameCounts[i] = 0;
                        cEventDulations[i] = 0;
                        cBeganEventDict.Add(id, cEvents[i]);
                        continue;
                    }

                    cEventFrameCounts[i] = currentFrame - cBeganEventDict[id].Frame;
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

        public int Count => mEventNum;
        public bool HasEvent => (mEventNum > 0);
        public PointerEvent GetRaw(int i) => (i < mEventNum) ? cEvents[i] : default(PointerEvent);
        public int GetX(int i) => (i < mEventNum) ? cEvents[i].X : 0;
        public int GetY(int i) => (i < mEventNum) ? cEvents[i].Y : 0;
        public bool GetIsBegan(int i) => (i < mEventNum) ? (cEvents[i].Phase == PointerEvent.EPhase.Began) : false;
        public bool GetIsEnded(int i) => (i < mEventNum) ? (cEvents[i].Phase == PointerEvent.EPhase.Ended) : false;
        public int GetFrameCount(int i) => (i < mEventNum) ? cEventFrameCounts[i] : 0;
        public float GetDulation(int i) => (i < mEventNum) ? cEventDulations[i] : 0f;

        public int X => HasEvent ? cEvents[0].X : 0;
        public int Y => HasEvent ? cEvents[0].Y : 0;
        public bool IsBegan => (mEventNum > 0 && cEvents[0].Phase == PointerEvent.EPhase.Began);
        public bool IsEnded => (mEventNum > 0 && cEvents[0].Phase == PointerEvent.EPhase.Ended);
        public int FrameCount => HasEvent ? cEventFrameCounts[0] : 0;
        public float Duration => HasEvent ? cEventDulations[0] : 0f;

        public int LastX => mLastX;
        public int LastY => mLastY;

        public float PressureMax => mPressureMax;

        #endregion

        //----------------------------------------------------------
        #region プライベート関数
        //----------------------------------------------------------

        private static void createMouseEvent(out PointerEvent e, int button, int id, ref Vector2 mousePos, ref PointerEvent? prev)
        {
            var phase = PointerEvent.EPhase.Began;
            if (prev.HasValue)
            {
                phase = (prev.Value.X == mousePos.x && prev.Value.Y == mousePos.y)
                    ? PointerEvent.EPhase.Stationary
                    : PointerEvent.EPhase.Moved;
            }
            if (Input.GetMouseButtonUp(button)) phase = PointerEvent.EPhase.Ended;
            e = new PointerEvent(id, (int)mousePos.x, (int)mousePos.y, phase, PointerEvent.EType.Others);
        }

        #endregion
    }
}