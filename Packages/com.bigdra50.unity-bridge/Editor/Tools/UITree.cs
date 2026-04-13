using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityBridge.Helpers;

namespace UnityBridge.Tools
{
    /// <summary>
    /// Handler for UIToolkit VisualElement tree inspection commands.
    /// Provides dump, query, and inspect operations for UI panels.
    /// </summary>
    [BridgeTool("uitree")]
    public static class UITree
    {
        private const string SessionKeyEpoch = "UnityBridge.UITree.Epoch";

        private static readonly Dictionary<string, WeakReference<VisualElement>> RefMap = new();
        private static readonly ConditionalWeakTable<VisualElement, string> ElementToRef = new();
        private static readonly Dictionary<Type, PropertyInfo> ActualViewPropCache = new();
        private static readonly Dictionary<Type, (PropertyInfo contextType, PropertyInfo visualTree, PropertyInfo ownerObject)> PanelPropCache = new();
        private static int _epoch;
        private static int _nextSeq = 1;

        [InitializeOnLoadMethod]
        private static void OnDomainReload()
        {
            _epoch = SessionState.GetInt(SessionKeyEpoch, 0) + 1;
            SessionState.SetInt(SessionKeyEpoch, _epoch);
            _nextSeq = 1;
        }

        public static JObject HandleCommand(JObject parameters)
        {
            var action = parameters["action"]?.Value<string>() ?? "";

            return action.ToLowerInvariant() switch
            {
                "dump" => HandleDump(parameters),
                "query" => HandleQuery(parameters),
                "inspect" => HandleInspect(parameters),
                "click" => HandleClick(parameters),
                "scroll" => HandleScroll(parameters),
                "text" => HandleText(parameters),
                _ => throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    $"Unknown action: {action}. Valid actions: dump, query, inspect, click, scroll, text")
            };
        }

        #region Actions

        private static JObject HandleDump(JObject parameters)
        {
            var panelName = parameters["panel"]?.Value<string>();
            var depth = parameters["depth"]?.Value<int>() ?? -1;
            var format = parameters["format"]?.Value<string>() ?? "text";

            if (string.IsNullOrEmpty(panelName))
            {
                return ListPanels();
            }

            var (root, resolvedPanelName) = FindPanelRoot(panelName);
            if (root == null)
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    $"Panel not found: {panelName}");
            }

            PruneDeadRefs();

            var elementCount = 0;

            try
            {
                if (format.Equals("json", StringComparison.OrdinalIgnoreCase))
                {
                    var tree = BuildJsonTree(root, depth, 0, ref elementCount);
                    return new JObject
                    {
                        ["tree"] = tree,
                        ["elementCount"] = elementCount,
                        ["panel"] = resolvedPanelName
                    };
                }
                else
                {
                    var sb = new StringBuilder();
                    BuildTextTree(root, depth, 0, sb, ref elementCount);
                    return new JObject
                    {
                        ["tree"] = sb.ToString().TrimEnd(),
                        ["elementCount"] = elementCount,
                        ["panel"] = resolvedPanelName
                    };
                }
            }
            catch (Exception ex) when (ex is not ProtocolException)
            {
                throw new ProtocolException(
                    ErrorCode.InternalError,
                    $"Failed to traverse panel '{resolvedPanelName}': {ex.GetType().Name}: {ex.Message}");
            }
        }

        private static JObject HandleQuery(JObject parameters)
        {
            var panelName = parameters["panel"]?.Value<string>();
            if (string.IsNullOrEmpty(panelName))
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    "'panel' parameter is required for query action");
            }

            var typeFilter = parameters["type"]?.Value<string>();
            var nameFilter = parameters["name"]?.Value<string>();
            var classFilter = parameters["class_name"]?.Value<string>();

            // Strip leading # from name and . from class_name
            nameFilter = StripSelectorPrefix(nameFilter, '#');
            classFilter = StripSelectorPrefix(classFilter, '.');

            var (root, resolvedPanelName) = FindPanelRoot(panelName);
            if (root == null)
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    $"Panel not found: {panelName}");
            }

            PruneDeadRefs();

            try
            {
                var matches = new JArray();
                CollectMatches(root, typeFilter, nameFilter, classFilter, matches, "");

                return new JObject
                {
                    ["matches"] = matches,
                    ["count"] = matches.Count,
                    ["panel"] = resolvedPanelName
                };
            }
            catch (Exception ex) when (ex is not ProtocolException)
            {
                throw new ProtocolException(
                    ErrorCode.InternalError,
                    $"Failed to query panel '{resolvedPanelName}': {ex.GetType().Name}: {ex.Message}");
            }
        }

        private static JObject HandleInspect(JObject parameters)
        {
            var includeStyle = parameters["include_style"]?.Value<bool>() ?? false;
            var includeChildren = parameters["include_children"]?.Value<bool>() ?? false;

            var (target, elementRefId) = ResolveTarget(parameters);
            var result = BuildInspectResult(target, elementRefId, includeStyle, includeChildren);
            return result;
        }

        private static (VisualElement element, string refId) ResolveTarget(JObject parameters)
        {
            var refId = parameters["ref"]?.Value<string>();
            var panelName = parameters["panel"]?.Value<string>();
            var nameFilter = parameters["name"]?.Value<string>();

            VisualElement target;

            if (!string.IsNullOrEmpty(refId))
            {
                target = ResolveRef(refId);
                if (target == null)
                {
                    throw new ProtocolException(
                        ErrorCode.InvalidParams,
                        $"ref not found or element has been garbage collected: {refId}");
                }
            }
            else if (!string.IsNullOrEmpty(panelName) && !string.IsNullOrEmpty(nameFilter))
            {
                nameFilter = StripSelectorPrefix(nameFilter, '#');

                var (root, _) = FindPanelRoot(panelName);
                if (root == null)
                {
                    throw new ProtocolException(
                        ErrorCode.InvalidParams,
                        $"Panel not found: {panelName}");
                }

                target = FindElementByName(root, nameFilter);
                if (target == null)
                {
                    throw new ProtocolException(
                        ErrorCode.InvalidParams,
                        $"Element with name '{nameFilter}' not found in panel '{panelName}'");
                }
            }
            else
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    "Either 'ref' or 'panel' + 'name' parameters are required");
            }

            var elementRefId = FindOrAssignRef(target);
            return (target, elementRefId);
        }

        private static JObject HandleClick(JObject parameters)
        {
            var (target, refId) = ResolveTarget(parameters);
            var button = parameters["button"]?.Value<int>() ?? 0;
            var clickCount = parameters["click_count"]?.Value<int>() ?? 1;

            if (!target.enabledInHierarchy)
                throw new ProtocolException(ErrorCode.InvalidParams,
                    $"Element is disabled: {refId}");

            if (target.panel == null)
                throw new ProtocolException(ErrorCode.InvalidParams,
                    $"Element not connected to a panel: {refId}");

            var center = target.worldBound.center;

            var down = new Event
            {
                type = EventType.MouseDown,
                mousePosition = center,
                button = button,
                clickCount = clickCount
            };
            using (var pd = PointerDownEvent.GetPooled(down))
            {
                pd.target = target;
                target.panel.visualTree.SendEvent(pd);
            }

            var up = new Event
            {
                type = EventType.MouseUp,
                mousePosition = center,
                button = button,
                clickCount = clickCount
            };
            using (var pu = PointerUpEvent.GetPooled(up))
            {
                pu.target = target;
                target.panel.visualTree.SendEvent(pu);
            }

            var click = new Event
            {
                type = EventType.MouseUp,
                mousePosition = center,
                button = button,
                clickCount = clickCount
            };
            using (var ce = ClickEvent.GetPooled(click))
            {
                ce.target = target;
                target.panel.visualTree.SendEvent(ce);
            }

            return new JObject
            {
                ["ref"] = refId,
                ["type"] = target.GetType().Name,
                ["action"] = "click",
                ["message"] = "Clicked element"
            };
        }

        private static JObject HandleScroll(JObject parameters)
        {
            var (target, refId) = ResolveTarget(parameters);
            var scrollView = FindScrollView(target);

            if (scrollView == null)
                throw new ProtocolException(ErrorCode.InvalidParams,
                    $"No ScrollView found at or above element: {refId}");

            var toChild = parameters["to_child"]?.Value<string>();
            if (!string.IsNullOrEmpty(toChild))
            {
                var child = ResolveRef(toChild);
                if (child == null)
                    throw new ProtocolException(ErrorCode.InvalidParams,
                        $"to_child ref not found: {toChild}");

                scrollView.ScrollTo(child);
            }
            else
            {
                var offset = scrollView.scrollOffset;
                var x = parameters["x"];
                var y = parameters["y"];

                if (x == null && y == null)
                    throw new ProtocolException(ErrorCode.InvalidParams,
                        "Either 'x'/'y' or 'to_child' parameter is required for scroll");

                if (x != null) offset.x = x.Value<float>();
                if (y != null) offset.y = y.Value<float>();
                scrollView.scrollOffset = offset;
            }

            var svRefId = FindOrAssignRef(scrollView);
            var finalOffset = scrollView.scrollOffset;
            return new JObject
            {
                ["ref"] = svRefId,
                ["type"] = "ScrollView",
                ["action"] = "scroll",
                ["scrollOffset"] = new JObject
                {
                    ["x"] = finalOffset.x,
                    ["y"] = finalOffset.y
                },
                ["message"] = "Scrolled element"
            };
        }

        private static JObject HandleText(JObject parameters)
        {
            var (target, refId) = ResolveTarget(parameters);

            string text = null;

            if (target is TextElement textElement)
            {
                text = textElement.text;
            }
            else if (target is BaseField<string> stringField)
            {
                text = stringField.value;
            }
            else
            {
                var childText = target.Q<TextElement>();
                if (childText != null)
                    text = childText.text;
            }

            if (text == null)
                throw new ProtocolException(ErrorCode.InvalidParams,
                    $"Element has no text content: {refId} ({target.GetType().Name})");

            return new JObject
            {
                ["ref"] = refId,
                ["type"] = target.GetType().Name,
                ["action"] = "text",
                ["text"] = text
            };
        }

        #endregion

        #region Panel Discovery

        private static JObject ListPanels()
        {
            var panels = new JArray();
            var allPanels = ResolveAllPanels(countElements: true);

            foreach (var info in allPanels)
            {
                var panelObj = new JObject
                {
                    ["name"] = info.Name,
                    ["contextType"] = info.ContextType,
                    ["elementCount"] = info.ElementCount
                };
                if (info.WindowType != null)
                    panelObj["windowType"] = info.WindowType;
                panels.Add(panelObj);
            }

            return new JObject
            {
                ["panels"] = panels
            };
        }

        private static (VisualElement root, string panelName) FindPanelRoot(string panelName)
        {
            var panels = ResolveAllPanels(countElements: false);

            // Exact match (case-insensitive)
            foreach (var info in panels)
                if (info.Name.Equals(panelName, StringComparison.OrdinalIgnoreCase))
                    return (info.Root, info.Name);

            // Partial match fallback
            foreach (var info in panels)
                if (info.Name.IndexOf(panelName, StringComparison.OrdinalIgnoreCase) >= 0)
                    return (info.Root, info.Name);

            return (null, null);
        }

        private struct PanelInfo
        {
            public string Name;
            public string ContextType;
            public int ElementCount;
            public VisualElement Root;
            public string WindowType;
        }

        private static List<PanelInfo> GetEditorPanels(bool countElements = true)
        {
            var results = new List<PanelInfo>();

            // UIElementsUtility.GetPanelsIterator() is internal, use reflection
            var utilityType = Type.GetType(
                "UnityEngine.UIElements.UIElementsUtility, UnityEngine.UIElementsModule");
            if (utilityType == null)
            {
                BridgeLog.Verbose("UIElementsUtility type not found");
                return results;
            }

            var getIteratorMethod = utilityType.GetMethod(
                "GetPanelsIterator",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (getIteratorMethod == null)
            {
                BridgeLog.Verbose("GetPanelsIterator method not found");
                return results;
            }

            object iterator;
            try
            {
                iterator = getIteratorMethod.Invoke(null, null);
            }
            catch (Exception ex)
            {
                BridgeLog.Error($"Failed to get panels iterator: {ex.Message}");
                return results;
            }

            // The iterator is Dictionary<int, Panel>.Enumerator (a struct).
            // Wrap it in IEnumerator to avoid struct boxing issues with repeated MoveNext.
            var iteratorType = iterator.GetType();
            var moveNextMethod = iteratorType.GetMethod("MoveNext");
            var currentProp = iteratorType.GetProperty("Current");
            var disposeMethod = iteratorType.GetMethod("Dispose");

            if (moveNextMethod == null || currentProp == null)
                return results;

            try
            {
                // Use TypedReference / pointer trick not available, so collect all at once
                // by repeatedly calling MoveNext on the boxed struct via interface
                if (iterator is IEnumerator enumerator)
                {
                    while (enumerator.MoveNext())
                    {
                        ProcessPanelEntry(enumerator.Current, results, countElements);
                    }
                }
                else
                {
                    // Fallback: use a wrapper that handles the struct enumerator properly
                    // Box once and invoke via reflection on the same boxed reference
                    while ((bool)moveNextMethod.Invoke(iterator, null))
                    {
                        var kvp = currentProp.GetValue(iterator);
                        ProcessPanelEntry(kvp, results, countElements);
                    }
                }
            }
            finally
            {
                disposeMethod?.Invoke(iterator, null);
            }

            return results;
        }

        private static void ProcessPanelEntry(object kvp, List<PanelInfo> results, bool countElements)
        {
            if (kvp == null) return;

            var kvpType = kvp.GetType();
            var valueProp = kvpType.GetProperty("Value");
            var panel = valueProp?.GetValue(kvp);

            if (panel == null) return;

            var panelType = panel.GetType();
            var props = GetPanelProperties(panelType);

            var contextType = props.contextType?.GetValue(panel)?.ToString() ?? "Unknown";
            var visualTree = props.visualTree?.GetValue(panel) as VisualElement;

            if (visualTree == null) return;

            var ownerObject = props.ownerObject?.GetValue(panel) as ScriptableObject;

            var (panelName, windowType) = DeriveEnrichedPanelName(ownerObject, contextType);

            var elementCount = countElements ? CountElements(visualTree) : 0;

            results.Add(new PanelInfo
            {
                Name = panelName,
                ContextType = contextType,
                ElementCount = elementCount,
                Root = visualTree,
                WindowType = windowType
            });
        }

        private static (PropertyInfo contextType, PropertyInfo visualTree, PropertyInfo ownerObject) GetPanelProperties(Type panelType)
        {
            if (PanelPropCache.TryGetValue(panelType, out var cached))
                return cached;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var entry = (
                contextType: panelType.GetProperty("contextType", flags),
                visualTree: panelType.GetProperty("visualTree", flags),
                ownerObject: panelType.GetProperty("ownerObject", flags)
            );
            PanelPropCache[panelType] = entry;
            return entry;
        }

        private static (string name, string windowType) DeriveEnrichedPanelName(
            ScriptableObject ownerObject, string contextType)
        {
            if (ownerObject == null)
                return ($"Panel_{contextType}", null);

            var ownerTypeName = ownerObject.GetType().Name;
            string viewTypeName = null;

            try
            {
                var ownerType = ownerObject.GetType();
                if (!ActualViewPropCache.TryGetValue(ownerType, out var prop))
                {
                    prop = ownerType.GetProperty("actualView",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    ActualViewPropCache[ownerType] = prop;
                }

                if (prop != null)
                {
                    var view = prop.GetValue(ownerObject);
                    if (view != null)
                        viewTypeName = view.GetType().Name;
                }
            }
            catch (TargetInvocationException ex)
            {
                BridgeLog.Verbose($"actualView reflection failed for {ownerTypeName}: {ex.InnerException?.GetType().Name}");
            }
            catch (MemberAccessException ex)
            {
                BridgeLog.Verbose($"actualView access denied for {ownerTypeName}: {ex.GetType().Name}");
            }

            if (viewTypeName != null)
                return ($"{ownerTypeName}:{viewTypeName}", viewTypeName);

            return (ownerTypeName, null);
        }

        private static List<PanelInfo> GetRuntimePanels(bool countElements = true)
        {
            var results = new List<PanelInfo>();

            var uiDocuments = UnityEngine.Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None);

            foreach (var doc in uiDocuments)
            {
                if (doc == null || doc.rootVisualElement == null)
                    continue;

                var panelSettingsName = doc.panelSettings != null
                    ? doc.panelSettings.name
                    : "Unknown";
                var panelName = $"UIDocument {panelSettingsName} ({doc.gameObject.name})";
                var elementCount = countElements ? CountElements(doc.rootVisualElement) : 0;

                results.Add(new PanelInfo
                {
                    Name = panelName,
                    ContextType = "Player",
                    ElementCount = elementCount,
                    Root = doc.rootVisualElement
                });
            }

            return results;
        }

        private static List<PanelInfo> ResolveAllPanels(bool countElements = true)
        {
            var panels = GetEditorPanels(countElements);
            panels.AddRange(GetRuntimePanels(countElements));
            DeduplicateNames(panels);
            return panels;
        }

        private static void DeduplicateNames(List<PanelInfo> panels)
        {
            var nameCount = new Dictionary<string, int>(StringComparer.Ordinal);
            foreach (var p in panels)
            {
                nameCount.TryGetValue(p.Name, out var count);
                nameCount[p.Name] = count + 1;
            }

            var seen = new Dictionary<string, int>(StringComparer.Ordinal);
            for (var i = 0; i < panels.Count; i++)
            {
                var name = panels[i].Name;
                if (nameCount[name] <= 1)
                    continue;

                seen.TryGetValue(name, out var idx);
                idx++;
                seen[name] = idx;

                if (idx >= 2)
                {
                    var info = panels[i];
                    info.Name = $"{name}-{idx}";
                    panels[i] = info;
                }
            }
        }

        #endregion

        #region Tree Building

        private static void BuildTextTree(
            VisualElement element, int maxDepth, int currentDepth,
            StringBuilder sb, ref int elementCount)
        {
            if (maxDepth >= 0 && currentDepth > maxDepth)
                return;

            var indent = new string(' ', currentDepth * 2);
            var refId = FindOrAssignRef(element);
            elementCount++;

            sb.Append(indent);
            sb.Append(element.GetType().Name);

            if (!string.IsNullOrEmpty(element.name))
            {
                sb.Append($" \"{element.name}\"");
            }

            foreach (var cls in element.GetClasses())
            {
                sb.Append($" .{cls}");
            }

            sb.AppendLine($" {refId}");

            foreach (var child in element.Children())
            {
                BuildTextTree(child, maxDepth, currentDepth + 1, sb, ref elementCount);
            }
        }

        private static JObject BuildJsonTree(
            VisualElement element, int maxDepth, int currentDepth,
            ref int elementCount)
        {
            var refId = FindOrAssignRef(element);
            elementCount++;

            var node = new JObject
            {
                ["ref"] = refId,
                ["type"] = element.GetType().Name,
                ["name"] = string.IsNullOrEmpty(element.name) ? null : element.name,
                ["classes"] = new JArray(element.GetClasses().ToArray<object>()),
                ["childCount"] = element.childCount
            };

            if (maxDepth >= 0 && currentDepth >= maxDepth)
            {
                // Don't recurse further, but still report childCount
                return node;
            }

            if (element.childCount > 0)
            {
                var children = new JArray();
                foreach (var child in element.Children())
                {
                    children.Add(BuildJsonTree(child, maxDepth, currentDepth + 1, ref elementCount));
                }
                node["children"] = children;
            }

            return node;
        }

        #endregion

        #region Query

        private static void CollectMatches(
            VisualElement element, string typeFilter, string nameFilter,
            string classFilter, JArray matches, string parentPath)
        {
            var typeName = element.GetType().Name;
            var currentPath = string.IsNullOrEmpty(parentPath)
                ? typeName
                : $"{parentPath} > {typeName}";

            // Type filter uses partial match (IndexOf) for convenience (e.g., "Button" matches "RepeatButton")
            // Name and class filters use exact match (Equals) to match CSS selector semantics
            var matchesType = string.IsNullOrEmpty(typeFilter) ||
                              typeName.IndexOf(typeFilter, StringComparison.OrdinalIgnoreCase) >= 0;
            var matchesName = string.IsNullOrEmpty(nameFilter) ||
                              (!string.IsNullOrEmpty(element.name) &&
                               element.name.Equals(nameFilter, StringComparison.OrdinalIgnoreCase));
            var matchesClass = string.IsNullOrEmpty(classFilter) ||
                               element.GetClasses().Any(c =>
                                   c.Equals(classFilter, StringComparison.OrdinalIgnoreCase));

            if (matchesType && matchesName && matchesClass)
            {
                // At least one filter must be specified
                if (!string.IsNullOrEmpty(typeFilter) ||
                    !string.IsNullOrEmpty(nameFilter) ||
                    !string.IsNullOrEmpty(classFilter))
                {
                    var refId = FindOrAssignRef(element);
                    var layout = element.layout;
                    matches.Add(new JObject
                    {
                        ["ref"] = refId,
                        ["type"] = typeName,
                        ["name"] = string.IsNullOrEmpty(element.name) ? null : element.name,
                        ["classes"] = new JArray(element.GetClasses().ToArray<object>()),
                        ["path"] = currentPath,
                        ["layout"] = new JObject
                        {
                            ["x"] = layout.x,
                            ["y"] = layout.y,
                            ["width"] = layout.width,
                            ["height"] = layout.height
                        }
                    });
                }
            }

            foreach (var child in element.Children())
            {
                CollectMatches(child, typeFilter, nameFilter, classFilter, matches, currentPath);
            }
        }

        #endregion

        #region Inspect

        private static JObject BuildInspectResult(
            VisualElement element, string refId,
            bool includeStyle, bool includeChildren)
        {
            var layout = element.layout;
            var worldBound = element.worldBound;

            var result = new JObject
            {
                ["ref"] = refId,
                ["type"] = element.GetType().Name,
                ["name"] = string.IsNullOrEmpty(element.name) ? null : element.name,
                ["classes"] = new JArray(element.GetClasses().ToArray<object>()),
                ["visible"] = element.visible,
                ["enabledSelf"] = element.enabledSelf,
                ["enabledInHierarchy"] = element.enabledInHierarchy,
                ["focusable"] = element.focusable,
                ["tooltip"] = element.tooltip ?? "",
                ["path"] = BuildElementPath(element),
                ["layout"] = new JObject
                {
                    ["x"] = layout.x,
                    ["y"] = layout.y,
                    ["width"] = layout.width,
                    ["height"] = layout.height
                },
                ["worldBound"] = new JObject
                {
                    ["x"] = worldBound.x,
                    ["y"] = worldBound.y,
                    ["width"] = worldBound.width,
                    ["height"] = worldBound.height
                },
                ["childCount"] = element.childCount
            };

            if (includeChildren)
            {
                var children = new JArray();
                foreach (var child in element.Children())
                {
                    var childRefId = FindOrAssignRef(child);
                    children.Add(new JObject
                    {
                        ["ref"] = childRefId,
                        ["type"] = child.GetType().Name,
                        ["name"] = string.IsNullOrEmpty(child.name) ? null : child.name,
                        ["classes"] = new JArray(child.GetClasses().ToArray<object>())
                    });
                }
                result["children"] = children;
            }

            if (includeStyle)
            {
                result["resolvedStyle"] = BuildResolvedStyle(element);
            }

            return result;
        }

        private static JObject BuildResolvedStyle(VisualElement element)
        {
            var style = element.resolvedStyle;
            return new JObject
            {
                ["width"] = style.width,
                ["height"] = style.height,
                ["backgroundColor"] = style.backgroundColor.ToString(),
                ["color"] = style.color.ToString(),
                ["fontSize"] = style.fontSize,
                ["display"] = style.display.ToString(),
                ["position"] = style.position.ToString(),
                ["flexDirection"] = style.flexDirection.ToString(),
                ["opacity"] = style.opacity,
                ["visibility"] = style.visibility.ToString(),
                ["marginTop"] = style.marginTop,
                ["marginBottom"] = style.marginBottom,
                ["marginLeft"] = style.marginLeft,
                ["marginRight"] = style.marginRight,
                ["paddingTop"] = style.paddingTop,
                ["paddingBottom"] = style.paddingBottom,
                ["paddingLeft"] = style.paddingLeft,
                ["paddingRight"] = style.paddingRight,
                ["borderTopWidth"] = style.borderTopWidth,
                ["borderBottomWidth"] = style.borderBottomWidth,
                ["borderLeftWidth"] = style.borderLeftWidth,
                ["borderRightWidth"] = style.borderRightWidth
            };
        }

        private static string BuildElementPath(VisualElement element)
        {
            var parts = new List<string>();
            var current = element;

            while (current != null)
            {
                parts.Add(current.GetType().Name);
                current = current.parent;
            }

            parts.Reverse();
            return string.Join(" > ", parts);
        }

        #endregion

        #region Ref ID Management

        private static string FindOrAssignRef(VisualElement ve)
        {
            if (ElementToRef.TryGetValue(ve, out var refId))
                return refId;

            refId = $"ref_{_epoch}_{_nextSeq++}";
            RefMap[refId] = new WeakReference<VisualElement>(ve);
            ElementToRef.Add(ve, refId);
            return refId;
        }

        private static VisualElement ResolveRef(string refId)
        {
            ValidateRefEpoch(refId);

            if (!RefMap.TryGetValue(refId, out var weakRef) || !weakRef.TryGetTarget(out var element))
                return null;

            // Only check panel attachment (not enabledInHierarchy) -
            // callers like HandleClick add their own interaction-readiness checks
            if (element.panel == null)
            {
                RefMap.Remove(refId);
                return null;
            }

            return element;
        }

        private static void ValidateRefEpoch(string refId)
        {
            // ref format: ref_{epoch}_{seq}
            var parts = refId.Split('_');
            if (parts.Length != 3 || !int.TryParse(parts[1], out var refEpoch))
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    $"Invalid ref format: {refId} (hint: ref IDs were reset by domain reload, re-dump the UI tree)");
            }

            if (refEpoch != _epoch)
            {
                throw new ProtocolException(
                    ErrorCode.StaleRef,
                    $"Stale ref: {refId} belongs to epoch {refEpoch}, current epoch is {_epoch}. Re-dump the UI tree to get fresh refs.");
            }
        }

        private static void PruneDeadRefs()
        {
            var dead = new List<string>();
            foreach (var kvp in RefMap)
            {
                if (!kvp.Value.TryGetTarget(out _))
                    dead.Add(kvp.Key);
            }
            foreach (var key in dead)
                RefMap.Remove(key);
        }

        #endregion

        #region Helpers

        private static string StripSelectorPrefix(string value, char prefix)
        {
            if (value != null && value.Length > 0 && value[0] == prefix)
                return value.Substring(1);
            return value;
        }

        private static ScrollView FindScrollView(VisualElement element)
        {
            var current = element;
            while (current != null)
            {
                if (current is ScrollView sv) return sv;
                current = current.parent;
            }
            return null;
        }

        private static VisualElement FindElementByName(VisualElement root, string name)
        {
            if (!string.IsNullOrEmpty(root.name) &&
                root.name.Equals(name, StringComparison.OrdinalIgnoreCase))
                return root;

            foreach (var child in root.Children())
            {
                var found = FindElementByName(child, name);
                if (found != null)
                    return found;
            }

            return null;
        }

        private static int CountElements(VisualElement root)
        {
            var count = 1;
            foreach (var child in root.Children())
            {
                count += CountElements(child);
            }
            return count;
        }

        #endregion
    }
}
