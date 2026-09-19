#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace GameCanvas.Editor
{
    [InitializeOnLoad]
    public sealed class GcAssetCatalogBuilder : AssetPostprocessor
    {
        const string Root = "Assets/Res/";
        const string Destination = "Assets/GameCanvas/Resources/GcAssetCatalog.asset";
        const string JsonPath = "Assets/GameCanvas/asset-catalog.json";
        static bool queued, refreshing;
        static GcAssetCatalogBuilder() => Queue();
        static void Queue()
        {
            if (queued || refreshing) return;
            queued = true;
            EditorApplication.delayCall += UpdateWhenReady;
        }
        static void UpdateWhenReady()
        {
            queued = false;
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || BuildPipeline.isBuildingPlayer || EditorApplication.isPlaying)
            { Queue(); return; }
            Refresh(false);
        }
        static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            if (imported.Concat(deleted).Concat(moved).Concat(movedFrom).Any(p => p.StartsWith(Root, StringComparison.Ordinal) || p == "Assets/Res")) Queue();
        }

        public static void Refresh(bool failOnError = true)
        {
            if (refreshing) return;
            refreshing = true;
            try
            {
                var entries = new List<GcAssetCatalog.Entry>();
                var errors = new List<string>();
                var keys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                var rows = new List<Row>();
                foreach (var path in AssetDatabase.GetAllAssetPaths().Where(p => p.StartsWith(Root, StringComparison.Ordinal)).OrderBy(p => p, StringComparer.Ordinal))
                {
                    if (AssetDatabase.IsValidFolder(path)) continue;
                    var key = path.Substring(Root.Length).Normalize(NormalizationForm.FormC);
                    if (keys.TryGetValue(key, out var other))
                    { errors.Add($"GC-ASSET-KEY: {other} と {path} の名前が衝突しています。大小文字とUnicode表記以外を変えてください。"); continue; }
                    keys.Add(key, path);
                    var importer = AssetImporter.GetAtPath(path);
                    UnityEngine.Object? asset = null;
                    if (importer is TextureImporter texture)
                    {
                        if (GcEditorResourceBuilder.OnPreprocessTexture(texture)) texture.SaveAndReimport();
                        asset = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    }
                    else if (importer is AudioImporter) asset = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                    else
                    {
                        var main = AssetDatabase.LoadMainAssetAtPath(path);
                        if (main is Font || main is TextAsset) asset = main;
                    }
                    if (asset == null)
                    { errors.Add($"GC-ASSET-TYPE: {path} を読み込めません。PNG/JPEG、WAV/OGG、TTF/OTF、TXT/JSONなどへ変換してください。"); continue; }
                    entries.Add(new GcAssetCatalog.Entry { key = key, asset = asset });
                    rows.Add(new Row { key = key, type = asset.GetType().Name, path = path });
                }
                Directory.CreateDirectory(Path.GetDirectoryName(Destination)!);
                var catalog = AssetDatabase.LoadAssetAtPath<GcAssetCatalog>(Destination);
                if (catalog == null) { catalog = ScriptableObject.CreateInstance<GcAssetCatalog>(); AssetDatabase.CreateAsset(catalog, Destination); }
                // エラー時にも無効状態を書き込む。古い一覧で成功したように動かさない。
                var nextEntries = errors.Count == 0 ? entries.ToArray() : Array.Empty<GcAssetCatalog.Entry>();
                if (!catalog.errors.SequenceEqual(errors) || catalog.entries.Length != nextEntries.Length ||
                    catalog.entries.Where((e, i) => i >= nextEntries.Length || e.key != nextEntries[i].key || e.asset != nextEntries[i].asset).Any())
                {
                    catalog.entries = nextEntries; catalog.errors = errors.ToArray();
                    EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssetIfDirty(catalog);
                }
                var json = JsonUtility.ToJson(new Listing { valid = errors.Count == 0, assets = rows.ToArray(), errors = errors.ToArray() }, true) + "\n";
                if (!File.Exists(JsonPath) || File.ReadAllText(JsonPath) != json) File.WriteAllText(JsonPath, json);
                GcAssets.Reset();
                if (errors.Count > 0 && failOnError) throw new BuildFailedException(string.Join("\n", errors));
                foreach (var error in errors) Debug.LogWarning(error);
            }
            finally { refreshing = false; }
        }
        [Serializable] sealed class Row { public string key = "", type = "", path = ""; }
        [Serializable] sealed class Listing { public bool valid; public Row[] assets = Array.Empty<Row>(); public string[] errors = Array.Empty<string>(); }
    }
}
