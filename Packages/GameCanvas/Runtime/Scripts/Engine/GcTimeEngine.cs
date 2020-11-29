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

namespace GameCanvas.Engine
{
    sealed class GcTimeEngine : ITime, IEngine
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        internal static readonly System.DateTimeOffset k_UnixZero = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Local);

#pragma warning disable IDE0032
        System.DateTimeOffset m_CurrentTime;
        int m_FrameCount;
        float m_SincePrevFrame;
        float m_SinceStartup;
        System.DateTimeOffset m_StartupTime;
        double m_TargetFrameInterval;
        bool m_VSyncEnabled;
#pragma warning restore IDE0032
        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        public int CurrentFrame => m_FrameCount;

        public System.DateTimeOffset CurrentTime => m_CurrentTime;

        public long CurrentTimestamp => (long)((m_CurrentTime - k_UnixZero).TotalSeconds);

        public System.DateTimeOffset NowTime => System.DateTimeOffset.Now;

        public double TargetFrameInterval => m_TargetFrameInterval;

        public int TargetFrameRate => (int)(1d / m_TargetFrameInterval);

        public float TimeSincePrevFrame => m_SincePrevFrame;

        public float TimeSinceStartup => m_SinceStartup;

        public bool VSyncEnabled => m_VSyncEnabled;

        public void SetFrameInterval(in double targetDeltaTime, bool vSyncEnabled = true)
        {
            if (targetDeltaTime <= 0)
            {
                Debug.LogError($"{nameof(targetDeltaTime)} に 0 以下の値は指定できません");
                return;
            }
            m_TargetFrameInterval = targetDeltaTime;
            m_VSyncEnabled = vSyncEnabled;

            QualitySettings.vSyncCount = m_VSyncEnabled ? 1 : 0;
            Application.targetFrameRate = m_VSyncEnabled ? TargetFrameRate : int.MaxValue;
        }

        public void SetFrameRate(in int targetFrameRate, bool vSyncEnabled = true)
        {
            SetFrameInterval(1d / targetFrameRate, vSyncEnabled);
        }
        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        internal GcTimeEngine()
        {
            m_StartupTime = System.DateTimeOffset.Now;
            m_CurrentTime = m_StartupTime;
            m_SinceStartup = 0;
            m_SincePrevFrame = 0;
            m_FrameCount = 0;

            SetFrameRate(60, true);
        }

        void System.IDisposable.Dispose() { }

        void IEngine.OnAfterDraw() { }

        void IEngine.OnBeforeUpdate(in System.DateTimeOffset now)
        {
            var prev = m_CurrentTime;
            m_CurrentTime = now;
            m_SinceStartup = (float)m_CurrentTime.Subtract(m_StartupTime).TotalSeconds;
            m_SincePrevFrame = (float)m_CurrentTime.Subtract(prev).TotalSeconds;
            m_FrameCount++;
        }
        #endregion
    }
}
