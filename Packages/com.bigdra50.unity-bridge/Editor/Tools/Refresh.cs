using Newtonsoft.Json.Linq;
using UnityEditor;

namespace UnityBridge.Tools
{
    /// <summary>
    /// Handler for refresh command.
    /// Triggers AssetDatabase.Refresh() to recompile scripts and reimport assets.
    /// </summary>
    [BridgeTool("refresh")]
    public static class Refresh
    {
        public static JObject HandleCommand(JObject parameters)
        {
            AssetDatabase.Refresh();

            return new JObject
            {
                ["message"] = "Asset database refreshed"
            };
        }
    }
}
