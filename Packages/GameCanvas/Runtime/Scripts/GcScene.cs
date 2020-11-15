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
    /// <summary>
    /// シーン基底
    /// </summary>
    public abstract class GcScene : IScene, ISceneManagement
    {
        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        private readonly List<GcActor> m_ActorList = new List<GcActor>();
        private readonly List<GcActor> m_AddActorList = new List<GcActor>();
        private readonly List<GcActor> m_RemoveActorList = new List<GcActor>();
        private readonly Dictionary<System.Type, List<GcActor>> m_TypeToActors = new Dictionary<System.Type, List<GcActor>>();

        private uint m_NextActorIndex;
#pragma warning disable IDE1006
        protected static GcProxy gc { get; private set; }
#pragma warning restore IDE1006

        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        /// <inheritdoc/>
        public void AddActor(in GcActor actor)
        {
            AddActor(actor.GetType(), actor);
        }

        /// <inheritdoc/>
        public T CreateActor<T>() where T : GcActor, new()
        {
            var actor = new T();
            AddActor(typeof(T), actor);
            return actor;
        }

        /// <inheritdoc/>
        public virtual void DrawScene() { }

        /// <inheritdoc/>
        public virtual void EnterScene(object state) { }

        /// <inheritdoc/>
        public int GetActorCount() => m_ActorList.Count;

        /// <inheritdoc/>
        public int GetActorCount<T>() where T : GcActor
            => m_TypeToActors.TryGetValue(typeof(T), out var list) ? list.Count : 0;

        /// <inheritdoc/>
        public virtual void LeaveScene() => gc.RemoveActorAll();

        /// <inheritdoc/>
        public virtual void PauseScene() { }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public virtual void ResumeScene() { }

        /// <inheritdoc/>
        public bool TryGetActor(in int i, out GcActor actor)
        {
            if (i >= 0 && m_ActorList.Count > i)
            {
                actor = m_ActorList[i];
                return true;
            }
            actor = default;
            return false;
        }

        /// <inheritdoc/>
        public bool TryGetActor<T>(in int i, out T actor) where T : GcActor
        {
            if (i >= 0 && m_TypeToActors.TryGetValue(typeof(T), out var list) && list.Count > i)
            {
                actor = (T)list[i];
                return true;
            }
            actor = default;
            return false;
        }

        /// <inheritdoc/>
        public bool TryGetActorList<T>(out ReadOnlyActorList<T> list) where T : GcActor
        {
            if (m_TypeToActors.TryGetValue(typeof(T), out var actors))
            {
                list = new ReadOnlyActorList<T>(actors);
                return true;
            }
            list = default;
            return false;
        }

        /// <inheritdoc/>
        public bool TryRemoveActor(in GcActor actor)
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

        /// <inheritdoc/>
        public virtual void UpdateScene() { }
        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        internal static void Inject(GcProxy proxy)
        {
            gc = proxy;
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

        private void AddActor(System.Type type, GcActor actor)
        {
            if (actor.m_Scene == this) return;

            if (!m_TypeToActors.ContainsKey(type))
            {
                m_TypeToActors.Add(type, new List<GcActor>());
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
        #endregion
    }
}
