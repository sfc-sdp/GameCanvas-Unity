using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace UnityBridge.Helpers
{
    internal static class BridgeJobStateStore
    {
        private static string GetStatePath(string toolName)
        {
            if (string.IsNullOrEmpty(toolName))
            {
                throw new ArgumentException("toolName cannot be null or empty", nameof(toolName));
            }

            var libraryPath = Path.Combine(Application.dataPath, "..", "Library");
            var fileName = $"UnityBridgeState_{toolName}.json";
            return Path.GetFullPath(Path.Combine(libraryPath, fileName));
        }

        public static void SaveState<T>(string toolName, T state)
        {
            var path = GetStatePath(toolName);
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            var json = JsonConvert.SerializeObject(state ?? Activator.CreateInstance<T>());
            File.WriteAllText(path, json);
        }

        public static T LoadState<T>(string toolName)
        {
            var path = GetStatePath(toolName);
            if (!File.Exists(path))
            {
                return default;
            }

            try
            {
                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (JsonException ex)
            {
                BridgeLog.Warn($"[BridgeJobStateStore] Failed to load state for '{toolName}': {ex.Message}");
                return default;
            }
        }

        public static void ClearState(string toolName)
        {
            var path = GetStatePath(toolName);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public static bool HasState(string toolName)
        {
            var path = GetStatePath(toolName);
            return File.Exists(path);
        }
    }
}
