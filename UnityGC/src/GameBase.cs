using System;
using UnityEngine;

namespace GameCanvas
{
    /// <summary>
    /// 内部に GameCanvas インスタンスを持つ MonoBehaviour
    /// </summary>
    public abstract class GameBase : MonoBehaviour
    {
        /// <summary>
        /// GameCanvasへの参照
        /// </summary>
        protected GameCanvas gc;
        
        private void Awake()
        {
            gc = GameCanvas.Instance;
        }

        private void Update()
        {
            Calc();
            Draw();
        }

        private void OnApplicationPause()
        {
            Pause();
        }

        private void OnApplicationQuit()
        {
            Final();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        abstract public void Start();

        /// <summary>
        /// 更新処理
        /// </summary>
        abstract public void Calc();

        /// <summary>
        /// 描画処理
        /// </summary>
        abstract public void Draw();

        /// <summary>
        /// 中断処理
        /// </summary>
        abstract public void Pause();

        /// <summary>
        /// 終了処理
        /// </summary>
        abstract public void Final();
    }
}
