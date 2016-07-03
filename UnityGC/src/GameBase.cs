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
        /// ゲームの初期化処理
        /// </summary>
        abstract public void Start();

        /// <summary>
        /// ゲームの更新処理
        /// </summary>
        abstract public void Calc();

        /// <summary>
        /// ゲームの描画処理
        /// </summary>
        abstract public void Draw();

        /// <summary>
        /// ゲームの中断処理
        /// </summary>
        abstract public void Pause();

        /// <summary>
        /// ゲームの終了処理
        /// </summary>
        abstract public void Final();
    }
}
