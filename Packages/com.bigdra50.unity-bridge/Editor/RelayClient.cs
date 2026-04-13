using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityBridge.Helpers;

namespace UnityBridge
{
    /// <summary>
    /// Connection status for the relay client
    /// </summary>
    public enum ConnectionStatus
    {
        Disconnected,
        Connecting,
        Connected,
        Reloading
    }

    /// <summary>
    /// Event args for connection status changes
    /// </summary>
    public class ConnectionStatusChangedEventArgs : EventArgs
    {
        public ConnectionStatus OldStatus { get; }
        public ConnectionStatus NewStatus { get; }

        public ConnectionStatusChangedEventArgs(ConnectionStatus oldStatus, ConnectionStatus newStatus)
        {
            OldStatus = oldStatus;
            NewStatus = newStatus;
        }
    }

    /// <summary>
    /// Event args for received commands
    /// </summary>
    public class CommandReceivedEventArgs : EventArgs
    {
        public string Id { get; }
        public string Command { get; }
        public JObject Parameters { get; }
        public int TimeoutMs { get; }

        public CommandReceivedEventArgs(string id, string command, JObject parameters, int timeoutMs)
        {
            Id = id;
            Command = command;
            Parameters = parameters;
            TimeoutMs = timeoutMs;
        }
    }

    /// <summary>
    /// TCP client for connecting to the Relay Server.
    /// Handles registration, heartbeat, and command message routing.
    /// </summary>
    public class RelayClient : IRelayClient
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private CancellationTokenSource _cts;
        private Task _receiveTask;
        private Task _heartbeatTask;
        private readonly SemaphoreSlim _sendLock = new(1, 1);
        private bool _disposed;

        private readonly string _host;
        private readonly int _port;
        private int _heartbeatIntervalMs = ProtocolConstants.HeartbeatIntervalMs;

        private ConnectionStatus _status = ConnectionStatus.Disconnected;
        private readonly object _statusLock = new();

        /// <summary>
        /// Instance ID (project path)
        /// </summary>
        public string InstanceId { get; }

        /// <summary>
        /// Project name
        /// </summary>
        public string ProjectName { get; }

        /// <summary>
        /// Unity version
        /// </summary>
        public string UnityVersion { get; }

        /// <summary>
        /// Supported capabilities
        /// </summary>
        public string[] Capabilities { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Current connection status
        /// </summary>
        public ConnectionStatus Status
        {
            get
            {
                lock (_statusLock)
                {
                    return _status;
                }
            }
            private set
            {
                ConnectionStatus oldStatus;
                lock (_statusLock)
                {
                    if (_status == value)
                        return;
                    oldStatus = _status;
                    _status = value;
                }

                BridgeLog.Verbose($"Status: {oldStatus} -> {value}");
                StatusChanged?.Invoke(this, new ConnectionStatusChangedEventArgs(oldStatus, value));
            }
        }

        /// <summary>
        /// Whether the client is currently connected
        /// </summary>
        public bool IsConnected => Status == ConnectionStatus.Connected;

        /// <summary>
        /// Event fired when connection status changes
        /// </summary>
        public event EventHandler<ConnectionStatusChangedEventArgs> StatusChanged;

        /// <summary>
        /// Event fired when a command is received from the relay server
        /// </summary>
        public event EventHandler<CommandReceivedEventArgs> CommandReceived;

        /// <summary>
        /// Create a new relay client
        /// </summary>
        public RelayClient(string host = "127.0.0.1", int port = ProtocolConstants.DefaultPort)
        {
            _host = host;
            _port = port;

            InstanceId = InstanceIdHelper.GetInstanceId();
            ProjectName = InstanceIdHelper.GetProjectName(InstanceId);
            UnityVersion = UnityEngine.Application.unityVersion;
        }

        /// <summary>
        /// Connect to the relay server and register this instance
        /// </summary>
        public async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            if (Status == ConnectionStatus.Connected || Status == ConnectionStatus.Connecting)
            {
                BridgeLog.Warn("Already connected or connecting");
                return;
            }

            Status = ConnectionStatus.Connecting;
            _cts = new CancellationTokenSource();

            try
            {
                BridgeLog.Verbose($"Connecting to {_host}:{_port}...");

                _client = new TcpClient();
                await _client.ConnectAsync(_host, _port);
                _stream = _client.GetStream();

                // Send REGISTER
                var registerMsg = Messages.CreateRegister(
                    InstanceId,
                    ProjectName,
                    UnityVersion,
                    Capabilities);

                await _sendLock.WaitAsync(cancellationToken);
                try
                {
                    await Framing.WriteFrameAsync(_stream, registerMsg, cancellationToken);
                }
                finally
                {
                    _sendLock.Release();
                }
                BridgeLog.Verbose($"Sent REGISTER: {InstanceId}");

                // Wait for REGISTERED response
                var response = await Framing.ReadFrameAsync(_stream, cancellationToken);
                var msgType = response["type"]?.Value<string>();

                if (msgType != MessageType.Registered)
                {
                    throw new ProtocolException(
                        ErrorCode.ProtocolError,
                        $"Expected REGISTERED, got: {msgType}");
                }

                var (success, heartbeatIntervalMs, errorCode, errorMessage) =
                    Messages.ParseRegistered(response);

                if (!success)
                {
                    throw new ProtocolException(
                        errorCode ?? ErrorCode.InternalError,
                        errorMessage ?? "Registration failed");
                }

                _heartbeatIntervalMs = heartbeatIntervalMs;
                Status = ConnectionStatus.Connected;
                BridgeLog.Info($"Connected to relay server ({_host}:{_port})");

                // Start receive and heartbeat tasks
                _receiveTask = Task.Run(() => ReceiveLoopAsync(_cts.Token), _cts.Token);
                _heartbeatTask = Task.Run(() => HeartbeatLoopAsync(_cts.Token), _cts.Token);
            }
            catch (Exception ex)
            {
                BridgeLog.Error($"Connection failed: {ex.Message}");
                await DisconnectInternalAsync();
                throw;
            }
        }

        /// <summary>
        /// Disconnect from the relay server
        /// </summary>
        public async Task DisconnectAsync()
        {
            BridgeLog.Verbose("Disconnecting...");
            await DisconnectInternalAsync();
        }

        /// <summary>
        /// Send a STATUS message with optional detail.
        /// Throws on send failure — callers are responsible for error handling.
        /// </summary>
        public async Task SendStatusAsync(string status, string detail = null)
        {
            if (_stream == null || _client is not { Connected: true })
                return;

            var statusMsg = Messages.CreateStatus(InstanceId, status, detail);
            var token = _cts?.Token ?? CancellationToken.None;
            if (!await _sendLock.WaitAsync(5000, token))
            {
                BridgeLog.Warn($"SendStatusAsync lock timeout, skipping STATUS: {status}");
                return;
            }
            try
            {
                await Framing.WriteFrameAsync(_stream, statusMsg, token);
            }
            finally
            {
                _sendLock.Release();
            }
            BridgeLog.Verbose($"Sent STATUS: {status}" + (detail != null ? $" ({detail})" : ""));
        }

        /// <summary>
        /// Send a STATUS message to indicate reloading state.
        /// Sets ConnectionStatus.Reloading regardless of send outcome.
        /// </summary>
        public async Task SendReloadingStatusAsync()
        {
            if (_stream == null || _client is not { Connected: true })
                return;

            Status = ConnectionStatus.Reloading;
            await SendStatusAsync(InstanceStatus.Reloading);
        }

        /// <summary>
        /// Send a STATUS message to indicate ready state
        /// </summary>
        public async Task SendReadyStatusAsync()
        {
            await SendStatusAsync(InstanceStatus.Ready);
        }

        /// <summary>
        /// Send a command result back to the relay server
        /// </summary>
        public async Task SendCommandResultAsync(string id, JObject data)
        {
            var msg = Messages.CreateCommandResult(id, data);
            await SendFrameAsync(msg, $"COMMAND_RESULT: {id}");
        }

        /// <summary>
        /// Send a command error result back to the relay server
        /// </summary>
        public async Task SendCommandErrorAsync(string id, string code, string message)
        {
            var msg = Messages.CreateCommandResultError(id, code, message);
            await SendFrameAsync(msg, $"COMMAND_RESULT (error): {id} - {code}");
        }

        private async Task SendFrameAsync(JObject message, string logLabel)
        {
            if (_stream == null || _client is not { Connected: true })
            {
                BridgeLog.Warn($"Cannot send {logLabel}: not connected");
                return;
            }

            try
            {
                await _sendLock.WaitAsync(_cts?.Token ?? CancellationToken.None);
                try
                {
                    await Framing.WriteFrameAsync(_stream, message);
                }
                finally
                {
                    _sendLock.Release();
                }
                BridgeLog.Verbose($"Sent {logLabel}");
            }
            catch (OperationCanceledException)
            {
                BridgeLog.Verbose($"Send cancelled: {logLabel}");
            }
            catch (Exception ex)
            {
                BridgeLog.Error($"Failed to send {logLabel}: {ex.Message}");
            }
        }

        private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
        {
            BridgeLog.Verbose("Receive loop started");

            try
            {
                while (!cancellationToken.IsCancellationRequested && _client is { Connected: true })
                {
                    var msg = await Framing.ReadFrameAsync(_stream, cancellationToken);
                    await HandleMessageAsync(msg, cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation
            }
            catch (Exception ex)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    BridgeLog.Error($"Receive loop error: {ex.Message}");
                    await DisconnectInternalAsync();
                }
            }

            BridgeLog.Verbose("Receive loop ended");
        }

        private async Task HandleMessageAsync(JObject msg, CancellationToken cancellationToken)
        {
            var msgType = msg["type"]?.Value<string>();

            switch (msgType)
            {
                case MessageType.Ping:
                    await HandlePingAsync(msg, cancellationToken);
                    break;

                case MessageType.Command:
                    HandleCommand(msg);
                    break;

                default:
                    BridgeLog.Warn($"Unknown message type: {msgType}");
                    break;
            }
        }

        private async Task HandlePingAsync(JObject msg, CancellationToken cancellationToken)
        {
            var pingTs = Messages.ParsePing(msg);
            var pongMsg = Messages.CreatePong(pingTs);

            // Use a short timeout to avoid heartbeat failure when _sendLock is contended
            // by STATUS messages from EditorStateCache
            if (await _sendLock.WaitAsync(3000, cancellationToken))
            {
                try
                {
                    await Framing.WriteFrameAsync(_stream, pongMsg, cancellationToken);
                }
                finally
                {
                    _sendLock.Release();
                }
            }
            else
            {
                BridgeLog.Warn("PONG send lock timeout — sending without lock");
                await Framing.WriteFrameAsync(_stream, pongMsg, cancellationToken);
            }
        }

        private void HandleCommand(JObject msg)
        {
            var (id, command, parameters, timeoutMs) = Messages.ParseCommand(msg);
            BridgeLog.Verbose($"Received COMMAND: {command} (id: {id})");

            // Fire event on main thread
            var args = new CommandReceivedEventArgs(id, command, parameters, timeoutMs);

            // Since Unity isn't thread-safe, we need to dispatch to main thread
            // This will be handled by the subscriber (e.g., CommandDispatcher)
            CommandReceived?.Invoke(this, args);
        }

        private async Task HeartbeatLoopAsync(CancellationToken cancellationToken)
        {
            BridgeLog.Verbose("Heartbeat monitor started");

            try
            {
                while (!cancellationToken.IsCancellationRequested && _client is { Connected: true })
                {
                    await Task.Delay(_heartbeatIntervalMs, cancellationToken);

                    // The server sends PING, we respond with PONG
                    // This loop is just for monitoring/cleanup if needed
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation
            }
            catch (Exception ex)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    BridgeLog.Error($"Heartbeat error: {ex.Message}");
                }
            }

            BridgeLog.Verbose("Heartbeat monitor ended");
        }

        private async Task DisconnectInternalAsync()
        {
            // Guard against re-entrant calls
            lock (_statusLock)
            {
                if (_status == ConnectionStatus.Disconnected)
                    return;
            }

            // Set status first to prevent race conditions
            // Use direct field assignment to avoid event firing during cleanup
            ConnectionStatus oldStatus;
            lock (_statusLock)
            {
                oldStatus = _status;
                _status = ConnectionStatus.Disconnected;
            }

            try
            {
                // Cancel token first to signal all async operations to stop
                try
                {
                    _cts?.Cancel();
                }
                catch (ObjectDisposedException)
                {
                    // Already disposed
                }

                // Capture references before nulling to avoid race conditions
                var stream = _stream;
                var client = _client;
                var receiveTask = _receiveTask;
                var heartbeatTask = _heartbeatTask;
                var cts = _cts;

                _stream = null;
                _client = null;
                _receiveTask = null;
                _heartbeatTask = null;
                _cts = null;

                // Close stream/client first to unblock any pending reads
                try
                {
                    stream?.Close();
                }
                catch
                {
                    // Ignore - stream may already be closed
                }

                try
                {
                    client?.Close();
                }
                catch
                {
                    // Ignore - client may already be closed
                }

                // Wait briefly for tasks to complete (non-blocking)
                // Use ConfigureAwait(false) to avoid deadlock on UI thread
                var tasksToWait = new System.Collections.Generic.List<Task>();
                if (receiveTask is { IsCompleted: false })
                    tasksToWait.Add(receiveTask);
                if (heartbeatTask is { IsCompleted: false })
                    tasksToWait.Add(heartbeatTask);

                if (tasksToWait.Count > 0)
                {
                    try
                    {
                        // Wait with timeout - don't block forever
                        await Task.WhenAny(
                            Task.WhenAll(tasksToWait),
                            Task.Delay(500)
                        ).ConfigureAwait(false);
                    }
                    catch
                    {
                        // Ignore - tasks may have faulted
                    }
                }

                // Dispose resources
                try
                {
                    stream?.Dispose();
                }
                catch
                {
                    // Ignore
                }

                try
                {
                    client?.Dispose();
                }
                catch
                {
                    // Ignore
                }

                try
                {
                    cts?.Dispose();
                }
                catch
                {
                    // Ignore
                }
            }
            finally
            {
                // Fire status changed event after all cleanup is done
                // Only fire if status actually changed
                if (oldStatus != ConnectionStatus.Disconnected)
                {
                    BridgeLog.Verbose($"Status: {oldStatus} -> Disconnected");
                    try
                    {
                        StatusChanged?.Invoke(this,
                            new ConnectionStatusChangedEventArgs(oldStatus, ConnectionStatus.Disconnected));
                    }
                    catch (Exception ex)
                    {
                        BridgeLog.Warn($"StatusChanged event handler error: {ex.Message}");
                    }
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;
            _disposed = true;

            await DisconnectInternalAsync();
            _sendLock.Dispose();
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;

            // Synchronous fallback for contexts that cannot use DisposeAsync.
            // Fire and forget to avoid blocking Unity main thread.
            _ = DisconnectInternalAsync();
            _sendLock.Dispose();
        }
    }
}
