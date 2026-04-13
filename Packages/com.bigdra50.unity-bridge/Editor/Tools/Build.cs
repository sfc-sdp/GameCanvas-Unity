using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace UnityBridge.Tools
{
    [BridgeTool("build")]
    public static class Build
    {
        public static JObject HandleCommand(JObject parameters)
        {
            var action = parameters["action"]?.Value<string>() ?? "";

            return action.ToLowerInvariant() switch
            {
                "settings" => GetSettings(),
                "build" => RunBuild(parameters),
                "scenes" => GetScenes(),
                _ => throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    $"Unknown action: {action}. Valid: settings, build, scenes")
            };
        }

        private static JObject GetSettings()
        {
            var target = EditorUserBuildSettings.activeBuildTarget;
            var targetGroup = BuildPipeline.GetBuildTargetGroup(target);
            var scenes = EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => s.path)
                .ToArray<object>();

            var scriptingBackend = PlayerSettings.GetScriptingBackend(
                UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(targetGroup));

            return new JObject
            {
                ["target"] = target.ToString(),
                ["targetGroup"] = targetGroup.ToString(),
                ["scenes"] = new JArray(scenes),
                ["productName"] = PlayerSettings.productName,
                ["companyName"] = PlayerSettings.companyName,
                ["bundleVersion"] = PlayerSettings.bundleVersion,
                ["scriptingBackend"] = scriptingBackend.ToString()
            };
        }

        private static JObject RunBuild(JObject parameters)
        {
            var targetStr = parameters["target"]?.Value<string>();
            var outputPath = parameters["outputPath"]?.Value<string>();
            var scenesParam = parameters["scenes"]?.ToObject<string[]>();

            BuildTarget target;
            if (!string.IsNullOrEmpty(targetStr))
            {
                if (!Enum.TryParse(targetStr, true, out target))
                {
                    throw new ProtocolException(
                        ErrorCode.InvalidParams,
                        $"Invalid BuildTarget: {targetStr}");
                }
            }
            else
            {
                target = EditorUserBuildSettings.activeBuildTarget;
            }

            string[] scenes;
            if (scenesParam != null && scenesParam.Length > 0)
            {
                scenes = scenesParam;
            }
            else
            {
                scenes = EditorBuildSettings.scenes
                    .Where(s => s.enabled)
                    .Select(s => s.path)
                    .ToArray();
            }

            if (scenes.Length == 0)
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    "No scenes to build. Add scenes to Build Settings or pass 'scenes' parameter.");
            }

            if (string.IsNullOrEmpty(outputPath))
            {
                var productName = PlayerSettings.productName;
                var extension = GetBuildExtension(target);
                outputPath = $"Builds/{target}/{productName}{extension}";
            }

            var directory = System.IO.Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = target,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            var steps = report.steps
                .Select(s => new JObject
                {
                    ["name"] = s.name,
                    ["duration"] = s.duration.TotalSeconds
                })
                .ToArray<object>();

            var messages = report.steps
                .SelectMany(s => s.messages)
                .Where(m => m.type == LogType.Error || m.type == LogType.Warning)
                .Select(m => new JObject
                {
                    ["type"] = m.type.ToString(),
                    ["content"] = m.content
                })
                .ToArray<object>();

            return new JObject
            {
                ["result"] = summary.result.ToString(),
                ["totalSize"] = summary.totalSize,
                ["totalTime"] = summary.totalTime.TotalSeconds,
                ["outputPath"] = summary.outputPath,
                ["target"] = target.ToString(),
                ["totalErrors"] = summary.totalErrors,
                ["totalWarnings"] = summary.totalWarnings,
                ["steps"] = new JArray(steps),
                ["messages"] = new JArray(messages)
            };
        }

        private static JObject GetScenes()
        {
            var scenes = EditorBuildSettings.scenes
                .Select(s => new JObject
                {
                    ["path"] = s.path,
                    ["enabled"] = s.enabled,
                    ["guid"] = s.guid.ToString()
                })
                .ToArray<object>();

            return new JObject
            {
                ["count"] = scenes.Length,
                ["scenes"] = new JArray(scenes)
            };
        }

        private static string GetBuildExtension(BuildTarget target)
        {
            return target switch
            {
                BuildTarget.StandaloneWindows => ".exe",
                BuildTarget.StandaloneWindows64 => ".exe",
                BuildTarget.StandaloneOSX => ".app",
                BuildTarget.Android => ".apk",
                _ => ""
            };
        }
    }
}
