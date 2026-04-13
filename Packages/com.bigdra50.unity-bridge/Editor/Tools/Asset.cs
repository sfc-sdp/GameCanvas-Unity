using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityBridge.Helpers;
using UnityEditor;
using UnityEngine;

namespace UnityBridge.Tools
{
    /// <summary>
    /// Handler for asset commands.
    /// Creates and manages Unity assets (Prefabs, ScriptableObjects, etc.)
    /// </summary>
    [BridgeTool("asset")]
    public static class Asset
    {
        public static JObject HandleCommand(JObject parameters)
        {
            var action = parameters["action"]?.Value<string>() ?? "";

            return action.ToLowerInvariant() switch
            {
                "create_prefab" => CreatePrefab(parameters),
                "create_scriptable_object" => CreateScriptableObject(parameters),
                "info" => GetAssetInfo(parameters),
                "deps" => GetDependencies(parameters),
                "refs" => GetReferencers(parameters),
                _ => throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    $"Unknown action: {action}. Valid: create_prefab, create_scriptable_object, info, deps, refs")
            };
        }

        private static JObject CreatePrefab(JObject parameters)
        {
            var sourceName = parameters["source"]?.Value<string>();
            var sourceId = parameters["sourceId"]?.Value<int>();
            var path = parameters["path"]?.Value<string>();

            if (string.IsNullOrEmpty(sourceName) && sourceId == null)
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    "Either 'source' (name) or 'sourceId' (instanceID) is required");
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    "'path' is required (e.g., 'Assets/Prefabs/MyPrefab.prefab')");
            }

            if (!path.EndsWith(".prefab"))
            {
                path += ".prefab";
            }

            var source = FindGameObject(sourceName, sourceId);
            if (source == null)
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    $"Source GameObject not found: {sourceName ?? sourceId.Value.ToString()}");
            }

            // Ensure directory exists
            var directory = System.IO.Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !AssetDatabase.IsValidFolder(directory))
            {
                CreateFolderRecursively(directory);
            }

            var prefab = PrefabUtility.SaveAsPrefabAsset(source, path);
            if (prefab == null)
            {
                throw new ProtocolException(
                    ErrorCode.InternalError,
                    $"Failed to create prefab at: {path}");
            }

            return new JObject
            {
                ["message"] = $"Prefab created: {path}",
                ["path"] = path,
                ["assetGuid"] = AssetDatabase.AssetPathToGUID(path)
            };
        }

        private static JObject CreateScriptableObject(JObject parameters)
        {
            var typeName = parameters["type"]?.Value<string>();
            var path = parameters["path"]?.Value<string>();

            if (string.IsNullOrEmpty(typeName))
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    "'type' is required (ScriptableObject type name)");
            }

            if (string.IsNullOrEmpty(path))
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    "'path' is required (e.g., 'Assets/Data/MyData.asset')");
            }

            if (!path.EndsWith(".asset"))
            {
                path += ".asset";
            }

            var type = FindType(typeName);
            if (type == null)
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    $"Type not found: {typeName}");
            }

            if (!typeof(ScriptableObject).IsAssignableFrom(type))
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    $"Type '{typeName}' is not a ScriptableObject");
            }

            // Ensure directory exists
            var directory = System.IO.Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !AssetDatabase.IsValidFolder(directory))
            {
                CreateFolderRecursively(directory);
            }

            var asset = ScriptableObject.CreateInstance(type);
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();

            return new JObject
            {
                ["message"] = $"ScriptableObject created: {path}",
                ["path"] = path,
                ["type"] = type.FullName,
                ["assetGuid"] = AssetDatabase.AssetPathToGUID(path)
            };
        }

        private static JObject GetAssetInfo(JObject parameters)
        {
            var path = parameters["path"]?.Value<string>();

            if (string.IsNullOrEmpty(path))
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    "'path' is required");
            }

            var asset = AssetDatabase.LoadMainAssetAtPath(path);
            if (asset == null)
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    $"Asset not found: {path}");
            }

            return new JObject
            {
                ["path"] = path,
                ["name"] = asset.name,
                ["type"] = asset.GetType().FullName,
                ["instanceID"] = asset.GetInstanceID(),
                ["guid"] = AssetDatabase.AssetPathToGUID(path)
            };
        }

        private static JObject GetDependencies(JObject parameters)
        {
            var path = parameters["path"]?.Value<string>();
            var recursive = parameters["recursive"]?.Value<bool>() ?? true;

            if (string.IsNullOrEmpty(path))
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    "'path' is required");
            }

            if (!AssetPathExists(path))
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    $"Asset not found: {path}");
            }

            var deps = AssetDatabase.GetDependencies(path, recursive)
                .Where(p => p != path)
                .Select(BuildAssetInfo)
                .ToArray<object>();

            return new JObject
            {
                ["path"] = path,
                ["recursive"] = recursive,
                ["count"] = deps.Length,
                ["dependencies"] = new JArray(deps)
            };
        }

        private static JObject GetReferencers(JObject parameters)
        {
            var path = parameters["path"]?.Value<string>();

            if (string.IsNullOrEmpty(path))
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    "'path' is required");
            }

            if (!AssetPathExists(path))
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    $"Asset not found: {path}");
            }

            var allAssets = AssetDatabase.GetAllAssetPaths();

            var referencers = allAssets
                .Where(assetPath => assetPath != path)
                .Where(assetPath => AssetDatabase.GetDependencies(assetPath, false).Contains(path))
                .Select(BuildAssetInfo)
                .ToArray<object>();

            return new JObject
            {
                ["path"] = path,
                ["count"] = referencers.Length,
                ["referencers"] = new JArray(referencers)
            };
        }

        private static JObject BuildAssetInfo(string assetPath)
        {
            var asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
            return new JObject
            {
                ["path"] = assetPath,
                ["guid"] = AssetDatabase.AssetPathToGUID(assetPath),
                ["type"] = asset?.GetType().FullName ?? "Unknown"
            };
        }

        private static GameObject FindGameObject(string name, int? instanceId)
            => GameObjectFinder.Find(name, instanceId);

        private static Type FindType(string typeName)
            => TypeResolver.FindType(typeName);

        private static bool AssetPathExists(string path)
        {
#if UNITY_6000_0_OR_NEWER
            return AssetDatabase.AssetPathExists(path);
#else
            return AssetDatabase.GetMainAssetTypeAtPath(path) != null;
#endif
        }

        private static void CreateFolderRecursively(string path)
        {
            var parts = path.Split('/');
            var current = parts[0]; // "Assets"

            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }
    }
}
