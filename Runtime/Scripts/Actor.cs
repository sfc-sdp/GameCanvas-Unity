/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using System.Collections.Generic;

namespace GameCanvas
{
    public interface IActor
    {
        /// <summary>
        /// アクターの処理優先度。初期値は0（無指定）
        /// </summary>
        /// <remarks>
        /// アクターは、シーン内でこの値が小さい順に処理が回ってくる。
        /// 同じ値だった場合は、シーンに登録した順序で実行される。
        /// 
        /// 負の値を設定した場合は、そのシーンの処理よりも前に実行される。
        /// </remarks>
        int Priority { get; set; }

        /// <summary>
        /// アクターの計算処理。毎フレーム（描画より前に）GameCanvasにより自動的に呼び出されます。
        /// </summary>
        void Update();

        /// <summary>
        /// アクターの描画処理。毎フレーム（計算より後に）GameCanvasにより自動的に呼び出されます。
        /// </summary>
        void Draw();

        void AfterDraw();
    }

    /// <summary>
    /// アクター基底
    /// </summary>
    public abstract class Actor : IActor, System.IComparable<Actor>
    {
        internal Scene m_Scene = null;
        internal int m_Priority = 0;
        internal uint m_Index = 0;

        // ---

        public virtual int Priority { get { return m_Priority; } set { m_Priority = value; } }

        public virtual void Update() { }

        public virtual void Draw() { }

        public virtual void AfterDraw() { }

#pragma warning disable IDE1006
        protected static Proxy gc { get; private set; }
#pragma warning restore IDE1006

        internal static void Inject(Proxy proxy) { gc = proxy; }

        public int CompareTo(Actor other)
        {
            if (m_Priority == other.Priority)
            {
                return (int)(m_Index - other.m_Index);
            }
            return m_Priority - other.Priority;
        }
    }
}
