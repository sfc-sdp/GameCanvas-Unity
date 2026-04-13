using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Profiling = UnityEngine.Profiling;

namespace UnityBridge.Tools
{
    [BridgeTool("profiler")]
    public static class Profiler
    {
        public static JObject HandleCommand(JObject parameters)
        {
            var action = parameters["action"]?.Value<string>() ?? "";

            return action.ToLowerInvariant() switch
            {
                "status" => GetStatus(),
                "start" => Start(),
                "stop" => Stop(),
                "snapshot" => GetSnapshot(),
                "frames" => GetFrames(parameters),
                _ => throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    $"Unknown action: {action}. Valid: status, start, stop, snapshot, frames")
            };
        }

        private static JObject GetStatus()
        {
            var enabled = ProfilerDriver.enabled;
            var firstFrame = ProfilerDriver.firstFrameIndex;
            var lastFrame = ProfilerDriver.lastFrameIndex;

            return new JObject
            {
                ["enabled"] = enabled,
                ["firstFrameIndex"] = firstFrame,
                ["lastFrameIndex"] = lastFrame
            };
        }

        private static JObject Start()
        {
            if (ProfilerDriver.enabled)
            {
                return new JObject
                {
                    ["message"] = "Profiler already running",
                    ["enabled"] = true
                };
            }

            var warning = !EditorApplication.isPlaying
                ? "Profiler started outside Play Mode. Some data may be limited."
                : null;

            ProfilerDriver.enabled = true;

            var result = new JObject
            {
                ["message"] = "Profiler started",
                ["enabled"] = true
            };

            if (warning != null)
                result["warning"] = warning;

            return result;
        }

        private static JObject Stop()
        {
            if (!ProfilerDriver.enabled)
            {
                return new JObject
                {
                    ["message"] = "Profiler already stopped",
                    ["enabled"] = false
                };
            }

            ProfilerDriver.enabled = false;

            return new JObject
            {
                ["message"] = "Profiler stopped",
                ["enabled"] = false
            };
        }

        private static JObject GetSnapshot()
        {
            var lastFrame = ProfilerDriver.lastFrameIndex;
            if (lastFrame < 0)
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    "No profiler data available. Start the profiler first.");
            }

            return BuildFrameData(lastFrame);
        }

        private static JObject GetFrames(JObject parameters)
        {
            var count = parameters["count"]?.Value<int>() ?? 10;
            count = Mathf.Clamp(count, 1, 300);

            var lastFrame = ProfilerDriver.lastFrameIndex;
            var firstFrame = ProfilerDriver.firstFrameIndex;

            if (lastFrame < 0 || firstFrame < 0)
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    "No profiler data available. Start the profiler first.");
            }

            var startFrame = Mathf.Max(firstFrame, lastFrame - count + 1);
            var frames = new JArray();

            for (var i = startFrame; i <= lastFrame; i++)
            {
                frames.Add(BuildFrameData(i));
            }

            return new JObject
            {
                ["count"] = frames.Count,
                ["firstFrameIndex"] = startFrame,
                ["lastFrameIndex"] = lastFrame,
                ["frames"] = frames
            };
        }

        private static readonly (string name, string key, Profiling.ProfilerArea area)[] Stats =
        {
            ("FPS", "fps", Profiling.ProfilerArea.CPU),
            ("CPU Main Thread Frame Time", "cpuFrameTimeMs", Profiling.ProfilerArea.CPU),
            ("CPU Render Thread Frame Time", "cpuRenderThreadTimeMs", Profiling.ProfilerArea.CPU),
            ("GPU Frame Time", "gpuFrameTimeMs", Profiling.ProfilerArea.GPU),
            ("Batches", "batches", Profiling.ProfilerArea.Rendering),
            ("Draw Calls", "drawCalls", Profiling.ProfilerArea.Rendering),
            ("Triangles", "triangles", Profiling.ProfilerArea.Rendering),
            ("Vertices", "vertices", Profiling.ProfilerArea.Rendering),
            ("SetPass Calls", "setPassCalls", Profiling.ProfilerArea.Rendering),
            ("GC Allocation In Frame Count", "gcAllocCount", Profiling.ProfilerArea.Memory),
            ("GC Allocated In Frame", "gcAllocBytes", Profiling.ProfilerArea.Memory),
        };

        private static JObject BuildFrameData(int frameIndex)
        {
            var result = new JObject
            {
                ["frameIndex"] = frameIndex
            };

            foreach (var (name, key, area) in Stats)
            {
                try
                {
                    var id = ProfilerDriver.GetStatisticsIdentifierForArea(area, name);
#pragma warning disable CS0618 // GetFormattedCounterValue signature differs across Unity versions
                    var value = ProfilerDriver.GetFormattedStatisticsValue(frameIndex, id);
#pragma warning restore CS0618
                    if (!string.IsNullOrEmpty(value))
                        result[key] = value;
                }
                catch
                {
                    // stat not available in this Unity version
                }
            }

            return result;
        }
    }
}
