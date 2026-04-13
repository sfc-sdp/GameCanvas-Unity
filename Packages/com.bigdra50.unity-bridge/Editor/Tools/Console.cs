using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityBridge.Helpers;

namespace UnityBridge.Tools
{
    /// <summary>
    /// Handler for console commands.
    /// Reads and clears Unity console log entries.
    /// Based on Coplay/unity-mcp (now CoplayDev/unity-mcp) implementation.
    /// </summary>
    [BridgeTool("console")]
    public static class Console
    {
        public static JObject HandleCommand(JObject parameters)
        {
            var action = parameters["action"]?.Value<string>() ?? "read";

            return action switch
            {
                "read" => HandleRead(parameters),
                "clear" => HandleClear(),
                _ => throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    $"Unknown action: {action}. Valid actions: read, clear")
            };
        }

        private static JObject HandleRead(JObject parameters)
        {
            // types未指定(null)の場合はnullのまま渡す（フィルタなし=全タイプ）
            var types = parameters["types"]?.ToObject<string[]>();
            var count = parameters["count"]?.Value<int?>() ?? int.MaxValue;  // unspecified = all
            var search = parameters["search"]?.Value<string>();
            var includeStackTrace = parameters["include_stacktrace"]?.Value<bool>() ?? false;

            var entries = GetConsoleEntries(types, count, search, includeStackTrace);

            return new JObject
            {
                ["entries"] = JArray.FromObject(entries),
                ["count"] = entries.Count
            };
        }

        private static JObject HandleClear()
        {
            ClearConsole();

            return new JObject
            {
                ["success"] = true,
                ["message"] = "Console cleared"
            };
        }

        private static List<object> GetConsoleEntries(string[] types, int count, string search, bool includeStackTrace)
        {
            var entries = new List<object>();

            // Use reflection to access internal LogEntries class
            var logEntriesType = Type.GetType("UnityEditor.LogEntries, UnityEditor");
            if (logEntriesType == null)
            {
                BridgeLog.Error("Could not find LogEntries type");
                return entries;
            }

            BridgeLog.Verbose($"LogEntries type found: {logEntriesType.FullName}");

            var logEntryType = Type.GetType("UnityEditor.LogEntry, UnityEditor");
            if (logEntryType == null)
            {
                BridgeLog.Error("Could not find LogEntry type");
                return entries;
            }

            BridgeLog.Verbose($"LogEntry type found: {logEntryType.FullName}");

            // Get methods - include NonPublic binding flag
            var bindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            var getCountMethod = logEntriesType.GetMethod("GetCount", bindingFlags);
            var startGettingEntriesMethod = logEntriesType.GetMethod("StartGettingEntries", bindingFlags);
            var getEntryInternalMethod = logEntriesType.GetMethod("GetEntryInternal", bindingFlags);
            var endGettingEntriesMethod = logEntriesType.GetMethod("EndGettingEntries", bindingFlags);

            if (getCountMethod == null || startGettingEntriesMethod == null ||
                getEntryInternalMethod == null || endGettingEntriesMethod == null)
            {
                BridgeLog.Error($"Could not find required LogEntries methods: GetCount={getCountMethod != null}, StartGettingEntries={startGettingEntriesMethod != null}, GetEntryInternal={getEntryInternalMethod != null}, EndGettingEntries={endGettingEntriesMethod != null}");
                // List all methods for debugging
                foreach (var method in logEntriesType.GetMethods(bindingFlags))
                {
                    BridgeLog.Verbose($"LogEntries method: {method.Name}");
                }
                return entries;
            }

            BridgeLog.Verbose("All LogEntries methods found");

            // Get LogEntry fields - include NonPublic binding flag
            var instanceBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var messageField = logEntryType.GetField("message", instanceBindingFlags);
            var modeField = logEntryType.GetField("mode", instanceBindingFlags);

            // Also try "condition" field which some Unity versions use
            if (messageField == null)
            {
                messageField = logEntryType.GetField("condition", instanceBindingFlags);
            }

            if (messageField == null || modeField == null)
            {
                BridgeLog.Error($"Could not find required LogEntry fields: message={messageField != null}, mode={modeField != null}");
                // List all fields for debugging
                foreach (var field in logEntryType.GetFields(instanceBindingFlags))
                {
                    BridgeLog.Error($"LogEntry field: {field.Name} ({field.FieldType})");
                }
                return entries;
            }

            BridgeLog.Verbose($"LogEntry fields found: message={messageField.Name}, mode={modeField.Name}");

            // Convert type filters (null = no filter, all types)
            HashSet<string> typeSet = types != null
                ? new HashSet<string>(types, StringComparer.OrdinalIgnoreCase)
                : null;

            try
            {
                startGettingEntriesMethod.Invoke(null, null);

                var totalCount = (int)getCountMethod.Invoke(null, null);
                var typesDesc = types != null ? string.Join(",", types) : "(all)";
                BridgeLog.Verbose($"Console: Total entries={totalCount}, requested types={typesDesc}, count={count}");

                var logEntry = Activator.CreateInstance(logEntryType);

                // Read from the end (most recent first) until we have enough filtered entries
                for (var i = totalCount - 1; i >= 0 && entries.Count < count; i--)
                {
                    getEntryInternalMethod.Invoke(null, new[] { i, logEntry });

                    var rawMessage = (string)messageField.GetValue(logEntry);
                    var mode = (int)modeField.GetValue(logEntry);
                    var entryType = GetEntryType(mode);

                    // Filter by type (skip if typeSet is null = no filter)
                    if (typeSet != null && !typeSet.Contains(entryType))
                        continue;

                    // Filter by search (check full message including stack trace)
                    if (!string.IsNullOrEmpty(search) &&
                        !rawMessage.Contains(search, StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Split message and stack trace at first newline
                    var (message, stackTrace) = SplitMessageAndStackTrace(rawMessage);

                    if (includeStackTrace && !string.IsNullOrEmpty(stackTrace))
                    {
                        entries.Add(new
                        {
                            message,
                            type = entryType,
                            stackTrace,
                            timestamp = DateTime.Now.ToString("HH:mm:ss")
                        });
                    }
                    else
                    {
                        entries.Add(new
                        {
                            message,
                            type = entryType,
                            timestamp = DateTime.Now.ToString("HH:mm:ss")
                        });
                    }
                }
            }
            finally
            {
                endGettingEntriesMethod.Invoke(null, null);
            }

            return entries;
        }

        /// <summary>
        /// Split raw log message into message body and stack trace.
        /// Unity combines message and stack trace with newline in the message field.
        /// </summary>
        private static (string message, string stackTrace) SplitMessageAndStackTrace(string rawMessage)
        {
            if (string.IsNullOrEmpty(rawMessage))
                return (string.Empty, string.Empty);

            var newlineIndex = rawMessage.IndexOf('\n');
            if (newlineIndex < 0)
                return (rawMessage.TrimEnd(), string.Empty);

            var message = rawMessage.Substring(0, newlineIndex).TrimEnd();
            var stackTrace = rawMessage.Substring(newlineIndex + 1).Trim();
            return (message, stackTrace);
        }

        /// <summary>
        /// Determine log entry type from Unity's internal Mode flags.
        /// Based on Unity's ConsoleWindow.cs Mode enum from UnityCsReference.
        /// </summary>
        private static string GetEntryType(int mode)
        {
            // Unity internal Mode enum bit flags (from UnityCsReference ConsoleWindow.cs)
            const int kError = 1 << 0;              // 1
            const int kAssert = 1 << 1;             // 2
            const int kLog = 1 << 2;                // 4
            const int kFatal = 1 << 4;              // 16
            const int kAssetImportError = 1 << 6;   // 64
            const int kAssetImportWarning = 1 << 7; // 128
            const int kScriptingError = 1 << 8;     // 256
            const int kScriptingWarning = 1 << 9;   // 512
            const int kScriptingLog = 1 << 10;      // 1024
            const int kScriptCompileError = 1 << 11;   // 2048
            const int kScriptCompileWarning = 1 << 12; // 4096
            const int kScriptingException = 1 << 17;   // 131072
            const int kScriptingAssertion = 1 << 21;   // 2097152
            const int kGraphCompileError = 1 << 20;    // 1048576
            const int kVisualScriptingError = 1 << 22; // 4194304

            // Exception check (highest priority)
            if ((mode & kScriptingException) != 0)
                return "exception";

            // Error checks
            const int kErrorMask = kError | kFatal | kAssetImportError | kScriptingError |
                                   kScriptCompileError | kGraphCompileError | kVisualScriptingError;
            if ((mode & kErrorMask) != 0)
                return "error";

            // Assert checks
            const int kAssertMask = kAssert | kScriptingAssertion;
            if ((mode & kAssertMask) != 0)
                return "assert";

            // Warning checks
            const int kWarningMask = kAssetImportWarning | kScriptingWarning | kScriptCompileWarning;
            if ((mode & kWarningMask) != 0)
                return "warning";

            // Log checks
            const int kLogMask = kLog | kScriptingLog;
            if ((mode & kLogMask) != 0)
                return "log";

            // Default: treat as log
            return "log";
        }

        private static void ClearConsole()
        {
            var logEntriesType = Type.GetType("UnityEditor.LogEntries, UnityEditor");
            var clearMethod = logEntriesType?.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            clearMethod?.Invoke(null, null);
        }
    }
}
