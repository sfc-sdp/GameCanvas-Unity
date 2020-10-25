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
    public struct ReadOnlyActorList<T>
        : ICollection<T>, IEnumerable<T>, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>
        where T : GcActor
    {
        //----------------------------------------------------------
        #region 構造体
        //----------------------------------------------------------

        public struct Enumerator : IEnumerator<T>
        {
#pragma warning disable IDE0032
            private readonly List<GcActor> m_List;
            private T m_Current;
            private int m_Index;
#pragma warning restore IDE0032

            internal Enumerator(in List<GcActor> list)
            {
                m_List = list;
                m_Index = -1;
                m_Current = null;
            }

            public T Current => m_Current;
            object IEnumerator.Current => m_Current;
            public void Dispose() { }
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

        //----------------------------------------------------------
        #region 変数
        //----------------------------------------------------------

        private readonly List<GcActor> m_List;

        #endregion

        //----------------------------------------------------------
        #region 公開関数
        //----------------------------------------------------------

        public int Count => (m_List == null) ? 0 : m_List.Count;
        public bool IsReadOnly => true;

        public T this[int index]
        {
            get => (T)m_List[index];
            set => throw new System.NotSupportedException();
        }
        public void Add(T item) => throw new System.NotSupportedException();
        public void Clear() => throw new System.NotSupportedException();
        public bool Contains(T item) => (m_List != null) && m_List.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => m_List?.CopyTo(array, arrayIndex);
        public Enumerator GetEnumerator() => new Enumerator(m_List);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        public int IndexOf(T item) => (m_List == null) ? -1 : m_List.IndexOf(item);
        public void Insert(int index, T item) => throw new System.NotSupportedException();
        public bool Remove(T item) => throw new System.NotSupportedException();
        public void RemoveAt(int index) => throw new System.NotSupportedException();

        #endregion

        //----------------------------------------------------------
        #region 内部関数
        //----------------------------------------------------------

        internal ReadOnlyActorList(List<GcActor> list)
        {
            m_List = list;
        }
        #endregion
    }
}
