/*------------------------------------------------------------*/
// <summary>GameCanvas for Unity</summary>
// <author>Seibe TAKAHASHI</author>
// <remarks>
// (c) 2015-2024 Smart Device Programming.
// This software is released under the MIT License.
// http://opensource.org/licenses/mit-license.php
// </remarks>
/*------------------------------------------------------------*/
#nullable enable
using System.Collections;
using System.Collections.Generic;

namespace GameCanvas
{
    public readonly struct ReadOnlyActorList<T>
        : ICollection<T>, IEnumerable<T>, IList<T>, IReadOnlyCollection<T>, IReadOnlyList<T>
        where T : GcActor
    {
        //----------------------------------------------------------
        #region 構造体
        //----------------------------------------------------------

        public struct Enumerator : IEnumerator<T>
        {
            private readonly List<GcActor> m_List;
            private T? m_Current;
            private int m_Index;

            internal Enumerator(in List<GcActor> list)
            {
                m_List = list;
                m_Index = -1;
                m_Current = null;
            }

            public readonly T Current => m_Current ?? throw new System.InvalidOperationException();
            readonly object? IEnumerator.Current => m_Current;
            public readonly void Dispose() { }
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

        public readonly int Count => (m_List == null) ? 0 : m_List.Count;
        public readonly bool IsReadOnly => true;

        public T this[int index]
        {
            readonly get => (T)m_List[index];
            set => throw new System.NotSupportedException();
        }
        public void Add(T item) => throw new System.NotSupportedException();
        public void Clear() => throw new System.NotSupportedException();
        public readonly bool Contains(T item) => (m_List != null) && m_List.Contains(item);
        public readonly void CopyTo(T[] array, int arrayIndex) => m_List?.CopyTo(array, arrayIndex);
        public readonly Enumerator GetEnumerator() => new(m_List);
        readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        readonly IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        public readonly int IndexOf(T item) => (m_List == null) ? -1 : m_List.IndexOf(item);
        public void Insert(int index, T item) => throw new System.NotSupportedException();
        public readonly bool Remove(T item) => throw new System.NotSupportedException();
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
