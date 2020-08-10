/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2020 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
using System.Collections;
using System.Collections.Generic;

namespace GameCanvas
{
    public interface IScene
    {
        /// <summary>
        /// シーンの開始処理。シーン遷移時にGameCanvasにより自動的に呼び出されます。
        /// </summary>
        /// <param name="state"><see cref="Proxy.ChangeScene{T}"/>を通じて渡される任意の値。未設定の場合はnull</param>
        void EnterScene(object state);

        /// <summary>
        /// シーンの終了処理。シーン遷移時にGameCanvasにより自動的に呼び出されます。
        /// </summary>
        void LeaveScene();

        /// <summary>
        /// シーンの計算処理。毎フレーム（描画より前に）GameCanvasにより自動的に呼び出されます。
        /// </summary>
        void UpdateScene();

        /// <summary>
        /// シーンの描画処理。毎フレーム（計算より後に）GameCanvasにより自動的に呼び出されます。
        /// </summary>
        void DrawScene();

        /// <summary>
        /// シーンの中断処理。アプリが一時停止する直前にGameCanvasにより自動的に呼び出されます。
        /// </summary>
        void PauseScene();

        /// <summary>
        /// シーンの再開処理。アプリが再開する直前にGameCanvasにより自動的に呼び出されます。
        /// </summary>
        void ResumeScene();
    }

    /// <summary>
    /// シーン基底
    /// </summary>
    public abstract class Scene : IScene
    {
        #region 構造体定義

        public struct ReadOnlyActorList<T> : ICollection<T>, IEnumerable<T>, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>
            where T : Actor
        {
            private readonly List<Actor> m_List;
            
            internal ReadOnlyActorList(List<Actor> list)
            {
                m_List = list;
            }

            public T this[int index]
            {
                get => (T)m_List[index];
                set => throw new System.NotSupportedException();
            }
            public int Count => (m_List == null) ? 0 : m_List.Count;
            public bool IsReadOnly => true;
            public void Add(T item) => throw new System.NotSupportedException();
            public void Clear() => throw new System.NotSupportedException();
            public bool Contains(T item) => (m_List != null) && m_List.Contains(item);
            public void CopyTo(T[] array, int arrayIndex) => m_List?.CopyTo(array, arrayIndex);
            public ActorEnumerator<T> GetEnumerator() => new ActorEnumerator<T>(m_List);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
            public int IndexOf(T item) => (m_List == null) ? -1 : m_List.IndexOf(item);
            public void Insert(int index, T item) => throw new System.NotSupportedException();
            public bool Remove(T item) => throw new System.NotSupportedException();
            public void RemoveAt(int index) => throw new System.NotSupportedException();
        }

        public struct ActorEnumerator<T> : IEnumerator<T> where T : Actor
        {
            private readonly List<Actor> m_List;
            private int m_Index;
            private T m_Current;

            internal ActorEnumerator(List<Actor> list)
            {
                m_List = list;
                m_Index = -1;
                m_Current = null;
            }

            public T Current => m_Current;
            object IEnumerator.Current => m_Current;
            public void Dispose() {}
            public bool MoveNext()
            {
                if (m_List != null && ++m_Index < m_List.Count)
                {
                    m_Current = (T)m_List[m_Index];
                    return true;
                }
                return false;
            }
            public void Reset()
            {
                m_Index = -1;
                m_Current = null;
            }
        }

        #endregion

        // ---

        private readonly List<Actor> m_ActorList = new List<Actor>();
        private readonly List<Actor> m_AddActorList = new List<Actor>();
        private readonly List<Actor> m_RemoveActorList = new List<Actor>();
        private readonly Dictionary<System.Type, List<Actor>> m_TypeToActors = new Dictionary<System.Type, List<Actor>>();

        private uint m_NextActorIndex;
#pragma warning disable IDE1006
        protected static Proxy gc { get; private set; }
#pragma warning restore IDE1006

        // ---

        public virtual void EnterScene(object state) { }

        public virtual void LeaveScene() { gc.RemoveActorAll(); }

        public virtual void UpdateScene() { }

        public virtual void DrawScene() { }

        public virtual void PauseScene() { }

        public virtual void ResumeScene() { }

        /// <summary>
        /// 指定したアクターを生成し、シーンに登録します。
        /// </summary>
        /// <typeparam name="T">生成・登録するアクターの型</typeparam>
        /// <returns>登録したアクター</returns>
        public T CreateActor<T>() where T : Actor, new()
        {
            var actor = new T();
            AddActor(typeof(T), actor);
            return actor;
        }

        /// <summary>
        /// 指定したアクターをシーンに登録します。
        /// </summary>
        /// <param name="actor">登録するアクター</param>
        public void AddActor(Actor actor)
        {
            AddActor(actor.GetType(), actor);
        }

        /// <summary>
        /// 指定したアクターをシーンから登録解除します。
        /// </summary>
        /// <param name="actor">登録解除するアクター</param>
        /// <returns>登録解除できたかどうか</returns>
        public bool TryRemoveActor(Actor actor)
        {
            if (actor.m_Scene != this) return false;

            if (m_AddActorList.Remove(actor))
            {
                actor.m_Scene = null;
                return true;
            }

            if (m_ActorList.Contains(actor))
            {
                // すぐには削除せず、削除を予約しておく
                m_RemoveActorList.Add(actor);
                return true;
            }

            return false;
        }

        /// <summary>
        /// シーンに登録されているすべてのアクターを登録解除します。
        /// </summary>
        public void RemoveActorAll()
        {
            foreach (var list in m_TypeToActors.Values)
            {
                foreach (var actor in list)
                {
                    actor.m_Scene = null;
                }
            }
            m_TypeToActors.Clear();
            m_ActorList.Clear();
            m_RemoveActorList.Clear();

            foreach (var actor in m_AddActorList)
            {
                actor.m_Scene = null;
            }
            m_AddActorList.Clear();
        }

        /// <summary>
        /// シーンに登録されているアクターのうち、指定した型のものを1つだけ取得します。
        /// </summary>
        /// <typeparam name="T">取得するアクターの型</typeparam>
        /// <returns>取得できたアクター</returns>
        public T GetActor<T>() where T : Actor
        {
            if (m_TypeToActors.TryGetValue(typeof(T), out var list) && list.Count > 0)
            {
                return (T)list[0];
            }
            return default;
        }

        /// <summary>
        /// シーンに登録されているアクターのうち、指定した型のものを1つだけ取得します。
        /// </summary>
        /// <param name="actor">取得できたアクター</param>
        /// <typeparam name="T">取得するアクターの型</typeparam>
        /// <returns>取得できたかどうか</returns>
        public bool TryGetActor<T>(out T actor) where T : Actor
        {
            if (m_TypeToActors.TryGetValue(typeof(T), out var list) && list.Count > 0)
            {
                actor = (T)list[0];
                return true;
            }
            actor = default;
            return false;
        }

        /// <summary>
        /// シーンに登録されているアクターの総数を取得します。
        /// </summary>
        /// <returns>シーンに登録されているアクターの総数</returns>
        public int GetActorCount()
        {
            var total = 0;

            foreach(var list in m_TypeToActors.Values)
            {
                total += list.Count;
            }
            return total;
        }

        /// <summary>
        /// シーンに登録されているアクターのうち、指定した型のものが幾つあるか。
        /// </summary>
        /// <typeparam name="T">取得するアクターの型</typeparam>
        /// <returns>登録数</returns>
        public int GetActorCount<T>() where T : Actor
        {
            if (m_TypeToActors.TryGetValue(typeof(T), out var list))
            {
                return list.Count;
            }
            return 0;
        }

        /// <summary>
        /// シーンに登録されているアクターのうち、指定した型のものを取得します。
        /// </summary>
        /// <typeparam name="T">取得するアクターの型</typeparam>
        /// <returns>取得できたアクターのリスト</returns>
        public ReadOnlyActorList<T> GetActorList<T>() where T : Actor
        {
            if (m_TypeToActors.TryGetValue(typeof(T), out var list))
            {
                return new ReadOnlyActorList<T>(list);
            }
            return default;
        }

        // ---

        internal static void Inject(Proxy proxy)
        {
            gc = proxy;
        }

        internal void Update()
        {
            SortActorList();

            foreach (var actor in m_ActorList)
            {
                if (actor.m_Priority >= 0) break;
                actor.Update();
            }

            UpdateScene();

            foreach (var actor in m_ActorList)
            {
                if (actor.m_Priority < 0) continue;
                actor.Update();
            }
        }

        internal void Draw()
        {
            SortActorList();

            foreach (var actor in m_ActorList)
            {
                if (actor.m_Priority >= 0) break;
                actor.Draw();
            }

            DrawScene();

            foreach (var actor in m_ActorList)
            {
                if (actor.m_Priority < 0) continue;
                actor.Draw();
            }

            foreach (var actor in m_ActorList)
            {
                actor.AfterDraw();
            }
        }

        // ---

        private void AddActor(System.Type type, Actor actor)
        {
            if (actor.m_Scene == this) return;

            if (!m_TypeToActors.ContainsKey(type))
            {
                m_TypeToActors.Add(type, new List<Actor>());
            }

            actor.m_Scene?.TryRemoveActor(actor);
            actor.m_Scene = this;
            actor.m_Index = m_NextActorIndex++;
            m_AddActorList.Add(actor);
            m_TypeToActors[type].Add(actor);
        }

        private void SortActorList()
        {
            // 削除予約を実行する
            foreach (var actor in m_RemoveActorList)
            {
                m_ActorList.Remove(actor);
                if (m_TypeToActors.TryGetValue(actor.GetType(), out var list))
                {
                    list.Remove(actor);
                }
                actor.m_Scene = null;
            }
            m_RemoveActorList.Clear();

            // 追加予約を実行する
            m_ActorList.AddRange(m_AddActorList);
            m_AddActorList.Clear();

            // 配列のソート
            m_ActorList.Sort();
        }
    }
}
