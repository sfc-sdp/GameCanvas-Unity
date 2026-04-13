using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace UnityBridge.Helpers
{
    /// <summary>
    /// Manages status file for reliable domain reload notification.
    /// Status file is used as fallback when TCP notification fails.
    /// </summary>
    internal static class BridgeStatusFile
    {
        private const string StatusDirName = ".unity-bridge";
        private static int _seq;

        /// <summary>
        /// Get status file directory.
        /// Uses UNITY_BRIDGE_STATUS_DIR environment variable if set, otherwise ~/.unity-bridge.
        /// </summary>
        internal static string GetStatusDir()
        {
            var envDir = Environment.GetEnvironmentVariable("UNITY_BRIDGE_STATUS_DIR");
            if (!string.IsNullOrWhiteSpace(envDir))
                return envDir;

            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, StatusDirName);
        }

        /// <summary>
        /// Compute instance hash from instance ID.
        /// Returns first 8 hex characters of SHA1 hash.
        /// </summary>
        internal static string ComputeInstanceHash(string instanceId)
        {
            using var sha1 = SHA1.Create();
            var bytes = Encoding.UTF8.GetBytes(instanceId ?? string.Empty);
            var hashBytes = sha1.ComputeHash(bytes);
            var sb = new StringBuilder(8);
            for (var i = 0; i < 4; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Get status file path for an instance.
        /// Format: {statusDir}/status-{hash}.json
        /// </summary>
        public static string GetStatusFilePath(string instanceId)
        {
            var dir = GetStatusDir();
            var hash = ComputeInstanceHash(instanceId);
            return Path.Combine(dir, $"status-{hash}.json");
        }

        /// <summary>
        /// Write status file synchronously.
        /// Called before domain reload to ensure status is persisted.
        /// </summary>
        public static void WriteStatus(
            string instanceId,
            string projectName,
            string unityVersion,
            string status,
            string relayHost,
            int relayPort)
        {
            try
            {
                var dir = GetStatusDir();
                Directory.CreateDirectory(dir);

                var filePath = GetStatusFilePath(instanceId);
                var payload = new
                {
                    instance_id = instanceId,
                    project_name = projectName,
                    unity_version = unityVersion,
                    status,
                    relay_host = relayHost,
                    relay_port = relayPort,
                    timestamp = DateTime.UtcNow.ToString("O"),
                    seq = Interlocked.Increment(ref _seq)
                };

                var json = JsonConvert.SerializeObject(payload);
                File.WriteAllText(filePath, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

                BridgeLog.Verbose($"Status file written: {status}");
            }
            catch (Exception ex)
            {
                BridgeLog.Warn($"Failed to write status file: {ex.Message}");
            }
        }

        /// <summary>
        /// Delete status file.
        /// Called when Unity quits or connection is intentionally closed.
        /// </summary>
        public static void DeleteStatus(string instanceId)
        {
            try
            {
                var filePath = GetStatusFilePath(instanceId);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    BridgeLog.Verbose("Status file deleted");
                }
            }
            catch (Exception ex)
            {
                BridgeLog.Warn($"Failed to delete status file: {ex.Message}");
            }
        }
    }
}
