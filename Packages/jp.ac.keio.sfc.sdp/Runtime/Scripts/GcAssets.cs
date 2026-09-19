#nullable enable
using System.Text;
using UnityEngine;

namespace GameCanvas
{
    /// <summary>Assets/Resからの相対パス（拡張子を含む）で素材を取得します。</summary>
    public static class GcAssets
    {
        internal const string Prefix = "catalog:";
        static GcAssetCatalog? catalog;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void Reset() => catalog = null;

        internal static string Normalize(string key) => key.Replace('\\', '/').Normalize(NormalizationForm.FormC);
        internal static T? Find<T>(string key) where T : Object
        {
            if (string.IsNullOrEmpty(key)) return null;
            if (catalog == null) catalog = Resources.Load<GcAssetCatalog>("GcAssetCatalog");
            return catalog == null ? null : catalog.Find<T>(Normalize(key));
        }
        internal static T? Resolve<T>(string path) where T : Object
            => path != null && path.StartsWith(Prefix, System.StringComparison.Ordinal) ? Find<T>(path.Substring(Prefix.Length)) : null;

        public static bool TryGetImage(string key, out GcImage image)
        {
            var sprite = Find<Sprite>(key);
            image = sprite == null ? default : new GcImage(Prefix + Normalize(key), (int)sprite.rect.width, (int)sprite.rect.height);
            return sprite != null;
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
