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
    /// <summary>
    /// 期限付き辞書
    /// </summary>
    public sealed class DictWithLife<TKey, TValue>
        where TKey : System.IEquatable<TKey>
        where TValue : class, new()
    {
        readonly List<TKey> m_AliveList;
        readonly HashSet<TKey> m_CurrentAliveFlag;
        readonly Dictionary<TKey, byte> m_DictCount;
        readonly Dictionary<TKey, TValue> m_DictValue;
        readonly Queue<TValue> m_Pool;

        public DictWithLife(int capacity = 8, int initialSize = 0)
        {
            capacity = math.max(capacity, initialSize);

            m_Pool = new Queue<TValue>(capacity);
            m_AliveList = new List<TKey>(capacity);
            m_CurrentAliveFlag = new HashSet<TKey>();
            m_DictCount = new Dictionary<TKey, byte>(capacity);
            m_DictValue = new Dictionary<TKey, TValue>(capacity);

            for (var i = 0; i < initialSize; i++)
            {
                m_Pool.Enqueue(new TValue());
            }
        }

        public void DecrementLife()
        {
            foreach (var key in m_CurrentAliveFlag)
            {
                m_DictCount[key]++;
            }
            m_CurrentAliveFlag.Clear();

            for (int i = 0, len = m_AliveList.Count; i < len; i++)
            {
                var hash = m_AliveList[i];
                if (--m_DictCount[hash] < 1)
                {
                    m_Pool.Enqueue(m_DictValue[hash]);
                    m_DictCount.Remove(hash);
                    m_DictValue.Remove(hash);

                    m_AliveList.RemoveAt(i);
                    i--;
                    len--;
                }
            }
        }

        public void ReleaseAll()
        {
            for (int i = 0, len = m_AliveList.Count; i < len; i++)
            {
                var key = m_AliveList[i];
                m_Pool.Enqueue(m_DictValue[key]);
            }

            m_AliveList.Clear();
            m_CurrentAliveFlag.Clear();
            m_DictCount.Clear();
            m_DictValue.Clear();
        }

        public bool TryGetValue(in TKey key, out TValue value)
        {
            if (m_DictValue.TryGetValue(key, out value))
            {
                m_CurrentAliveFlag.Add(key);
                return true;
            }
            return false;
        }

        public void Issue(in TKey key, out TValue value, in byte life = 3)
        {
            if (m_AliveList.Contains(key))
                throw new System.InvalidOperationException();

            value = GetOrCreate();
            m_AliveList.Add(key);
            m_DictCount.Add(key, life);
            m_DictValue.Add(key, value);
        }

        private TValue GetOrCreate()
        {
            return (m_Pool.Count > 0)
                ? m_Pool.Dequeue()
                : new TValue();
        }
    }
}
