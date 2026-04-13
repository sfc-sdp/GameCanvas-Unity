using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityBridge.Helpers
{
    /// <summary>
    /// Finds GameObjects by name or instanceID.
    /// Consolidates duplicate FindGameObject implementations from Asset.cs and Component.cs.
    /// Supports PrefabStage, inactive objects, and instanceID-to-Component resolution.
    /// </summary>
    internal static class GameObjectFinder
    {
        /// <summary>
        /// Find a GameObject by name or instanceID.
        /// If instanceID resolves to a Component, returns its gameObject.
        /// </summary>
        public static GameObject Find(string name, int? instanceId)
        {
            if (instanceId.HasValue)
            {
                var obj = EditorUtility.InstanceIDToObject(instanceId.Value);
                if (obj is GameObject go)
                    return go;
                if (obj is Component comp)
                    return comp.gameObject;
                return null;
            }

            if (!string.IsNullOrEmpty(name))
            {
                return FindByName(name);
            }

            return null;
        }

        /// <summary>
        /// Find a GameObject by name. Searches:
        /// 1. Current PrefabStage (if open)
        /// 2. Active scene root objects and all children (including inactive)
        /// </summary>
        private static GameObject FindByName(string name)
        {
            // Check prefab stage first
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage?.prefabContentsRoot != null)
            {
                foreach (var transform in prefabStage.prefabContentsRoot
                             .GetComponentsInChildren<Transform>(true))
                {
                    if (transform.name == name)
                        return transform.gameObject;
                }
            }

            // Search in active scene (including inactive objects)
            var activeScene = SceneManager.GetActiveScene();
            foreach (var root in activeScene.GetRootGameObjects())
            {
                foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                {
                    if (transform.gameObject.name == name)
                        return transform.gameObject;
                }
            }

            return null;
        }
    }
}
