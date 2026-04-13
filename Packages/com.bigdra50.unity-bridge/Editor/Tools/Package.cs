using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace UnityBridge.Tools
{
    [BridgeTool("package")]
    public static class Package
    {
        private const int DefaultTimeoutMs = 60000;

        public static JObject HandleCommand(JObject parameters)
        {
            var action = parameters["action"]?.Value<string>() ?? "";

            return action.ToLowerInvariant() switch
            {
                "list" => ListPackages(),
                "add" => AddPackage(parameters),
                "remove" => RemovePackage(parameters),
                _ => throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    $"Unknown action: {action}. Valid: list, add, remove")
            };
        }

        private static JObject ListPackages()
        {
            var request = Client.List(true);
            WaitForRequest(request);

            var packages = request.Result
                .Select(p => new JObject
                {
                    ["name"] = p.name,
                    ["version"] = p.version,
                    ["displayName"] = p.displayName,
                    ["source"] = p.source.ToString()
                })
                .ToArray<object>();

            return new JObject
            {
                ["count"] = packages.Length,
                ["packages"] = new JArray(packages)
            };
        }

        private static JObject AddPackage(JObject parameters)
        {
            var name = parameters["name"]?.Value<string>();
            if (string.IsNullOrEmpty(name))
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    "'name' is required (e.g., 'com.unity.textmeshpro@3.0.6')");
            }

            var request = Client.Add(name);
            WaitForRequest(request);

            var info = request.Result;
            return new JObject
            {
                ["message"] = $"Package added: {info.name}@{info.version}",
                ["name"] = info.name,
                ["version"] = info.version,
                ["displayName"] = info.displayName,
                ["source"] = info.source.ToString()
            };
        }

        private static JObject RemovePackage(JObject parameters)
        {
            var name = parameters["name"]?.Value<string>();
            if (string.IsNullOrEmpty(name))
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    "'name' is required (e.g., 'com.unity.textmeshpro')");
            }

            var request = Client.Remove(name);
            WaitForRequest(request);

            return new JObject
            {
                ["message"] = $"Package removed: {name}"
            };
        }

        private static void WaitForRequest(Request request, int timeoutMs = DefaultTimeoutMs)
        {
            var sw = Stopwatch.StartNew();
            while (!request.IsCompleted)
            {
                if (sw.ElapsedMilliseconds > timeoutMs)
                    throw new ProtocolException(ErrorCode.Timeout, "Package operation timed out");
                System.Threading.Thread.Sleep(100);
            }

            if (request.Status == StatusCode.Failure)
                throw new ProtocolException(ErrorCode.InternalError, request.Error.message);
        }
    }
}
