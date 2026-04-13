using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityBridge.Tools
{
    /// <summary>
    /// Handler for selection commands.
    /// Provides information about currently selected objects in the editor.
    /// </summary>
    [BridgeTool("selection")]
    public static class EditorSelection
    {
        public static JObject HandleCommand(JObject parameters)
        {
            var action = parameters["action"]?.Value<string>() ?? "get";

            return action.ToLowerInvariant() switch
            {
                "get" => GetSelection(),
                _ => throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    $"Unknown action: {action}. Valid actions: get")
            };
        }

        private static JObject GetSelection()
        {
            var activeObject = Selection.activeObject;
            var activeGameObject = Selection.activeGameObject;
            var activeTransform = Selection.activeTransform;

            var selectedObjects = Selection.objects;
            var selectedGameObjects = Selection.gameObjects;

            var result = new JObject
            {
                ["count"] = Selection.count,
                ["activeObject"] = activeObject != null
                    ? SerializeObject(activeObject)
                    : null,
                ["activeGameObject"] = activeGameObject != null
                    ? SerializeGameObject(activeGameObject)
                    : null,
                ["activeInstanceID"] = Selection.activeInstanceID,
                ["objects"] = new JArray(
                    selectedObjects.Select(SerializeObject)),
                ["gameObjects"] = new JArray(
                    selectedGameObjects.Select(SerializeGameObject)),
                ["assetGUIDs"] = new JArray(Selection.assetGUIDs.ToArray<object>())
            };

            if (activeTransform != null)
            {
                result["activeTransform"] = new JObject
                {
                    ["position"] = SerializeVector3(activeTransform.position),
                    ["rotation"] = SerializeVector3(activeTransform.eulerAngles),
                    ["scale"] = SerializeVector3(activeTransform.localScale)
                };
            }

            return result;
        }

        private static JObject SerializeObject(Object obj)
        {
            if (obj == null) return null;

            var result = new JObject
            {
                ["name"] = obj.name,
                ["instanceID"] = obj.GetInstanceID(),
                ["type"] = obj.GetType().Name
            };

            var assetPath = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(assetPath))
            {
                result["assetPath"] = assetPath;
            }

            return result;
        }

        private static JObject SerializeGameObject(GameObject go)
        {
            if (go == null) return null;

            return new JObject
            {
                ["name"] = go.name,
                ["instanceID"] = go.GetInstanceID(),
                ["tag"] = go.tag,
                ["layer"] = go.layer,
                ["layerName"] = LayerMask.LayerToName(go.layer),
                ["activeSelf"] = go.activeSelf,
                ["activeInHierarchy"] = go.activeInHierarchy,
                ["isStatic"] = go.isStatic,
                ["scenePath"] = GetScenePath(go),
                ["componentCount"] = go.GetComponents(typeof(UnityEngine.Component)).Length
            };
        }

        private static string GetScenePath(GameObject go)
        {
            var path = go.name;
            var parent = go.transform.parent;

            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }

        private static JArray SerializeVector3(Vector3 v)
        {
            return new JArray(v.x, v.y, v.z);
        }
    }
}
