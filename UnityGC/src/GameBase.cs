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
            gc.ReadDataByStorage();

            gc.SetOnStart(() => { enabled = true; });
            enabled = false;
        }

        private void Update()
        {
            Calc();
            Draw();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        virtual public void Start() { }

        /// <summary>
        /// 更新処理
        /// </summary>
        virtual public void Calc() { }

        /// <summary>
        /// 描画処理
        /// </summary>
        virtual public void Draw() { }
    }
}
