/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2026 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
#nullable enable
using UnityEngine;

namespace GameCanvas.Engine
{
    sealed class GcTimeEngine : ITime, IEngine
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        internal static readonly System.DateTimeOffset k_UnixZero = System.DateTimeOffset.UnixEpoch;

#pragma warning disable IDE0032
        System.DateTimeOffset m_CurrentTime;
        int m_FrameCount;
        float m_SincePrevFrame;
        double m_SinceStartup;
        readonly System.Func<double> m_Clock;
        readonly double m_StartupTick;
        double m_PreviousTick;
        bool m_ResetDelta = true;
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

        public int TargetFrameRate => (int)System.Math.Round(1d / m_TargetFrameInterval);

        public float TimeSincePrevFrame => m_SincePrevFrame;

        public double TimeSinceStartup => m_SinceStartup;

        public bool VSyncEnabled => m_VSyncEnabled;

        public void SetFrameInterval(in double targetDeltaTime, bool vSyncEnabled = true)
        {
            if (double.IsNaN(targetDeltaTime) || double.IsInfinity(targetDeltaTime) ||
                targetDeltaTime < 1d / int.MaxValue || targetDeltaTime > 1)
                throw new System.ArgumentOutOfRangeException(nameof(targetDeltaTime));
            m_TargetFrameInterval = targetDeltaTime;
            m_VSyncEnabled = vSyncEnabled;

            QualitySettings.vSyncCount = m_VSyncEnabled ? 1 : 0;
            Application.targetFrameRate = TargetFrameRate;
        }

        public void SetFrameRate(in int targetFrameRate, bool vSyncEnabled = true)
        {
            if (targetFrameRate <= 0) throw new System.ArgumentOutOfRangeException(nameof(targetFrameRate));
            SetFrameInterval(1d / targetFrameRate, vSyncEnabled);
        }
        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        internal GcTimeEngine(System.Func<double>? clock = null)
        {
            m_Clock = clock ?? (() => Time.realtimeSinceStartupAsDouble);
            m_StartupTick = m_PreviousTick = m_Clock();
            m_CurrentTime = System.DateTimeOffset.Now;
            m_SinceStartup = 0;
            m_SincePrevFrame = 0;
            m_FrameCount = 0;

            SetFrameRate(60, true);
        }

        internal void ResetDelta() => m_ResetDelta = true;

        void System.IDisposable.Dispose() { }

        void IEngine.OnAfterDraw() { }

        void IEngine.OnBeforeUpdate(in System.DateTimeOffset now)
        {
            m_CurrentTime = now;
            var tick = System.Math.Max(m_PreviousTick, m_Clock());
            m_SinceStartup = tick - m_StartupTick;
            m_SincePrevFrame = m_ResetDelta ? 0 : (float)(tick - m_PreviousTick);
            m_ResetDelta = false;
            m_PreviousTick = tick;
            m_FrameCount++;
        }
        #endregion
    }
}
