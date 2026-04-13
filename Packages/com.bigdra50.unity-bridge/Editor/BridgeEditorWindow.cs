using System;
using UnityEditor;
using UnityEngine;

namespace UnityBridge
{
    /// <summary>
    /// Editor window for managing the Unity Bridge connection
    /// </summary>
    public class BridgeEditorWindow : EditorWindow
    {
        private const string EditorPrefsKeyHost = "UnityBridge.Connection.Host";
        private const string EditorPrefsKeyPort = "UnityBridge.Connection.Port";

        private string _host = "127.0.0.1";
        private int _port = ProtocolConstants.DefaultPort;
        private Vector2 _scrollPosition;
        private string _statusMessage = "";
        private MessageType _statusMessageType = MessageType.None;
        private bool _showServerSettings;
        private string _customCommand = "";
        private ConnectionStatus _lastStatus = ConnectionStatus.Disconnected;

        private enum MessageType
        {
            None,
            Info,
            Warning,
            Error
        }

        [MenuItem("Window/Unity Bridge")]
        public static void ShowWindow()
        {
            var window = GetWindow<BridgeEditorWindow>("Unity Bridge");
            window.minSize = new Vector2(300, 200);
        }

        private void OnEnable()
        {
            _host = EditorPrefs.GetString(EditorPrefsKeyHost, "127.0.0.1");
            _port = Mathf.Clamp(
                EditorPrefs.GetInt(EditorPrefsKeyPort, ProtocolConstants.DefaultPort),
                1,
                65535);

            RelayServerLauncher.Instance.ServerStarted += OnServerStateChanged;
            RelayServerLauncher.Instance.ServerStopped += OnServerStateChanged;
            EditorApplication.update += PollConnectionStatus;
        }

        private void OnDisable()
        {
            RelayServerLauncher.Instance.ServerStarted -= OnServerStateChanged;
            RelayServerLauncher.Instance.ServerStopped -= OnServerStateChanged;
            EditorApplication.update -= PollConnectionStatus;
        }

        private void PollConnectionStatus()
        {
            var current = BridgeManager.Instance.Client?.Status ?? ConnectionStatus.Disconnected;
            if (current == _lastStatus) return;

            _lastStatus = current;
            Repaint();
        }

        private async void OnServerStateChanged(object sender, EventArgs e)
        {
            if (!RelayServerLauncher.Instance.IsRunning && BridgeManager.Instance.IsConnected)
            {
                try
                {
                    await BridgeManager.Instance.DisconnectAsync();
                }
                catch
                {
                    // Ignore disconnect errors when server is already gone
                }
            }

            Repaint();
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            DrawServerSection();
            EditorGUILayout.Space(10);
            DrawConnectionSection();
            EditorGUILayout.Space(10);
            DrawStatusSection();

            EditorGUILayout.EndScrollView();
        }

        private void DrawServerSection()
        {
            EditorGUILayout.LabelField("Relay Server", EditorStyles.boldLabel);

            var launcher = RelayServerLauncher.Instance;
            var isRunning = launcher.IsRunning;

            // Server status
            var statusColor = isRunning ? new Color(0.3f, 0.8f, 0.3f) : new Color(0.6f, 0.6f, 0.6f);
            var statusText = isRunning ? "Running" : "Stopped";

            var originalColor = GUI.color;
            GUI.color = statusColor;
            EditorGUILayout.LabelField($"● {statusText}", EditorStyles.boldLabel);
            GUI.color = originalColor;

            // Detected mode
            var (mode, detail) = launcher.GetDetectedMode();
            EditorGUILayout.LabelField($"Mode: {mode}", EditorStyles.miniLabel);

            // Command preview
            DrawServerCommandPreview(launcher);

            // Start/Stop buttons
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledGroupScope(isRunning))
                {
                    if (GUILayout.Button("Start Server", GUILayout.Height(25)))
                    {
                        launcher.Start(_port);
                    }
                }

                using (new EditorGUI.DisabledGroupScope(!isRunning))
                {
                    if (GUILayout.Button("Stop Server", GUILayout.Height(25)))
                    {
                        launcher.Stop();
                    }
                }
            }

            // Settings foldout
            _showServerSettings = EditorGUILayout.Foldout(_showServerSettings, "Server Settings", true);
            if (!_showServerSettings) return;

            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField($"Detail: {detail}", EditorStyles.miniLabel);

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Custom Command (optional):", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("Use {port} as placeholder", EditorStyles.miniLabel);

            _customCommand = launcher.CustomCommand;
            var newCommand = EditorGUILayout.TextField(_customCommand);
            if (newCommand != _customCommand)
            {
                launcher.CustomCommand = newCommand;
                _customCommand = newCommand;
            }

            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField("Example: uv run --from git+https://... unity-relay --port {port}",
                EditorStyles.miniLabel);

            EditorGUI.indentLevel--;
        }

        private void DrawServerCommandPreview(RelayServerLauncher launcher)
        {
            var command = launcher.GetServerCommand(_port);
            if (string.IsNullOrEmpty(command))
            {
                EditorGUILayout.HelpBox("uv is not installed or no custom command is set.", UnityEditor.MessageType.Warning);
                return;
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Command to execute:", EditorStyles.miniLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                // Selectable text field for copying
                var textFieldStyle = new GUIStyle(EditorStyles.textField)
                {
                    wordWrap = true,
                    fixedHeight = 0
                };
                var height = textFieldStyle.CalcHeight(new GUIContent(command), EditorGUIUtility.currentViewWidth - 70);
                EditorGUILayout.SelectableLabel(command, textFieldStyle, GUILayout.Height(Mathf.Max(height, 20)));

                if (GUILayout.Button("Copy", GUILayout.Width(50), GUILayout.Height(20)))
                {
                    EditorGUIUtility.systemCopyBuffer = command;
                    _statusMessage = "Command copied to clipboard";
                    _statusMessageType = MessageType.Info;
                }
            }

            EditorGUILayout.LabelField("Paste this command in terminal to run manually.", EditorStyles.miniLabel);
            EditorGUILayout.Space(5);
        }

        private void DrawConnectionSection()
        {
            EditorGUILayout.LabelField("Connection", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Host:", GUILayout.Width(40));
                var newHost = EditorGUILayout.TextField(_host);
                if (newHost != _host)
                {
                    _host = newHost.Trim();
                    EditorPrefs.SetString(EditorPrefsKeyHost, _host);
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Port:", GUILayout.Width(40));
                var newPort = Mathf.Clamp(EditorGUILayout.IntField(_port), 1, 65535);
                if (newPort != _port)
                {
                    _port = newPort;
                    EditorPrefs.SetInt(EditorPrefsKeyPort, _port);
                }
            }

            EditorGUILayout.Space(5);

            var canConnect = _lastStatus == ConnectionStatus.Disconnected;
            var canDisconnect = _lastStatus == ConnectionStatus.Connected;

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledGroupScope(!canConnect))
                {
                    if (GUILayout.Button("Connect", GUILayout.Height(25)))
                    {
                        ConnectAsync();
                    }
                }

                using (new EditorGUI.DisabledGroupScope(!canDisconnect))
                {
                    if (GUILayout.Button("Disconnect", GUILayout.Height(25)))
                    {
                        DisconnectAsync();
                    }
                }
            }

            // Status message
            if (string.IsNullOrEmpty(_statusMessage)) return;
            EditorGUILayout.Space(5);

            var style = _statusMessageType switch
            {
                MessageType.Error => new GUIStyle(EditorStyles.helpBox)
                {
                    normal = { textColor = new Color(0.9f, 0.3f, 0.3f) }
                },
                MessageType.Warning => new GUIStyle(EditorStyles.helpBox)
                {
                    normal = { textColor = new Color(0.9f, 0.7f, 0.2f) }
                },
                _ => EditorStyles.helpBox
            };

            EditorGUILayout.LabelField(_statusMessage, style);
        }

        private void DrawStatusSection()
        {
            EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);

            var statusColor = _lastStatus switch
            {
                ConnectionStatus.Connected => new Color(0.3f, 0.8f, 0.3f),
                ConnectionStatus.Connecting => new Color(0.9f, 0.7f, 0.2f),
                ConnectionStatus.Reloading => new Color(0.9f, 0.7f, 0.2f),
                _ => new Color(0.6f, 0.6f, 0.6f)
            };

            var originalColor = GUI.color;
            GUI.color = statusColor;

            EditorGUILayout.LabelField($"● {_lastStatus}", EditorStyles.boldLabel);

            GUI.color = originalColor;

            var client = BridgeManager.Instance.Client;
            if (client != null)
            {
                EditorGUILayout.LabelField($"Instance ID: {client.InstanceId}");
                EditorGUILayout.LabelField($"Project: {client.ProjectName}");
                EditorGUILayout.LabelField($"Unity: {client.UnityVersion}");
            }

            EditorGUILayout.Space(10);

            // Log toggle
            var logEnabled = Helpers.BridgeLog.Enabled;
            var newLogEnabled = EditorGUILayout.Toggle("Console Logging", logEnabled);
            if (newLogEnabled != logEnabled)
            {
                Helpers.BridgeLog.Enabled = newLogEnabled;
            }
        }

        private async void ConnectAsync()
        {
            try
            {
                _statusMessage = "Connecting...";
                _statusMessageType = MessageType.Info;
                Repaint();

                await BridgeManager.Instance.ConnectAsync(_host, _port);

                _statusMessage = "Connected successfully";
                _statusMessageType = MessageType.Info;
            }
            catch (Exception ex)
            {
                _statusMessage = $"Connection failed: {ex.Message}";
                _statusMessageType = MessageType.Error;
            }

            Repaint();
        }

        private async void DisconnectAsync()
        {
            try
            {
                _statusMessage = "Disconnecting...";
                _statusMessageType = MessageType.Info;
                Repaint();

                // Use ConfigureAwait(true) to return to main thread for Repaint
                await BridgeManager.Instance.DisconnectAsync().ConfigureAwait(true);
                _statusMessage = "Disconnected";
                _statusMessageType = MessageType.Info;
            }
            catch (ObjectDisposedException)
            {
                // Already disconnected - this is fine
                _statusMessage = "Disconnected";
                _statusMessageType = MessageType.Info;
            }
            catch (Exception ex)
            {
                _statusMessage = $"Disconnect error: {ex.Message}";
                _statusMessageType = MessageType.Warning;
                Helpers.BridgeLog.Warn($"EditorWindow disconnect error: {ex}");
            }

            Repaint();
        }
    }
}
