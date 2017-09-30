/*------------------------------------------------------------*/
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

    public sealed class Accelerometer
    {
        //----------------------------------------------------------
        #region フィールド変数
        //----------------------------------------------------------

        private readonly Vector3[] cEvents;
        private readonly Vector3[] cNormalizedEvents;
        private int mEventCount;
        private Vector3 mLast;

        #endregion

        //----------------------------------------------------------
        #region パブリック関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal Accelerometer()
        {
            cEvents = new Vector3[5];
            cNormalizedEvents = new Vector3[5];
            mLast = Vector3.zero;
        }

        internal void OnBeforeUpdate()
        {
            mLast = Input.acceleration;
            mEventCount = Input.accelerationEventCount;

            for (var i = 0; i < cEvents.Length; ++i)
            {
                if (i >= mEventCount) break;
                cEvents[i] = Input.GetAccelerationEvent(i).acceleration;
                cNormalizedEvents[i] = cEvents[i].normalized;
            }
        }

        public float LastX { get { return mLast.x; } }
        public float LastY { get { return mLast.y; } }
        public float LastZ { get { return mLast.z; } }

        public int EventCount { get { return mEventCount; } }
        public float GetX(ref int i) { return (i >= 0 && i < mEventCount) ? cEvents[i].x : 0f; }
        public float GetY(ref int i) { return (i >= 0 && i < mEventCount) ? cEvents[i].y : 0f; }
        public float GetZ(ref int i) { return (i >= 0 && i < mEventCount) ? cEvents[i].z : 0f; }
        public float GetNormalizedX(ref int i) { return (i >= 0 && i < mEventCount) ? cNormalizedEvents[i].x : 0f; }
        public float GetNormalizedY(ref int i) { return (i >= 0 && i < mEventCount) ? cNormalizedEvents[i].y : 0f; }
        public float GetNormalizedZ(ref int i) { return (i >= 0 && i < mEventCount) ? cNormalizedEvents[i].z : 0f; }

        #endregion
    }
}
