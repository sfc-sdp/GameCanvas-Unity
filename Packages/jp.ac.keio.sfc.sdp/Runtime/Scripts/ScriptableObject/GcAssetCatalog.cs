#nullable enable
using System;
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
        internal T? Find<T>(string key) where T : UnityEngine.Object
        {
            if (errors.Length != 0) return null;
            foreach (var entry in entries)
                if (entry.key == key) return entry.asset as T;
            return null;
        }
    }
}
