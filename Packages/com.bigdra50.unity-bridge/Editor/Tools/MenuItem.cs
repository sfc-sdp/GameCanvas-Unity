using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityBridge.Tools
{
    /// <summary>
    /// Handler for menu item commands.
    /// Executes arbitrary MenuItems and ContextMenus via reflection.
    /// </summary>
    [BridgeTool("menu")]
    public static class MenuItem
    {
        public static JObject HandleCommand(JObject parameters)
        {
            var action = parameters["action"]?.ToString() ?? "execute";

            return action switch
            {
                "execute" => ExecuteMenuItem(parameters),
                "list" => ListMenuItems(parameters),
                "context" => ExecuteContextMenu(parameters),
                _ => throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    $"Unknown action: {action}. Valid actions: execute, list, context")
            };
        }

        private static JObject ExecuteMenuItem(JObject parameters)
        {
            var path = parameters["path"]?.ToString();
            if (string.IsNullOrEmpty(path))
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    "'path' is required for execute action");
            }

            var result = EditorApplication.ExecuteMenuItem(path);

            return new JObject
            {
                ["success"] = result,
                ["path"] = path,
                ["message"] = result
                    ? $"Executed: {path}"
                    : $"Failed to execute: {path} (menu not found or validation failed)"
            };
        }

        private static JObject ListMenuItems(JObject parameters)
        {
            var filter = parameters["filter"]?.ToString();
            var limit = parameters["limit"]?.ToObject<int>() ?? 100;

            var menuItems = CollectMenuItems(filter, limit);

            return new JObject
            {
                ["count"] = menuItems.Count,
                ["items"] = new JArray(menuItems.Select(m => new JObject
                {
                    ["path"] = m.Path,
                    ["priority"] = m.Priority,
                    ["shortcut"] = m.Shortcut,
                    ["type"] = m.DeclaringType
                }))
            };
        }

        private static JObject ExecuteContextMenu(JObject parameters)
        {
            var methodName = parameters["method"]?.ToString();
            var targetPath = parameters["target"]?.ToString();

            if (string.IsNullOrEmpty(methodName))
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    "'method' is required for context action");
            }

            UnityEngine.Object targetObject = null;

            if (!string.IsNullOrEmpty(targetPath))
            {
                // Try to find by hierarchy path
                var go = GameObject.Find(targetPath);
                targetObject = go != null ? go :
                    // Try asset path
                    AssetDatabase.LoadMainAssetAtPath(targetPath);

                if (targetObject == null)
                {
                    throw new ProtocolException(
                        ErrorCode.InvalidParams,
                        $"Target not found: {targetPath}");
                }
            }

            var result = InvokeContextMenu(methodName, targetObject);
            return result;
        }

        private static JObject InvokeContextMenu(string methodName, UnityEngine.Object target)
        {
            // Search for ContextMenu attribute in all loaded assemblies
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var candidates = new List<(MethodInfo Method, Type Type, string MenuName)>();

            foreach (var assembly in assemblies)
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (!typeof(MonoBehaviour).IsAssignableFrom(type) &&
                            !typeof(ScriptableObject).IsAssignableFrom(type))
                            continue;

                        foreach (var method in type.GetMethods(
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                        {
                            var attr = method.GetCustomAttribute<ContextMenu>();
                            if (attr == null) continue;

                            if (attr.menuItem == methodName ||
                                method.Name == methodName)
                            {
                                candidates.Add((method, type, attr.menuItem));
                            }
                        }
                    }
                }
                catch
                {
                    // Skip assemblies that fail to load types
                }
            }

            if (candidates.Count == 0)
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    $"ContextMenu not found: {methodName}");
            }

            // If target is a GameObject, search for components with matching ContextMenu
            if (target is GameObject go)
            {
                foreach (var candidate in candidates)
                {
                    var component = go.GetComponent(candidate.Type);
                    if (component != null)
                    {
                        try
                        {
                            candidate.Method.Invoke(component, null);
                            return new JObject
                            {
                                ["success"] = true,
                                ["method"] = candidate.Method.Name,
                                ["menu"] = candidate.MenuName,
                                ["target"] = go.name,
                                ["component"] = candidate.Type.Name
                            };
                        }
                        catch (Exception ex)
                        {
                            throw new ProtocolException(
                                ErrorCode.InternalError,
                                $"Failed to invoke: {ex.InnerException?.Message ?? ex.Message}");
                        }
                    }
                }

                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    $"ContextMenu '{methodName}' not found on any component of '{go.name}'");
            }

            // If target is a Component or ScriptableObject
            if (target != null)
            {
                var targetType = target.GetType();
                var match = candidates.FirstOrDefault(c =>
                    c.Type.IsAssignableFrom(targetType) || targetType.IsAssignableFrom(c.Type));

                if (match.Method == null)
                {
                    throw new ProtocolException(
                        ErrorCode.InvalidParams,
                        $"ContextMenu '{methodName}' not found on type {targetType.Name}");
                }

                try
                {
                    match.Method.Invoke(target, null);
                    return new JObject
                    {
                        ["success"] = true,
                        ["method"] = match.Method.Name,
                        ["menu"] = match.MenuName,
                        ["target"] = target.name
                    };
                }
                catch (Exception ex)
                {
                    throw new ProtocolException(
                        ErrorCode.InternalError,
                        $"Failed to invoke: {ex.InnerException?.Message ?? ex.Message}");
                }
            }

            // No target: try to invoke on selected objects
            var selection = Selection.objects;
            if (selection.Length == 0)
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    "No target specified and nothing selected. Provide 'target' parameter or select an object in Unity.");
            }

            var results = new JArray();
            foreach (var obj in selection)
            {
                var objType = obj.GetType();
                var match = candidates.FirstOrDefault(c =>
                    c.Type.IsAssignableFrom(objType) || objType.IsAssignableFrom(c.Type));

                if (match.Method != null)
                {
                    try
                    {
                        match.Method.Invoke(obj, null);
                        results.Add(new JObject
                        {
                            ["success"] = true,
                            ["target"] = obj.name
                        });
                    }
                    catch (Exception ex)
                    {
                        results.Add(new JObject
                        {
                            ["success"] = false,
                            ["target"] = obj.name,
                            ["error"] = ex.InnerException?.Message ?? ex.Message
                        });
                    }
                }
            }

            return new JObject
            {
                ["results"] = results,
                ["processed"] = results.Count
            };
        }

        private record MenuItemInfo(string Path, int Priority, string Shortcut, string DeclaringType);

        private static List<MenuItemInfo> CollectMenuItems(string filter, int limit)
        {
            var items = new List<MenuItemInfo>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        foreach (var method in type.GetMethods(
                            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                        {
                            var attr = method.GetCustomAttribute<UnityEditor.MenuItem>();
                            if (attr == null) continue;

                            // Skip validation methods
                            if (attr.validate) continue;

                            var path = attr.menuItem;

                            // Apply filter
                            if (!string.IsNullOrEmpty(filter) &&
                                !path.Contains(filter, StringComparison.OrdinalIgnoreCase))
                                continue;

                            // Extract shortcut from path (e.g., "Edit/Play %P")
                            var shortcut = "";
                            var spaceIdx = path.LastIndexOf(' ');
                            if (spaceIdx > 0)
                            {
                                var possibleShortcut = path.Substring(spaceIdx + 1);
                                if (possibleShortcut.StartsWith("%") ||
                                    possibleShortcut.StartsWith("#") ||
                                    possibleShortcut.StartsWith("&") ||
                                    possibleShortcut.StartsWith("_"))
                                {
                                    shortcut = possibleShortcut;
                                    path = path.Substring(0, spaceIdx);
                                }
                            }

                            items.Add(new MenuItemInfo(
                                path,
                                attr.priority,
                                shortcut,
                                type.FullName ?? type.Name
                            ));

                            if (items.Count >= limit) break;
                        }
                        if (items.Count >= limit) break;
                    }
                }
                catch
                {
                    // Skip assemblies that fail to load
                }
                if (items.Count >= limit) break;
            }

            return items.OrderBy(i => i.Path).ToList();
        }
    }
}
