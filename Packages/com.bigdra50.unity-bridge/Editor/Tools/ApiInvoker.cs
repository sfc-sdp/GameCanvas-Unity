using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityBridge.Helpers;

namespace UnityBridge.Tools
{
    [BridgeTool("api-invoke")]
    public static class ApiInvoker
    {
        public static JObject HandleCommand(JObject parameters)
        {
            var typeName = parameters["type"]?.Value<string>();
            var methodName = parameters["method"]?.Value<string>();
            var args = parameters["params"] as JArray;

            if (string.IsNullOrEmpty(typeName))
                throw new ProtocolException(ErrorCode.InvalidParams, "'type' parameter is required");
            if (string.IsNullOrEmpty(methodName))
                throw new ProtocolException(ErrorCode.InvalidParams, "'method' parameter is required");

            // Resolve type
            var type = TypeResolver.FindType(typeName);
            if (type == null)
                throw new ProtocolException(ErrorCode.InvalidParams, $"Type not found: {typeName}");

            // Safety check
            if (!ApiSafetyGuard.IsNamespaceAllowed(type))
                throw new ProtocolException(ErrorCode.InvalidParams,
                    $"Type '{type.FullName}' is in a disallowed namespace. Only UnityEngine and UnityEditor namespaces are permitted.");

            if (ApiSafetyGuard.IsObsolete(type))
                throw new ProtocolException(ErrorCode.InvalidParams,
                    $"Type '{type.FullName}' is marked as [Obsolete]");

            // Find method
            var argCount = args?.Count ?? 0;
            var candidates = type
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == methodName)
                .Where(m => !m.IsGenericMethod)
                .Where(m => !ApiSafetyGuard.IsObsolete(m))
                .Where(m => ApiSafetyGuard.HasAllSupportedParams(m))
                .ToList();

            if (candidates.Count == 0)
                throw new ProtocolException(ErrorCode.InvalidParams,
                    $"No matching public static method '{methodName}' found on {type.FullName}");

            // Match by parameter count (considering default params)
            var matched = candidates
                .Where(m =>
                {
                    var ps = m.GetParameters();
                    var required = ps.Count(p => !p.HasDefaultValue);
                    return argCount >= required && argCount <= ps.Length;
                })
                .ToList();

            if (matched.Count == 0)
            {
                var sigs = string.Join("\n  ", candidates.Select(ApiSafetyGuard.FormatSignature));
                throw new ProtocolException(ErrorCode.InvalidParams,
                    $"No overload of '{methodName}' matches {argCount} argument(s). Available:\n  {sigs}");
            }

            if (matched.Count > 1)
            {
                var sigs = string.Join("\n  ", matched.Select(ApiSafetyGuard.FormatSignature));
                throw new ProtocolException(ErrorCode.InvalidParams,
                    $"Ambiguous overload for '{methodName}' with {argCount} argument(s). Candidates:\n  {sigs}");
            }

            var method = matched[0];

            // Deserialize arguments
            object[] deserializedArgs;
            try
            {
                deserializedArgs = ApiSafetyGuard.DeserializeArgs(method, args);
            }
            catch (ArgumentException ex)
            {
                throw new ProtocolException(ErrorCode.InvalidParams, ex.Message);
            }

            // Invoke
            BridgeLog.Info($"api-invoke: {type.FullName}.{methodName}({argCount} args)");

            object result;
            try
            {
                result = method.Invoke(null, deserializedArgs);
            }
            catch (TargetInvocationException ex)
            {
                var inner = ex.InnerException ?? ex;
                throw new ProtocolException(ErrorCode.InternalError,
                    $"{type.FullName}.{methodName} threw {inner.GetType().Name}: {inner.Message}");
            }

            return new JObject
            {
                ["type"] = type.FullName,
                ["method"] = methodName,
                ["returnType"] = method.ReturnType.Name,
                ["result"] = ApiSafetyGuard.SerializeReturnValue(result, method.ReturnType)
            };
        }
    }
}
