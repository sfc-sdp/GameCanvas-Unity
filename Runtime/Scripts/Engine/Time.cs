/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
namespace GameCanvas.Engine
{
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

        public float SinceStartup => mSinceStartup;
        public float SincePrevFrame => mSincePrevFrame;
        public int FrameCount => mFrameCount;

        public System.DateTimeOffset Current => mCurrent;
        public long Timestamp => (long)((mCurrent - cUnixZero).TotalSeconds);
        public int Year => mCurrent.Year;
        public int Month => mCurrent.Month;
        public int Day => mCurrent.Day;
        public int Hour => mCurrent.Hour;
        public int Minute => mCurrent.Minute;
        public int Second => mCurrent.Second;
        public int Millisecond => mCurrent.Millisecond;
        public System.DayOfWeek DayOfWeek => mCurrent.DayOfWeek;

        public System.DateTimeOffset Now => System.DateTimeOffset.Now;

        #endregion
    }
}
