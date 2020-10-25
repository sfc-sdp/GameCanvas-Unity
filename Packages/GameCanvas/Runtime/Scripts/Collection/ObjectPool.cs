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
using Unity.Mathematics;

namespace GameCanvas
{
    public sealed class ObjectPool<T> where T : class, new()
    {
        readonly Queue<T> m_Pooled;
        readonly Queue<T> m_Lent;

        public ObjectPool(int capacity = 8, int initialSize = 0)
        {
            capacity = math.max(capacity, initialSize);
            m_Pooled = new Queue<T>(capacity);
            m_Lent = new Queue<T>(capacity);

            for (var i = 0; i < initialSize; i++)
            {
                m_Pooled.Enqueue(new T());
            }
        }

        public T Get()
        {
            var obj = m_Pooled.Dequeue();
            if (obj != null) m_Lent.Enqueue(obj);
            return obj;
        }

        public T GetOrCreate()
        {
            var obj = (m_Pooled.Count == 0) ? new T() : m_Pooled.Dequeue();
            m_Lent.Enqueue(obj);
            return obj;
        }

        public void ReleaseAll()
        {
            while (m_Lent.Count > 0)
            {
                var obj = m_Lent.Dequeue();
                m_Pooled.Enqueue(obj);
            }
        }
    }
}
