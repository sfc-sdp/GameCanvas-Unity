using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace UnityBridge
{
    /// <summary>
    /// Abstraction for command dispatching.
    /// Enables dependency injection and testability for BridgeManager.
    /// </summary>
    public interface ICommandDispatcher
    {
        /// <summary>
        /// Get all registered command names
        /// </summary>
        IEnumerable<string> RegisteredCommands { get; }

        /// <summary>
        /// Initialize and discover all handlers
        /// </summary>
        void Initialize();

        /// <summary>
        /// Execute a command by name
        /// </summary>
        Task<JObject> ExecuteAsync(string commandName, JObject parameters);
    }
}
