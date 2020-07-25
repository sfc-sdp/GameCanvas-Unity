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
using GameCanvas.Input;
using UnityEngine;
using UnityEngine.Assertions;
using Collision = GameCanvas.Engine.Collision;
using Network = GameCanvas.Engine.Network;
using Sequence = System.Collections.IEnumerator;
using Time = GameCanvas.Engine.Time;

namespace GameCanvas
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera), typeof(AudioListener), typeof(AudioSource))]
    public abstract class BehaviourBase : MonoBehaviour
    {
        //----------------------------------------------------------
        #region フィールド変数
        //----------------------------------------------------------

        [SerializeField]
        internal int CanvasWidth = 720;
        [SerializeField]
        internal int CanvasHeight = 1280;
        [SerializeField]
        internal Resource Resource = null;

        internal Camera m_Camera;

        private Proxy m_Proxy;
        private Sequence m_Sequence;
        private bool m_IsPause;

        #endregion

        //----------------------------------------------------------
        #region  Unity イベント関数
        //----------------------------------------------------------

        private void Awake()
        {
            if (Resource == null)
            {
                throw new System.NullReferenceException("Game コンポーネントに Res.asset がアタッチされていません");
            }
            Resource.Initialize();

            m_Camera = GetComponent<Camera>();
            Assert.IsNotNull(m_Camera);
            Assert.IsNotNull(GetComponent<AudioListener>());

            m_IsPause = false;
            m_Proxy = new Proxy(this);
        }

        private Sequence Start()
        {
            if (Resource == null) yield break;

#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += OnChangedPlayMode;

            void OnChangedPlayMode(UnityEditor.PlayModeStateChange change)
            {
                if (change == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                {
                    UnityEditor.EditorApplication.playModeStateChanged -= OnChangedPlayMode;
                    PauseGame();
                }
            }
#endif // UNITY_EDITOR

            var now = System.DateTimeOffset.Now;
            m_Proxy.OnBeforeUpdate(now);
            InitGame();
            m_Sequence = Entry();
            UpdateGame();
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
                        m_Proxy.OnBeforeUpdate(now);
                    }
                    samplers[4].End();

                    samplers[5].Begin();
                    {
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
                        DrawGame();
                    }
                    samplers[7].End();
                }
                samplers[3].End();
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

        private void OnEnable()
        {
            m_Proxy.Graphic?.OnEnable();
        }

        private void OnDisable()
        {
            m_Proxy.Graphic?.OnDisable();
        }

        private void OnDestroy()
        {
            m_Proxy.Graphic?.Dispose();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            if (Resource == null)
            {
                try
                {
                    Resource = UnityEditor.AssetDatabase.LoadAssetAtPath<Resource>("Assets/GameCanvas/Res.asset");
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                }
                catch (System.InvalidOperationException) { }
            }
        }

        private void OnValidate()
        {
            Reset();
            m_Proxy?.Graphic?.SetResolution(CanvasWidth, CanvasHeight);
        }
#endif //UNITY_EDITOR

        #endregion

        //----------------------------------------------------------
        #region 公開関数 (Game.cs に公開している関数)
        //----------------------------------------------------------

        protected Proxy gc => m_Proxy;

        /// <summary>
        /// アプリ起動直後の処理
        /// </summary>
        public abstract void InitGame();

        /// <summary>
        /// 毎フレームの描画前の計算処理
        /// </summary>
        public abstract void UpdateGame();

        /// <summary>
        /// 毎フレームの計算後の描画処理
        /// </summary>
        public abstract void DrawGame();

        /// <summary>
        /// アプリが一時停止する直前の処理
        /// </summary>
        public abstract void PauseGame();

        /// <summary>
        /// アプリが一時停止から復帰した直後の処理
        /// </summary>
        public abstract void ResumeGame();

        /// <summary>
        /// ゲームループを記述するコルーチン
        /// </summary>
        public abstract Sequence Entry();

        #endregion
    }
}
