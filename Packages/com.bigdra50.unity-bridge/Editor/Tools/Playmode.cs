using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace UnityBridge.Tools
{
    /// <summary>
    /// Handler for playmode commands.
    /// Controls Unity Editor play mode state and queries editor status.
    /// </summary>
    [BridgeTool("playmode")]
    public static class Playmode
    {
        public static JObject HandleCommand(JObject parameters)
        {
            var action = parameters["action"]?.Value<string>() ?? "";

            return action.ToLowerInvariant() switch
            {
                "enter" => Enter(),
                "exit" => Exit(),
                "pause" => Pause(),
                "unpause" => Unpause(),
                "step" => Step(),
                "state" => GetState(),
                _ => throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    $"Unknown action: {action}. Valid actions: enter, exit, pause, unpause, step, state")
            };
        }

        private static JObject Enter()
        {
            if (EditorApplication.isPlaying)
            {
                return new JObject
                {
                    ["message"] = "Already in play mode"
                };
            }

            // Schedule play mode for next update using EditorApplication.update.
            // This ensures COMMAND_RESULT is sent BEFORE domain reload starts.
            // Note: delayCall doesn't fire until editor receives focus, so we use update instead.
            void StartPlayMode()
            {
                EditorApplication.update -= StartPlayMode;
                if (!EditorApplication.isPlaying)
                {
                    EditorApplication.isPlaying = true;
                }
            }

            EditorApplication.update += StartPlayMode;

            return new JObject
            {
                ["message"] = "Play mode starting"
            };
        }

        private static JObject Exit()
        {
            if (!EditorApplication.isPlaying)
            {
                return new JObject
                {
                    ["message"] = "Not in play mode"
                };
            }

            EditorApplication.isPlaying = false;

            return new JObject
            {
                ["message"] = "Play mode stopped"
            };
        }

        private static JObject Pause()
        {
            if (!EditorApplication.isPlaying)
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    "Cannot pause: not in play mode. Enter play mode first with action 'enter'.");
            }

            EditorApplication.isPaused = true;

            return new JObject
            {
                ["message"] = "Paused"
            };
        }

        private static JObject Unpause()
        {
            if (!EditorApplication.isPlaying)
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    "Cannot unpause: not in play mode. Enter play mode first with action 'enter'.");
            }

            EditorApplication.isPaused = false;

            return new JObject
            {
                ["message"] = "Unpaused"
            };
        }

        private static JObject Step()
        {
            if (!EditorApplication.isPlaying)
            {
                throw new ProtocolException(
                    ErrorCode.InvalidParams,
                    "Cannot step: not in play mode. Enter play mode first with action 'enter'.");
            }

            EditorApplication.Step();

            return new JObject
            {
                ["message"] = "Stepped one frame"
            };
        }

        private static JObject GetState()
        {
            var activeScene = SceneManager.GetActiveScene();

            return new JObject
            {
                ["isPlaying"] = EditorApplication.isPlaying,
                ["isPaused"] = EditorApplication.isPaused,
                ["isCompiling"] = EditorApplication.isCompiling,
                ["currentScene"] = activeScene.path,
                ["sceneName"] = activeScene.name,
                ["sceneIsDirty"] = activeScene.isDirty,
                ["applicationPath"] = EditorApplication.applicationPath,
                ["applicationContentsPath"] = EditorApplication.applicationContentsPath,
                ["timeSinceStartup"] = EditorApplication.timeSinceStartup
            };
        }
    }
}
