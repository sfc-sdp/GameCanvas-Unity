/*------------------------------------------------------------*/
/// <summary>GameGanvas Base Class</summary>
/// <author>Seibe TAKAHASHI</author>
/// <remarks>
/// (c) 2015-2016 Smart Device Programming.
/// This software is released under the MIT License.
/// http://opensource.org/licenses/mit-license.php
/// </remarks>
/*------------------------------------------------------------*/
using UnityEngine;

namespace GameCanvas
{
    /// <summary>
    /// 内部に GameCanvas インスタンスを持つ MonoBehaviour
    /// </summary>
    [DisallowMultipleComponent()]
    public abstract class GameBase : MonoBehaviour
    {
        /// <summary>
        /// GameCanvasへの参照
        /// </summary>
        protected GameCanvas gc;
        
        private void Awake()
        {
            enabled = false;
            gc = GameCanvas.CreateInstance();
            gc.Regist(Ready, Calc, Draw);
        }

        private void Ready()
        {
            enabled = true;
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
