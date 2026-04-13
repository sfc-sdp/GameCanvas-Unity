using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityBridge.Helpers;

namespace UnityBridge
{
    /// <summary>
    /// Protocol constants for Unity Bridge v1.0
    /// </summary>
    public static class ProtocolConstants
    {
        public const string Version = "1.0";
        public const int HeaderSize = 4;
        public const int MaxPayloadBytes = 16 * 1024 * 1024; // 16 MiB
        public const int DefaultPort = 6500;
        public const int HeartbeatIntervalMs = 5000;
        public const int HeartbeatTimeoutMs = 15000;
        public const int CommandTimeoutMs = 30000;
        public const int ReloadTimeoutMs = 30000;
        public const int PortReleaseWaitMs = 500;
        public const int DisposeTaskWaitMs = 500;
        public const int PortWaitTimeoutMs = 5000;
    }

    /// <summary>
    /// Message types for the Unity Bridge protocol
    /// </summary>
    public static class MessageType
    {
        // Unity → Relay
        public const string Register = "REGISTER";
        public const string Registered = "REGISTERED";
        public const string Status = "STATUS";
        public const string CommandResult = "COMMAND_RESULT";
        public const string Pong = "PONG";

        // Relay → Unity
        public const string Ping = "PING";
        public const string Command = "COMMAND";

        // CLI → Relay
        public const string Request = "REQUEST";
        public const string ListInstances = "LIST_INSTANCES";
        public const string SetDefault = "SET_DEFAULT";

        // Relay → CLI
        public const string Response = "RESPONSE";
        public const string Error = "ERROR";
        public const string Instances = "INSTANCES";
    }

    /// <summary>
    /// Error codes returned by the protocol
    /// </summary>
    public static class ErrorCode
    {
        public const string InstanceNotFound = "INSTANCE_NOT_FOUND";
        public const string InstanceReloading = "INSTANCE_RELOADING";
        public const string InstanceBusy = "INSTANCE_BUSY";
        public const string InstanceDisconnected = "INSTANCE_DISCONNECTED";
        public const string CommandNotFound = "COMMAND_NOT_FOUND";
        public const string InvalidParams = "INVALID_PARAMS";
        public const string Timeout = "TIMEOUT";
        public const string InternalError = "INTERNAL_ERROR";
        public const string ProtocolError = "PROTOCOL_ERROR";
        public const string MalformedJson = "MALFORMED_JSON";
        public const string PayloadTooLarge = "PAYLOAD_TOO_LARGE";
        public const string ProtocolVersionMismatch = "PROTOCOL_VERSION_MISMATCH";
        public const string CapabilityNotSupported = "CAPABILITY_NOT_SUPPORTED";
        public const string QueueFull = "QUEUE_FULL";
        public const string StaleRef = "STALE_REF";
    }

    /// <summary>
    /// Instance status values
    /// </summary>
    public static class InstanceStatus
    {
        public const string Ready = "ready";
        public const string Busy = "busy";
        public const string Reloading = "reloading";
        public const string Disconnected = "disconnected";
    }

    /// <summary>
    /// Activity phase detail values sent with STATUS busy.
    /// Must match relay/_VALID_DETAILS allowlist.
    /// </summary>
    public static class ActivityPhase
    {
        public const string Idle = "idle";
        public const string Compiling = "compiling";
        public const string RunningTests = "running_tests";
        public const string AssetImport = "asset_import";
        public const string PlaymodeTransition = "playmode_transition";
        public const string EditorBlocked = "editor_blocked";
    }

    /// <summary>
    /// Protocol framing: 4-byte big-endian length prefix + JSON payload
    /// </summary>
    public static class Framing
    {
        /// <summary>
        /// Write a framed message to the stream
        /// </summary>
        public static async Task WriteFrameAsync(
            NetworkStream stream,
            JObject payload,
            CancellationToken cancellationToken = default)
        {
            var json = payload.ToString(Formatting.None);
            var payloadBytes = Encoding.UTF8.GetBytes(json);

            if (payloadBytes.Length > ProtocolConstants.MaxPayloadBytes)
            {
                throw new ProtocolException(
                    ErrorCode.PayloadTooLarge,
                    $"Payload too large: {payloadBytes.Length} > {ProtocolConstants.MaxPayloadBytes}");
            }

            // 4-byte big-endian length header
            var header = new byte[ProtocolConstants.HeaderSize];
            header[0] = (byte)((payloadBytes.Length >> 24) & 0xFF);
            header[1] = (byte)((payloadBytes.Length >> 16) & 0xFF);
            header[2] = (byte)((payloadBytes.Length >> 8) & 0xFF);
            header[3] = (byte)(payloadBytes.Length & 0xFF);

            BridgeLog.Verbose($"WriteFrameAsync: writing header ({header.Length} bytes)");
            await stream.WriteAsync(header, 0, header.Length, cancellationToken).ConfigureAwait(false);
            BridgeLog.Verbose($"WriteFrameAsync: writing payload ({payloadBytes.Length} bytes)");
            await stream.WriteAsync(payloadBytes, 0, payloadBytes.Length, cancellationToken).ConfigureAwait(false);
            BridgeLog.Verbose("WriteFrameAsync: flushing");
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            BridgeLog.Verbose("WriteFrameAsync: done");
        }

        /// <summary>
        /// Read a framed message from the stream
        /// </summary>
        public static async Task<JObject> ReadFrameAsync(
            NetworkStream stream,
            CancellationToken cancellationToken = default)
        {
            // Read 4-byte header
            var header = new byte[ProtocolConstants.HeaderSize];
            var bytesRead = await ReadExactlyAsync(stream, header, cancellationToken);
            if (bytesRead < ProtocolConstants.HeaderSize)
            {
                throw new ProtocolException(
                    ErrorCode.ProtocolError,
                    "Connection closed while reading header");
            }

            // Parse big-endian length
            var length = (header[0] << 24) | (header[1] << 16) | (header[2] << 8) | header[3];

            if (length > ProtocolConstants.MaxPayloadBytes)
            {
                throw new ProtocolException(
                    ErrorCode.PayloadTooLarge,
                    $"Payload too large: {length} > {ProtocolConstants.MaxPayloadBytes}");
            }

            if (length <= 0)
            {
                throw new ProtocolException(
                    ErrorCode.ProtocolError,
                    $"Invalid payload length: {length}");
            }

            // Read payload
            var payload = new byte[length];
            bytesRead = await ReadExactlyAsync(stream, payload, cancellationToken);
            if (bytesRead < length)
            {
                throw new ProtocolException(
                    ErrorCode.ProtocolError,
                    "Connection closed while reading payload");
            }

            var json = Encoding.UTF8.GetString(payload);

            try
            {
                return JObject.Parse(json);
            }
            catch (JsonException ex)
            {
                throw new ProtocolException(
                    ErrorCode.MalformedJson,
                    $"Invalid JSON: {ex.Message}");
            }
        }

        private static async Task<int> ReadExactlyAsync(
            NetworkStream stream,
            byte[] buffer,
            CancellationToken cancellationToken)
        {
            var totalRead = 0;
            while (totalRead < buffer.Length)
            {
                var read = await stream.ReadAsync(
                    buffer,
                    totalRead,
                    buffer.Length - totalRead,
                    cancellationToken);

                if (read == 0)
                {
                    break; // Connection closed
                }

                totalRead += read;
            }

            return totalRead;
        }
    }

    /// <summary>
    /// Protocol-specific exception
    /// </summary>
    public class ProtocolException : Exception
    {
        public string Code { get; }

        public ProtocolException(string code, string message)
            : base(message)
        {
            Code = code;
        }
    }

    /// <summary>
    /// Message builder utilities
    /// </summary>
    public static class Messages
    {
        private static long GetTimestamp() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        /// <summary>
        /// Create a REGISTER message
        /// </summary>
        public static JObject CreateRegister(
            string instanceId,
            string projectName,
            string unityVersion,
            IEnumerable<string> capabilities = null)
        {
            var msg = new JObject
            {
                ["type"] = MessageType.Register,
                ["protocol_version"] = ProtocolConstants.Version,
                ["instance_id"] = instanceId,
                ["project_name"] = projectName,
                ["unity_version"] = unityVersion,
                ["bridge_version"] = GetBridgeVersion(),
                ["capabilities"] = new JArray(capabilities ?? Array.Empty<string>()),
                ["ts"] = GetTimestamp()
            };
            return msg;
        }

        private static string GetBridgeVersion()
        {
            var info = UnityEditor.PackageManager.PackageInfo
                .FindForAssembly(typeof(ProtocolConstants).Assembly);
            return info?.version ?? "unknown";
        }

        /// <summary>
        /// Create a STATUS message
        /// </summary>
        public static JObject CreateStatus(string instanceId, string status, string detail = null)
        {
            var msg = new JObject
            {
                ["type"] = MessageType.Status,
                ["instance_id"] = instanceId,
                ["status"] = status,
                ["ts"] = GetTimestamp()
            };

            if (!string.IsNullOrEmpty(detail))
            {
                msg["detail"] = detail;
            }

            return msg;
        }

        /// <summary>
        /// Create a COMMAND_RESULT message (success)
        /// </summary>
        public static JObject CreateCommandResult(string id, JObject data)
        {
            return new JObject
            {
                ["type"] = MessageType.CommandResult,
                ["id"] = id,
                ["success"] = true,
                ["data"] = data,
                ["ts"] = GetTimestamp()
            };
        }

        /// <summary>
        /// Create a COMMAND_RESULT message (error)
        /// </summary>
        public static JObject CreateCommandResultError(string id, string code, string message)
        {
            return new JObject
            {
                ["type"] = MessageType.CommandResult,
                ["id"] = id,
                ["success"] = false,
                ["error"] = new JObject
                {
                    ["code"] = code,
                    ["message"] = message
                },
                ["ts"] = GetTimestamp()
            };
        }

        /// <summary>
        /// Create a PONG message
        /// </summary>
        public static JObject CreatePong(long echoTs)
        {
            return new JObject
            {
                ["type"] = MessageType.Pong,
                ["ts"] = GetTimestamp(),
                ["echo_ts"] = echoTs
            };
        }

        /// <summary>
        /// Parse a REGISTERED response
        /// </summary>
        public static (bool success, int heartbeatIntervalMs, string errorCode, string errorMessage)
            ParseRegistered(JObject msg)
        {
            var success = msg["success"]?.Value<bool>() ?? false;
            var heartbeatIntervalMs = msg["heartbeat_interval_ms"]?.Value<int>()
                                      ?? ProtocolConstants.HeartbeatIntervalMs;

            string errorCode = null;
            string errorMessage = null;

            if (!success && msg["error"] is JObject error)
            {
                errorCode = error["code"]?.Value<string>();
                errorMessage = error["message"]?.Value<string>();
            }

            return (success, heartbeatIntervalMs, errorCode, errorMessage);
        }

        /// <summary>
        /// Parse a COMMAND message
        /// </summary>
        public static (string id, string command, JObject parameters, int timeoutMs)
            ParseCommand(JObject msg)
        {
            var id = msg["id"]?.Value<string>() ?? "";
            var command = msg["command"]?.Value<string>() ?? "";
            var parameters = msg["params"] as JObject ?? new JObject();
            var timeoutMs = msg["timeout_ms"]?.Value<int>() ?? ProtocolConstants.CommandTimeoutMs;

            return (id, command, parameters, timeoutMs);
        }

        /// <summary>
        /// Parse a PING message
        /// </summary>
        public static long ParsePing(JObject msg)
        {
            return msg["ts"]?.Value<long>() ?? 0;
        }
    }

    /// <summary>
    /// Helper for generating instance IDs
    /// </summary>
    public static class InstanceIdHelper
    {
        /// <summary>
        /// Get the canonical instance ID for the current Unity project.
        /// Uses Path.GetFullPath to normalize the path.
        /// </summary>
        public static string GetInstanceId()
        {
#if UNITY_EDITOR
            // Application.dataPath returns "ProjectPath/Assets"
            // We want "ProjectPath"
            var dataPath = UnityEngine.Application.dataPath;
            var projectPath = Path.GetFullPath(Path.Combine(dataPath, ".."));

            // Normalize: remove trailing slash
            projectPath = projectPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            return projectPath;
#else
            throw new InvalidOperationException("InstanceIdHelper is only available in Unity Editor");
#endif
        }

        /// <summary>
        /// Get the project name from the instance ID
        /// </summary>
        public static string GetProjectName(string instanceId)
        {
            return Path.GetFileName(instanceId);
        }
    }
}
