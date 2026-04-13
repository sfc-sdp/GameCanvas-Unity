using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityBridge.Tools
{
    /// <summary>
    /// Handler for scene commands.
    /// Provides scene information, hierarchy traversal, loading, and saving.
    /// </summary>
    [BridgeTool("scene")]
    public static class Scene
    {
        private const int DefaultPageSize = 50;
        private const int DefaultDepth = 1;

        public static JObject HandleCommand(JObject parameters)
        {
            var action = parameters["action"]?.Value<string>() ?? "";

            return action.ToLowerInvariant() switch
            {
                "active" => GetActiveScene(),
                "hierarchy" => GetHierarchy(parameters),
                "load" => LoadScene(parameters),
                "save" => SaveScene(parameters),
                _ => throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    $"Unknown action: {action}. Valid actions: active, hierarchy, load, save")
            };
        }

        private static JObject GetActiveScene()
        {
            var scene = SceneManager.GetActiveScene();

            return new JObject
            {
                ["name"] = scene.name,
                ["path"] = scene.path,
                ["buildIndex"] = scene.buildIndex,
                ["isDirty"] = scene.isDirty,
                ["isLoaded"] = scene.isLoaded,
                ["rootCount"] = scene.rootCount
            };
        }

        private static JObject GetHierarchy(JObject parameters)
        {
            var depth = parameters["depth"]?.Value<int>() ?? DefaultDepth;
            var pageSize = parameters["page_size"]?.Value<int>() ?? DefaultPageSize;
            var cursor = parameters["cursor"]?.Value<int>() ?? 0;

            var scene = SceneManager.GetActiveScene();
            var rootObjects = scene.GetRootGameObjects();
            var totalRootCount = rootObjects.Length;

            var items = new List<JObject>();
            var currentIndex = 0;
            var collected = 0;

            foreach (var root in rootObjects)
            {
                if (currentIndex < cursor)
                {
                    currentIndex++;
                    continue;
                }

                if (collected >= pageSize)
                {
                    break;
                }

                CollectHierarchy(root, items, depth, 0, ref collected, pageSize);
                currentIndex++;
            }

            var hasMore = currentIndex < totalRootCount;

            return new JObject
            {
                ["items"] = JArray.FromObject(items),
                ["hasMore"] = hasMore,
                ["nextCursor"] = hasMore ? currentIndex : -1,
                ["totalRootCount"] = totalRootCount
            };
        }

        private static void CollectHierarchy(
            GameObject obj,
            List<JObject> items,
            int maxDepth,
            int currentDepth,
            ref int collected,
            int pageSize)
        {
            if (collected >= pageSize)
            {
                return;
            }

            items.Add(new JObject
            {
                ["name"] = obj.name,
                ["instanceID"] = obj.GetInstanceID(),
                ["childCount"] = obj.transform.childCount,
                ["activeSelf"] = obj.activeSelf,
                ["tag"] = obj.tag,
                ["layer"] = obj.layer,
                ["depth"] = currentDepth
            });

            collected++;

            if (currentDepth >= maxDepth)
            {
                return;
            }

            var transform = obj.transform;
            for (var i = 0; i < transform.childCount && collected < pageSize; i++)
            {
                CollectHierarchy(
                    transform.GetChild(i).gameObject,
                    items,
                    maxDepth,
                    currentDepth + 1,
                    ref collected,
                    pageSize);
            }
        }

        private static JObject LoadScene(JObject parameters)
        {
            var path = parameters["path"]?.Value<string>();
            var name = parameters["name"]?.Value<string>();
            var additive = parameters["additive"]?.Value<bool>() ?? false;

            if (string.IsNullOrEmpty(path) && string.IsNullOrEmpty(name))
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    "Either 'path' or 'name' parameter is required");
            }

            var mode = additive
                ? OpenSceneMode.Additive
                : OpenSceneMode.Single;

            UnityEngine.SceneManagement.Scene loadedScene;

            if (!string.IsNullOrEmpty(path))
            {
                if (!System.IO.File.Exists(path))
                {
                    throw new ProtocolException(
                        ErrorCode.InvalidParams,
                        $"Scene not found at path: {path}");
                }

                loadedScene = EditorSceneManager.OpenScene(path, mode);
            }
            else
            {
                var scenePath = FindSceneByName(name);
                if (string.IsNullOrEmpty(scenePath))
                {
                    throw new ProtocolException(
                        ErrorCode.InvalidParams,
                        $"Scene not found with name: {name}");
                }

                loadedScene = EditorSceneManager.OpenScene(scenePath, mode);
            }

            return new JObject
            {
                ["message"] = $"Scene loaded: {loadedScene.name}",
                ["name"] = loadedScene.name,
                ["path"] = loadedScene.path
            };
        }

        private static string FindSceneByName(string name)
        {
            var guids = AssetDatabase.FindAssets($"t:Scene {name}");

            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var sceneName = System.IO.Path.GetFileNameWithoutExtension(assetPath);

                if (sceneName == name)
                {
                    return assetPath;
                }
            }

            return null;
        }

        private static JObject SaveScene(JObject parameters)
        {
            var path = parameters["path"]?.Value<string>();
            var scene = SceneManager.GetActiveScene();

            if (string.IsNullOrEmpty(path) && string.IsNullOrEmpty(scene.path))
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    "Scene has no path. Specify '--path' to save to a new location.");
            }

            var saved = !string.IsNullOrEmpty(path)
                ? EditorSceneManager.SaveScene(scene, path)
                : EditorSceneManager.SaveScene(scene);

            if (!saved)
            {
                throw new ProtocolException(
                    ErrorCode.InternalError,
                    "Failed to save scene");
            }

            return new JObject
            {
                ["message"] = $"Scene saved: {scene.name}",
                ["name"] = scene.name,
                ["path"] = scene.path
            };
        }
    }
}
