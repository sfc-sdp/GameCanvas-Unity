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
    }
}
