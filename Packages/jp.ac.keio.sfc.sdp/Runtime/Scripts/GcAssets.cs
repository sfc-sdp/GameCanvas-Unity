#nullable enable
using System.Text;
using System.Collections.Generic;
using UnityEngine;

namespace GameCanvas
{
    /// <summary>Assets/Resからの相対パス（拡張子を含む）で素材を取得します。</summary>
    public static class GcAssets
    {
        internal const string Prefix = "catalog:";
        static GcAssetCatalog? catalog;
        const int CacheLimit = 256;
        static readonly Dictionary<string, GcImage> images = new(System.StringComparer.Ordinal);
        static readonly HashSet<string> reportedMissing = new(System.StringComparer.Ordinal);
        static bool missingLimitReported;
        static bool catalogLoaded;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void Reset()
        {
            if (catalog != null) catalog.Invalidate();
            catalog = null; catalogLoaded = false;
            images.Clear(); reportedMissing.Clear(); missingLimitReported = false;
        }

        internal static string Normalize(string key) => key.Replace('\\', '/').Normalize(NormalizationForm.FormC);
        internal static T? Find<T>(string key) where T : Object
        {
            if (string.IsNullOrEmpty(key)) return null;
            if (!catalogLoaded)
            { catalog = Resources.Load<GcAssetCatalog>("GcAssetCatalog"); catalogLoaded = true; }
            return catalog == null ? null : catalog.Find<T>(Normalize(key));
        }
        internal static T? Resolve<T>(string path) where T : Object
            => path != null && path.StartsWith(Prefix, System.StringComparison.Ordinal) ? Find<T>(path.Substring(Prefix.Length)) : null;

        public static bool TryGetImage(string key, out GcImage image)
        {
            if (string.IsNullOrEmpty(key)) { image = default; return false; }
            if (images.TryGetValue(key, out image)) return !image.Invalid;
            var sprite = Find<Sprite>(key);
            image = sprite == null ? default : new GcImage(Prefix + Normalize(key), (int)sprite.rect.width, (int)sprite.rect.height);
            // 画像は複製しない。文字列と小さなハンドルだけを上限付きで保持する。
            if (images.Count >= CacheLimit) images.Clear();
            images[key] = image;
            return sprite != null;
        }
        internal static void ReportMissingImage(string key)
        {
            key ??= "<null>";
            if (reportedMissing.Contains(key)) return;
            if (reportedMissing.Count >= CacheLimit)
            {
                if (!missingLimitReported) UnityEngine.Debug.LogWarning("[GameCanvas] GC_ASSET_MISSING: further missing image paths suppressed.");
                missingLimitReported = true; return;
            }
            reportedMissing.Add(key);
            UnityEngine.Debug.LogWarning($"[GameCanvas] GC_ASSET_MISSING: '{key}'. Add this image to Assets/Res and check the asset catalog.");
        }
        public static bool TryGetSound(string key, out GcSound sound)
        {
            var clip = Find<AudioClip>(key);
            sound = clip == null ? default : new GcSound(Prefix + Normalize(key));
            return clip != null;
        }
        public static bool TryGetFont(string key, out GcFont font)
        {
            var asset = Find<Font>(key);
            font = asset == null ? default : new GcFont(Prefix + Normalize(key));
            return asset != null;
        }
        public static bool TryGetText(string key, out string text)
        {
            var asset = Find<TextAsset>(key);
            text = asset == null ? "" : asset.text;
            return asset != null;
        }
    }
}
