using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityBridge.Helpers;
using UnityEditor;

namespace UnityBridge.Tools
{
    [BridgeTool("api-schema")]
    public static class ApiSchema
    {
        private static List<MethodInfo> _allMethodsCache;

        [InitializeOnLoadMethod]
        private static void RegisterReloadCallback()
        {
            AssemblyReloadEvents.beforeAssemblyReload += () => _allMethodsCache = null;
        }

        public static JObject HandleCommand(JObject parameters)
        {
            var nsFilter = parameters["namespace"]?.ToObject<string[]>();
            var typeFilter = parameters["type"]?.Value<string>();
            var methodFilter = parameters["method"]?.Value<string>();
            var cacheAll = parameters["cache_all"]?.Value<bool>() ?? false;
            var limit = parameters["limit"]?.Value<int>() ?? 100;
            var offset = parameters["offset"]?.Value<int>() ?? 0;

            var allMethods = FilterMethods(GetOrBuildCache(), nsFilter, typeFilter, methodFilter);
            var total = allMethods.Count;
            var page = cacheAll ? allMethods : allMethods.Skip(offset).Take(limit).ToList();

            var methodsArray = new JArray();
            foreach (var mi in page)
            {
                var paramsArray = new JArray();
                foreach (var p in mi.GetParameters())
                {
                    paramsArray.Add(new JObject
                    {
                        ["name"] = p.Name,
                        ["type"] = p.ParameterType.Name,
                        ["hasDefault"] = p.HasDefaultValue
                    });
                }

                methodsArray.Add(new JObject
                {
                    ["type"] = mi.DeclaringType?.FullName,
                    ["method"] = mi.Name,
                    ["returnType"] = mi.ReturnType.Name,
                    ["parameters"] = paramsArray
                });
            }

            return new JObject
            {
                ["methods"] = methodsArray,
                ["total"] = total,
                ["hasMore"] = offset + limit < total
            };
        }

        private static List<MethodInfo> GetOrBuildCache()
        {
            if (_allMethodsCache != null) return _allMethodsCache;
            _allMethodsCache = ScanAllMethods();
            return _allMethodsCache;
        }

        private static List<MethodInfo> FilterMethods(
            List<MethodInfo> source, string[] nsFilter, string typeFilter, string methodFilter)
        {
            IEnumerable<MethodInfo> results = source;

            if (nsFilter != null && nsFilter.Length > 0)
            {
                results = results.Where(m =>
                {
                    var ns = m.DeclaringType?.Namespace ?? "";
                    return nsFilter.Any(f => ns.StartsWith(f, StringComparison.OrdinalIgnoreCase));
                });
            }

            if (!string.IsNullOrEmpty(typeFilter))
            {
                results = results.Where(m =>
                    m.DeclaringType != null &&
                    (m.DeclaringType.Name.Equals(typeFilter, StringComparison.OrdinalIgnoreCase)
                     || (m.DeclaringType.FullName?.Equals(typeFilter, StringComparison.OrdinalIgnoreCase) ?? false)));
            }

            if (!string.IsNullOrEmpty(methodFilter))
            {
                results = results.Where(m => m.Name.Equals(methodFilter, StringComparison.OrdinalIgnoreCase));
            }

            return results.ToList();
        }

        private static List<MethodInfo> ScanAllMethods()
        {
            var results = new List<MethodInfo>();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types.Where(t => t != null).ToArray();
                }

                foreach (var type in types)
                {
                    if (!type.IsPublic) continue;
                    if (!ApiSafetyGuard.IsNamespaceAllowed(type)) continue;
                    if (ApiSafetyGuard.IsObsolete(type)) continue;

                    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
                    foreach (var mi in methods)
                    {
                        if (ApiSafetyGuard.IsObsolete(mi)) continue;
                        if (!ApiSafetyGuard.HasAllSupportedParams(mi)) continue;
                        if (mi.IsGenericMethod) continue;
                        results.Add(mi);
                    }
                }
            }

            return results.OrderBy(m => m.DeclaringType?.FullName).ThenBy(m => m.Name).ToList();
        }
    }
}
