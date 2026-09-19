#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;

namespace GameCanvas
{
    /// <summary>
    /// Countと添字で読めるフレーム内の一覧。foreachも使える。
    /// 次の更新で内容が変わるため、残したい要素は値としてコピーする。
    /// </summary>
    public readonly struct GcReadOnlyList<T> : IReadOnlyList<T> where T : struct
    {
        readonly List<T>? items;
        internal GcReadOnlyList(List<T> items) { this.items = items; }
        public int Count => items?.Count ?? 0;
        public T this[int index] => items == null ? throw new ArgumentOutOfRangeException(nameof(index)) : items[index];
        public Enumerator GetEnumerator() => new(this);
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        /// <summary>具体型のままforeachで読む場合はヒープに確保しない。</summary>
        public struct Enumerator : IEnumerator<T>
        {
            readonly GcReadOnlyList<T> list;
            int index;
            internal Enumerator(GcReadOnlyList<T> list) { this.list = list; index = -1; }
            public T Current => index >= 0 && index < list.Count ? list[index] : throw new InvalidOperationException();
            object IEnumerator.Current => Current;
            public bool MoveNext() => ++index < list.Count;
            public void Reset() => index = -1;
            public void Dispose() { }
        }
    }
}
