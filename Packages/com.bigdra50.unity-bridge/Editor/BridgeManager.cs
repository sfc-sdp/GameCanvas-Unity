using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityBridge.Helpers;
using UnityEditor;

namespace UnityBridge
{
    /// <summary>
    /// Singleton manager for the Unity Bridge connection.
    /// Handles connection lifecycle, command dispatch, and main thread synchronization.
    /// </summary>
    public class BridgeManager
    {
        private static BridgeManager _instance;
        private static readonly object Lock = new();

        // Command queue for main thread execution
        private readonly ConcurrentQueue<CommandReceivedEventArgs> _commandQueue = new();
        private bool _updateRegistered;
        private bool _isProcessing;

        /// <summary>
        /// Singleton instance
        /// </summary>
        public static BridgeManager Instance
        {
            get
            {
                if (_instance != null) return _instance;
                lock (Lock)
                {
                    _instance ??= new BridgeManager();
                }

                return _instance;
            }
        }

        /// <summary>
        /// The relay client instance
        /// </summary>
        public IRelayClient Client { get; private set; }

        /// <summary>
        /// The command dispatcher instance
        /// </summary>
        public ICommandDispatcher Dispatcher { get; }

        /// <summary>
        /// Current connection host
        /// </summary>
        public string Host { get; private set; } = "127.0.0.1";

        /// <summary>
        /// Current connection port
        /// </summary>
        public int Port { get; private set; } = ProtocolConstants.DefaultPort;

        /// <summary>
        /// Whether the client is connected
        /// </summary>
        public bool IsConnected => Client?.IsConnected ?? false;

        /// <summary>
        /// Event fired when connection status changes
        /// </summary>
        public event EventHandler<ConnectionStatusChangedEventArgs> StatusChanged;

        private readonly Func<string, int, IRelayClient> _clientFactory;

        private BridgeManager() : this(new CommandDispatcher(), (host, port) => new RelayClient(host, port))
        {
        }

        /// <summary>
        /// Constructor with dependency injection for testability.
        /// </summary>
        internal BridgeManager(ICommandDispatcher dispatcher, Func<string, int, IRelayClient> clientFactory = null)
        {
            Dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
            _clientFactory = clientFactory ?? ((host, port) => new RelayClient(host, port));
        }

        /// <summary>
        /// Connect to the relay server
        /// </summary>
        public async Task ConnectAsync(string host = "127.0.0.1", int port = ProtocolConstants.DefaultPort)
        {
            if (Client != null)
            {
                await DisconnectAsync();
            }

            Host = host;
            Port = port;

            Client = _clientFactory(host, port);
            Client.StatusChanged += OnClientStatusChanged;
            Client.CommandReceived += OnCommandReceived;

            // Register update handler on main thread BEFORE connecting,
            // so commands are processed immediately when received.
            EnsureUpdateRegistered();

            await Client.ConnectAsync();

            // Register for reload handling
            BridgeReloadHandler.RegisterClient(Client, host, port);
        }

        /// <summary>
        /// Disconnect from the relay server
        /// </summary>
        public async Task DisconnectAsync()
        {
            BridgeReloadHandler.UnregisterClient();

            // Unregister update handler
            UnregisterUpdate();

            // Capture reference to avoid race conditions
            var client = Client;
            Client = null;

            if (client != null)
            {
                // Unsubscribe from events first to prevent callbacks during disconnect
                client.StatusChanged -= OnClientStatusChanged;
                client.CommandReceived -= OnCommandReceived;

                try
                {
                    await client.DisposeAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    BridgeLog.Warn($"Dispose error (ignored): {ex.Message}");
                }
            }
        }

        private void OnClientStatusChanged(object sender, ConnectionStatusChangedEventArgs e)
        {
            // Forward to subscribers
            StatusChanged?.Invoke(this, e);
        }

        private void OnCommandReceived(object sender, CommandReceivedEventArgs e)
        {
            BridgeLog.Verbose($"Queuing command for main thread: {e.Command} (id: {e.Id})");

            // Queue command for main thread execution (ConcurrentQueue is thread-safe)
            _commandQueue.Enqueue(e);

            // QueuePlayerLoopUpdate is main-thread-only in Unity 6.
            // Since OnCommandReceived fires from a background thread (ReceiveLoopAsync),
            // we skip the call here. ProcessCommandQueue picks up commands on the next
            // EditorApplication.update tick, which is already registered in ConnectAsync.
        }

        private void EnsureUpdateRegistered()
        {
            if (_updateRegistered)
                return;

            EditorApplication.update += ProcessCommandQueue;
            _updateRegistered = true;
            BridgeLog.Verbose("Registered EditorApplication.update handler");
        }

        private void UnregisterUpdate()
        {
            if (!_updateRegistered)
                return;

            EditorApplication.update -= ProcessCommandQueue;
            _updateRegistered = false;
        }

        private async void ProcessCommandQueue()
        {
            // Prevent re-entrant processing: EditorApplication.update fires every frame,
            // and previous await may not have completed yet.
            if (_isProcessing)
                return;

            if (_commandQueue.IsEmpty)
                return;

            _isProcessing = true;
            try
            {
                while (_commandQueue.TryDequeue(out var e))
                {
                    BridgeLog.Verbose($"Processing command from queue: {e.Command} (id: {e.Id})");
                    await ExecuteCommandOnMainThreadAsync(e);
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"[UnityBridge:Queue] Exception: {ex}");
            }
            finally
            {
                _isProcessing = false;
            }
        }

        internal async Task ExecuteCommandOnMainThreadAsync(CommandReceivedEventArgs e)
        {
            BridgeLog.Verbose($"Executing command on main thread: {e.Command} (id: {e.Id})");

            if (Client is not { IsConnected: true })
            {
                BridgeLog.Warn($"Cannot execute command, not connected: {e.Command}");
                return;
            }

            try
            {
                var result = await Dispatcher.ExecuteAsync(e.Command, e.Parameters);
                await Client.SendCommandResultAsync(e.Id, result).ConfigureAwait(false);
            }
            catch (ProtocolException pex)
            {
                BridgeLog.Warn($"Protocol error: {pex.Code} - {pex.Message}");
                try
                {
                    await Client.SendCommandErrorAsync(e.Id, pex.Code, pex.Message).ConfigureAwait(false);
                }
                catch (Exception sendEx)
                {
                    BridgeLog.Error($"Failed to send error response: {sendEx.Message}");
                }
            }
            catch (Exception ex)
            {
                BridgeLog.Error($"Command execution failed: {ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}");
                try
                {
                    await Client.SendCommandErrorAsync(e.Id, ErrorCode.InternalError, ex.Message)
                        .ConfigureAwait(false);
                }
                catch (Exception sendEx)
                {
                    BridgeLog.Error($"Failed to send error response: {sendEx.Message}");
                }
            }
        }

        /// <summary>
        /// Set the capabilities to advertise during registration
        /// </summary>
        public void SetCapabilities(params string[] capabilities)
        {
            if (Client != null)
            {
                Client.Capabilities = capabilities;
            }
        }
    }
}
