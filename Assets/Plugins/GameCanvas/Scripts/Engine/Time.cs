/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2017 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/

namespace GameCanvas.Engine
{
    using UnityEngine;

    public sealed class Time
    {
        //----------------------------------------------------------
        #region フィールド変数
        //----------------------------------------------------------

        internal static readonly System.DateTimeOffset cUnixZero = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Local);

        private System.DateTimeOffset mCurrent;
        private System.DateTimeOffset mStartup;
        private float mSinceStartup;
        private float mSincePrevFrame;
        private int mFrameCount;

        #endregion

        //----------------------------------------------------------
        #region パブリック関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal Time()
        {
            mStartup = System.DateTimeOffset.Now;
            mCurrent = mStartup;
            mSinceStartup = 0;
            mSincePrevFrame = 0;
            mFrameCount = 0;
        }

        internal void OnBeforeUpdate()
        {
            var prev = mCurrent;
            mCurrent = System.DateTimeOffset.Now;
            mSinceStartup = (float)mCurrent.Subtract(mStartup).TotalSeconds;
            mSincePrevFrame = (float)mCurrent.Subtract(prev).TotalSeconds;
            mFrameCount++;
        }

        public float SinceStartup { get { return mSinceStartup; } }
        public float SincePrevFrame { get { return mSincePrevFrame; } }
        public int FrameCount { get { return mFrameCount; } }

        public System.DateTimeOffset Current { get { return mCurrent; } }
        public long Timestamp { get { return (long)((mCurrent - cUnixZero).TotalSeconds); } }
        public int Year { get { return mCurrent.Year; } }
        public int Month { get { return mCurrent.Month; } }
        public int Day { get { return mCurrent.Day; } }
        public int Hour { get { return mCurrent.Hour; } }
        public int Minute { get { return mCurrent.Minute; } }
        public int Second { get { return mCurrent.Second; } }
        public int Millisecond { get { return mCurrent.Millisecond; } }
        public System.DayOfWeek DayOfWeek { get { return mCurrent.DayOfWeek; } }

        public System.DateTimeOffset Now { get { return System.DateTimeOffset.Now; } }

        #endregion
    }
}
