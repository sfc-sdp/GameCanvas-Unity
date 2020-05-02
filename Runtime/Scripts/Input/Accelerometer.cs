/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using UnityEngine;

namespace GameCanvas.Input
{
    public sealed class Accelerometer
    {
#if !GC_DISABLE_ACCELEROMETER
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
            mLast = UnityEngine.Input.acceleration;
            mEventCount = UnityEngine.Input.accelerationEventCount;

            for (var i = 0; i < cEvents.Length; ++i)
            {
                if (i >= mEventCount) break;
                cEvents[i] = UnityEngine.Input.GetAccelerationEvent(i).acceleration;
                cNormalizedEvents[i] = cEvents[i].normalized;
            }
        }

        public float LastX => mLast.x;
        public float LastY => mLast.y;
        public float LastZ => mLast.z;

        public int EventCount => mEventCount;
        public float GetX(ref int i) => (i >= 0 && i < mEventCount) ? cEvents[i].x : 0f;
        public float GetY(ref int i) => (i >= 0 && i < mEventCount) ? cEvents[i].y : 0f;
        public float GetZ(ref int i) => (i >= 0 && i < mEventCount) ? cEvents[i].z : 0f;
        public float GetNormalizedX(ref int i) => (i >= 0 && i < mEventCount) ? cNormalizedEvents[i].x : 0f;
        public float GetNormalizedY(ref int i) => (i >= 0 && i < mEventCount) ? cNormalizedEvents[i].y : 0f;
        public float GetNormalizedZ(ref int i) => (i >= 0 && i < mEventCount) ? cNormalizedEvents[i].z : 0f;

        #endregion
#else
        internal void OnBeforeUpdate() { }
#endif //!GC_DISABLE_ACCELEROMETER
    }
}
