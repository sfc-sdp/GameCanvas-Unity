using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace UnityBridge
{
    /// <summary>
    /// Abstraction for relay client communication.
    /// Enables dependency injection and testability for BridgeManager.
    /// </summary>
    public interface IRelayClient : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// Instance ID (project path)
        /// </summary>
        string InstanceId { get; }

        /// <summary>
        /// Whether the client is currently connected
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Current connection status
        /// </summary>
        ConnectionStatus Status { get; }

        /// <summary>
        /// Project name
        /// </summary>
        string ProjectName { get; }

        /// <summary>
        /// Unity version
        /// </summary>
        string UnityVersion { get; }

        /// <summary>
        /// Supported capabilities
        /// </summary>
        string[] Capabilities { get; set; }

        /// <summary>
        /// Event fired when connection status changes
        /// </summary>
        event EventHandler<ConnectionStatusChangedEventArgs> StatusChanged;

        /// <summary>
        /// Event fired when a command is received from the relay server
        /// </summary>
        event EventHandler<CommandReceivedEventArgs> CommandReceived;

        /// <summary>
        /// Connect to the relay server and register this instance
        /// </summary>
        Task ConnectAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Disconnect from the relay server
        /// </summary>
        Task DisconnectAsync();

        /// <summary>
        /// Send a STATUS message with optional detail to the relay server
        /// </summary>
        Task SendStatusAsync(string status, string detail = null);

        /// <summary>
        /// Send STATUS "reloading" to the relay server
        /// </summary>
        Task SendReloadingStatusAsync();

        /// <summary>
        /// Send STATUS "ready" to the relay server
        /// </summary>
        Task SendReadyStatusAsync();

        /// <summary>
        /// Send a command result (success) back through the relay
        /// </summary>
        Task SendCommandResultAsync(string id, JObject data);

        /// <summary>
        /// Send a command error back through the relay
        /// </summary>
        Task SendCommandErrorAsync(string id, string code, string message);
    }
}
