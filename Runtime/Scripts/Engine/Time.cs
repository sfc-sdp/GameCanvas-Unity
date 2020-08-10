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
        #region 変数
        //----------------------------------------------------------

        internal static readonly System.DateTimeOffset UnixZero = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Local);

        private System.DateTimeOffset m_CurrentTime;
        private System.DateTimeOffset m_StartupTime;
        private float m_SinceStartup;
        private float m_SincePrevFrame;
        private int m_FrameCount;

        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        /// <summary>
        /// コンストラクタ
        /// </summary>
        internal Time()
        {
            m_StartupTime = System.DateTimeOffset.Now;
            m_CurrentTime = m_StartupTime;
            m_SinceStartup = 0;
            m_SincePrevFrame = 0;
            m_FrameCount = 0;
        }

        internal void OnBeforeUpdate(in System.DateTimeOffset now)
        {
            var prev = m_CurrentTime;
            m_CurrentTime = now;
            m_SinceStartup = (float)m_CurrentTime.Subtract(m_StartupTime).TotalSeconds;
            m_SincePrevFrame = (float)m_CurrentTime.Subtract(prev).TotalSeconds;
            m_FrameCount++;
        }

        public float SinceStartup => m_SinceStartup;
        public float SincePrevFrame => m_SincePrevFrame;
        public int FrameCount => m_FrameCount;

        public System.DateTimeOffset Current => m_CurrentTime;
        public long Timestamp => (long)((m_CurrentTime - UnixZero).TotalSeconds);
        public int Year => m_CurrentTime.Year;
        public int Month => m_CurrentTime.Month;
        public int Day => m_CurrentTime.Day;
        public int Hour => m_CurrentTime.Hour;
        public int Minute => m_CurrentTime.Minute;
        public int Second => m_CurrentTime.Second;
        public int Millisecond => m_CurrentTime.Millisecond;
        public System.DayOfWeek DayOfWeek => m_CurrentTime.DayOfWeek;

        public System.DateTimeOffset Now => System.DateTimeOffset.Now;

        #endregion
    }
}
