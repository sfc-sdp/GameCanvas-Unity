#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameCanvas
{
    // 描画用Spriteを直接参照する。同名ファイルをアトラス名で誤解決しない。
    sealed class GcAssetCatalog : ScriptableObject
    {
        [Serializable] internal sealed class Entry
        {
            public string key = "";
            public UnityEngine.Object asset = null!;
        }
        [SerializeField] internal Entry[] entries = Array.Empty<Entry>();
        [SerializeField] internal string[] errors = Array.Empty<string>();
        Dictionary<string, UnityEngine.Object>? index;
        Entry[]? indexedEntries;
        internal void Invalidate() { index = null; indexedEntries = null; }
        void OnEnable() => Invalidate();
        void OnValidate() => Invalidate();
        internal T? Find<T>(string key) where T : UnityEngine.Object
        {
            if (errors.Length != 0) return null;
            if (index == null || !ReferenceEquals(indexedEntries, entries))
            {
                index = new Dictionary<string, UnityEngine.Object>(entries.Length, StringComparer.Ordinal);
                foreach (var entry in entries) index[entry.key] = entry.asset;
                indexedEntries = entries;
            }
            return index.TryGetValue(key, out var asset) ? asset as T : null;
        }
    }
}
