/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;
using Sequence = System.Collections.IEnumerator;

namespace GameCanvas
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera), typeof(AudioListener))]
    public abstract class BehaviourBase : MonoBehaviour
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        internal Camera m_Camera;
        internal event System.Action OnFocusOnce;

#pragma warning disable IDE0032
        private bool m_IsPause;
        private GcProxy m_Proxy;
        private Sequence m_Sequence;
#pragma warning restore IDE0032

        #endregion

        //----------------------------------------------------------
        #region  Unity イベント関数
        //----------------------------------------------------------

        private void Awake()
        {
            m_Camera = GetComponent<Camera>() ?? Camera.main;
            Assert.IsNotNull(m_Camera);
            Assert.IsNotNull(GetComponent<AudioListener>());

            m_IsPause = false;
            m_Proxy = new GcProxy(this);
        }

        private void OnApplicationFocus(bool focus)
        {
            if (focus)
            {
                OnFocusOnce?.Invoke();
                OnFocusOnce = null;
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                if (!m_IsPause)
                {
                    m_IsPause = true;
                    PauseGame();
                }
            }
            else
            {
                if (m_IsPause)
                {
                    m_IsPause = false;
                    ResumeGame();
                }
            }
        }

        private void OnDisable()
        {
            m_Proxy.OnDisable();
            OnFocusOnce = null;
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            if (m_Proxy == null) Awake();

            UnityEditor.EditorApplication.playModeStateChanged += OnChangedPlayMode;

            void OnChangedPlayMode(UnityEditor.PlayModeStateChange change)
            {
                if (change == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                {
                    UnityEditor.EditorApplication.playModeStateChanged -= OnChangedPlayMode;
                    OnDisable();
                    StopAllCoroutines();
                }
            }
#endif // UNITY_EDITOR

            StartCoroutine(GameLoop());
        }

        private Sequence GameLoop()
        {
            m_Proxy.OnEnable();

            var now = System.DateTimeOffset.Now;
            m_Proxy.OnBeforeUpdate(now);
            InitGame();
            m_Sequence = Entry();
            m_Proxy.UpdateCurrentScene();
            UpdateGame();
            m_Proxy.DrawCurrentScene();
            DrawGame();

            var samplers = new[] {
                UnityEngine.Profiling.CustomSampler.Create("WaitForNextFrame"),
                UnityEngine.Profiling.CustomSampler.Create("Sleep"),
                UnityEngine.Profiling.CustomSampler.Create("BusyWait"),
                UnityEngine.Profiling.CustomSampler.Create("GameCanvas"),
                UnityEngine.Profiling.CustomSampler.Create("EngineUpdate"),
                UnityEngine.Profiling.CustomSampler.Create("UpdateGame"),
                UnityEngine.Profiling.CustomSampler.Create("Entry.MoveNext"),
                UnityEngine.Profiling.CustomSampler.Create("DrawGame")
            };
            var isRunning = true;
            var targetFrameTime = System.DateTimeOffset.Now;
            var w4ef = new WaitForEndOfFrame();

            while (enabled)
            {
                if (!m_Proxy.VSyncEnabled)
                {
                    //
                    // https://blogs.unity3d.com/jp/2019/06/03/precise-framerates-in-unity/
                    //
                    yield return w4ef;

                    samplers[0].Begin();
                    {
                        targetFrameTime += System.TimeSpan.FromSeconds(m_Proxy.TargetFrameInterval);
                        now = System.DateTimeOffset.Now;

                        var diff = (targetFrameTime - now).TotalMilliseconds;
                        if (diff > 0)
                        {
                            if (diff > 1)
                            {
                                samplers[1].Begin();
                                {
                                    var sleepTime = Mathf.Max(0, (int)(diff - 1));
                                    System.Threading.Thread.Sleep(sleepTime);
                                }
                                samplers[1].End();
                            }

                            samplers[2].Begin();
                            {
                                do { now = System.DateTimeOffset.Now; }
                                while (now < targetFrameTime);
                            }
                            samplers[2].End();
                        }
                        else if (diff < 0)
                        {
                            targetFrameTime = now;
                        }
                    }
                    samplers[0].End();
                }

                yield return null;

                samplers[3].Begin();
                {
                    samplers[4].Begin();
                    {
                        m_Proxy.OnBeforeUpdate(System.DateTimeOffset.Now);
                    }
                    samplers[4].End();

                    samplers[5].Begin();
                    {
                        m_Proxy.UpdateCurrentScene();
                        UpdateGame();
                    }
                    samplers[5].End();

                    samplers[6].Begin();
                    {
                        isRunning = isRunning && m_Sequence.MoveNext();
                    }
                    samplers[6].End();

                    samplers[7].Begin();
                    {
                        m_Proxy.DrawCurrentScene();
                        DrawGame();
                        m_Proxy.OnAterDraw();
                    }
                    samplers[7].End();
                }
                samplers[3].End();
            }
        }

        #endregion

        //----------------------------------------------------------
        #region 公開関数 (Game.cs に公開している関数)
        //----------------------------------------------------------

#pragma warning disable IDE1006
        protected IGameCanvas gc
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Proxy;
        }
#pragma warning restore IDE1006

        /// <summary>
        /// 毎フレームの計算後の描画処理
        /// </summary>
        public abstract void DrawGame();

        /// <summary>
        /// ゲームループを記述するコルーチン
        /// </summary>
        public abstract Sequence Entry();

        /// <summary>
        /// アプリ起動直後の処理
        /// </summary>
        public abstract void InitGame();

        /// <summary>
        /// アプリが一時停止する直前の処理
        /// </summary>
        public abstract void PauseGame();

        /// <summary>
        /// アプリが一時停止から復帰した直後の処理
        /// </summary>
        public abstract void ResumeGame();

        /// <summary>
        /// 毎フレームの描画前の計算処理
        /// </summary>
        public abstract void UpdateGame();
        #endregion
    }
}
