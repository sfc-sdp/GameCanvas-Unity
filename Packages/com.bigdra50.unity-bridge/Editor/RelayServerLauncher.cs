using System;
using System.Diagnostics;
using System.IO;
using UnityBridge.Helpers;
using UnityEditor;
using UnityEngine;

namespace UnityBridge
{
    /// <summary>
    /// Manages the Relay Server process lifecycle.
    /// Uses SessionState to persist server PID across domain reloads.
    /// </summary>
    public sealed class RelayServerLauncher : IDisposable
    {
        private const string PackageSource = "git+https://github.com/bigdra50/unity-cli";
        private const string EditorPrefsKeyCommand = "UnityBridge.RelayServer.CustomCommand";
        private const string SessionStateKeyPid = "UnityBridge.RelayServer.Pid";
        private const string SessionStateKeyPort = "UnityBridge.RelayServer.Port";

        private static RelayServerLauncher _instance;
        public static RelayServerLauncher Instance => _instance ??= new RelayServerLauncher();

        private Process _serverProcess;
        private string _uvPath;
        private string _localDevPath;

        /// <summary>
        /// Check if server is running - either via Process object or via saved PID
        /// </summary>
        public bool IsRunning
        {
            get
            {
                // First check if we have a direct process reference
                if (_serverProcess is { HasExited: false })
                {
                    return true;
                }

                // Check if there's a saved PID from before domain reload
                var savedPid = SessionState.GetInt(SessionStateKeyPid, -1);
                if (savedPid <= 0) return false;
                try
                {
                    var process = Process.GetProcessById(savedPid);
                    if (!process.HasExited)
                    {
                        return true;
                    }
                    SessionState.EraseInt(SessionStateKeyPid);
                }
                catch
                {
                    // Process doesn't exist anymore
                    SessionState.EraseInt(SessionStateKeyPid);
                }

                return false;
            }
        }

        /// <summary>
        /// Get the port the server is running on (from SessionState)
        /// </summary>
        public int CurrentPort => SessionState.GetInt(SessionStateKeyPort, ProtocolConstants.DefaultPort);

        public event EventHandler<string> OutputReceived;
        public event EventHandler<string> ErrorReceived;
        public event EventHandler ServerStarted;
        public event EventHandler ServerStopped;

        /// <summary>
        /// Custom command saved in EditorPrefs
        /// </summary>
        public string CustomCommand
        {
            get => EditorPrefs.GetString(EditorPrefsKeyCommand, "");
            set => EditorPrefs.SetString(EditorPrefsKeyCommand, value);
        }

        private RelayServerLauncher()
        {
            EditorApplication.quitting += OnEditorQuitting;
            DetectPaths();
        }

        private void DetectPaths()
        {
            _uvPath = FindExecutable("uv");

#if UNITY_BRIDGE_LOCAL_DEV
            // Force local development mode with define symbol
            var projectRoot = Path.GetDirectoryName(Application.dataPath) ?? ".";
            var relayPath = Path.Combine(projectRoot, "relay");
            if (Directory.Exists(relayPath))
            {
                _localDevPath = projectRoot;
                BridgeLog.Verbose($"UNITY_BRIDGE_LOCAL_DEV enabled, using local path: {_localDevPath}");
            }
            else
            {
                BridgeLog.Warn("UNITY_BRIDGE_LOCAL_DEV enabled but relay/ not found in project root");
            }
#endif
        }

        /// <summary>
        /// Extract numeric version from Python directory name (e.g. "Python312" → 312).
        /// Returns 0 for unparseable names so they sort last.
        /// </summary>
        private static int ExtractPythonVersion(string dirName)
        {
            // Strip "Python" prefix and parse remainder as integer
            if (dirName.StartsWith("Python", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(dirName.Substring(6), out var version))
            {
                return version;
            }
            return 0;
        }

        private static string[] _cachedSearchPaths;

        /// <summary>
        /// Platform-specific search paths for common binary locations.
        /// Results are cached after the first call.
        /// </summary>
        private static string[] GetPlatformSearchPaths()
        {
            if (_cachedSearchPaths != null)
                return _cachedSearchPaths;

            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                var pythonPaths = new System.Collections.Generic.List<string>();

                // Discover installed Python versions dynamically
                try
                {
                    var pythonBase = Path.Combine(localAppData, "Programs", "Python");
                    if (Directory.Exists(pythonBase))
                    {
                        var dirs = Directory.GetDirectories(pythonBase, "Python*");
                        // Sort by extracted version number (descending) to prefer newest
                        System.Array.Sort(dirs, (a, b) =>
                        {
                            var verA = ExtractPythonVersion(Path.GetFileName(a));
                            var verB = ExtractPythonVersion(Path.GetFileName(b));
                            return verB.CompareTo(verA);
                        });
                        foreach (var dir in dirs)
                        {
                            var scripts = Path.Combine(dir, "Scripts");
                            if (Directory.Exists(scripts))
                                pythonPaths.Add(scripts);
                        }
                    }
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    BridgeLog.Warn($"Error scanning Python directories: {ex.Message}");
                }

                var paths = new System.Collections.Generic.List<string>(pythonPaths);
                paths.Add(Path.Combine(appData, "Python", "Scripts"));
                paths.Add(Path.Combine(homeDir, ".local", "bin"));
                paths.Add(Path.Combine(homeDir, ".cargo", "bin"));

                _cachedSearchPaths = paths.ToArray();
            }
            else
            {
                _cachedSearchPaths = new[]
                {
                    "/opt/homebrew/bin",
                    "/usr/local/bin",
                    "/usr/bin",
                    "/bin",
                    Path.Combine(homeDir, ".local", "bin"),
                    Path.Combine(homeDir, ".cargo", "bin"),
                };
            }

            return _cachedSearchPaths;
        }

        /// <summary>
        /// Build augmented PATH that includes common binary locations.
        /// Unity GUI app has limited PATH, so we need to add common paths explicitly.
        /// </summary>
        private static string BuildAugmentedPath()
        {
            var additionalPaths = GetPlatformSearchPaths();
            var currentPath = Environment.GetEnvironmentVariable("PATH") ?? "";
            var separator = Application.platform == RuntimePlatform.WindowsEditor ? ";" : ":";
            return string.Join(separator, additionalPaths) + separator + currentPath;
        }

        /// <summary>
        /// Kill any process listening on the specified port
        /// </summary>
        private static void KillProcessOnPort(int port)
        {
            try
            {
                var pid = GetProcessIdForPort(port);
                if (pid > 0)
                {
                    BridgeLog.Verbose($"Killing existing process on port {port} (PID: {pid})");
                    KillProcess(pid);
                    // Wait a bit for the port to be released
                    System.Threading.Thread.Sleep(500);
                }
            }
            catch (Exception ex)
            {
                BridgeLog.Warn($"Error killing process on port {port}: {ex.Message}");
            }
        }

        private static int GetProcessIdForPort(int port)
        {
            try
            {
#if UNITY_EDITOR_WIN
                return GetProcessIdForPortWindows(port);
#else
                return GetProcessIdForPortUnix(port);
#endif
            }
            catch (Exception ex)
            {
                BridgeLog.Warn($"Error checking port {port}: {ex.Message}");
                return -1;
            }
        }

#if UNITY_EDITOR_WIN
        private static int GetProcessIdForPortWindows(int port)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "netstat",
                Arguments = "-ano",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null) return -1;

            var output = process.StandardOutput.ReadToEnd();
            if (!process.WaitForExit(5000))
            {
                try { process.Kill(); } catch { /* best effort */ }
            }

            var portSuffix = $":{port}";
            foreach (var line in output.Split('\n'))
            {
                var trimmed = line.Trim();
                if (!trimmed.StartsWith("TCP", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!trimmed.Contains(portSuffix))
                    continue;

                var parts = trimmed.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                // Format: TCP  <local>  <foreign>  <state>  <PID>
                if (parts.Length < 5)
                    continue;

                var colonIdx = parts[1].LastIndexOf(':');
                if (colonIdx < 0 || parts[1].Substring(colonIdx + 1) != port.ToString())
                    continue;

                if (parts[^2].Equals("LISTENING", StringComparison.OrdinalIgnoreCase)
                    && int.TryParse(parts[^1], out var pid))
                {
                    return pid;
                }
            }

            return -1;
        }
#else
        private static int GetProcessIdForPortUnix(int port)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "/usr/sbin/lsof",
                Arguments = $"-i :{port} -t",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null) return -1;

            var output = process.StandardOutput.ReadToEnd().Trim();
            if (!process.WaitForExit(5000))
            {
                try { process.Kill(); } catch { /* best effort */ }
            }

            if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
            {
                var firstLine = output.Split('\n')[0].Trim();
                if (int.TryParse(firstLine, out var pid))
                {
                    return pid;
                }
            }

            return -1;
        }
#endif

        private static void KillProcess(int pid)
        {
            try
            {
                using var process = Process.GetProcessById(pid);
                if (!process.HasExited)
                {
                    process.Kill();
                    process.WaitForExit(3000);
                }
            }
            catch (ArgumentException)
            {
                // Process already exited
            }
            catch (Exception ex)
            {
                BridgeLog.Warn($"Error killing process {pid}: {ex.Message}");
            }
        }

        private static string FindExecutable(string name)
        {
            // First check platform-specific search paths
            var searchPaths = GetPlatformSearchPaths();

            foreach (var basePath in searchPaths)
            {
                var fullPath = Path.Combine(basePath, name);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }

                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    var exePath = fullPath + ".exe";
                    if (File.Exists(exePath))
                    {
                        return exePath;
                    }
                }
            }

            // Also check current PATH
            var pathEnv = Environment.GetEnvironmentVariable("PATH") ?? "";
            var separator = Application.platform == RuntimePlatform.WindowsEditor ? ';' : ':';
            var paths = pathEnv.Split(separator);

            foreach (var basePath in paths)
            {
                if (string.IsNullOrEmpty(basePath)) continue;

                var fullPath = Path.Combine(basePath, name);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }

                if (Application.platform == RuntimePlatform.WindowsEditor)
                {
                    var exePath = fullPath + ".exe";
                    if (File.Exists(exePath))
                    {
                        return exePath;
                    }
                }
            }

            return null;
        }

        public void Start(int port = ProtocolConstants.DefaultPort)
        {
            if (IsRunning)
            {
                BridgeLog.Warn("Relay Server is already running");
                return;
            }

            // Kill any existing process on the port
            KillProcessOnPort(port);

            try
            {
                var startInfo = BuildStartInfo(port);
                if (startInfo == null)
                {
                    return;
                }

                _serverProcess = new Process { StartInfo = startInfo, EnableRaisingEvents = true };

                _serverProcess.OutputDataReceived += (_, e) =>
                {
                    if (string.IsNullOrEmpty(e.Data)) return;
                    BridgeLog.Relay(e.Data);
                    OutputReceived?.Invoke(this, e.Data);
                };

                _serverProcess.ErrorDataReceived += (_, e) =>
                {
                    if (string.IsNullOrEmpty(e.Data)) return;
                    if (e.Data.Contains("ERROR") || e.Data.Contains("Exception"))
                    {
                        BridgeLog.RelayError(e.Data);
                    }
                    else
                    {
                        BridgeLog.Relay(e.Data);
                    }

                    ErrorReceived?.Invoke(this, e.Data);
                };

                _serverProcess.Exited += (_, _) =>
                {
                    BridgeLog.Info("Relay Server stopped");
                    ServerStopped?.Invoke(this, EventArgs.Empty);
                };

                _serverProcess.Start();
                _serverProcess.BeginOutputReadLine();
                _serverProcess.BeginErrorReadLine();

                // Save PID and port to SessionState for domain reload persistence
                SessionState.SetInt(SessionStateKeyPid, _serverProcess.Id);
                SessionState.SetInt(SessionStateKeyPort, port);

                BridgeLog.Info($"Relay Server started (PID: {_serverProcess.Id}, Port: {port})");
                BridgeLog.Verbose($"Command: {startInfo.FileName} {startInfo.Arguments}");
                ServerStarted?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                BridgeLog.Error($"Failed to start Relay Server: {ex.Message}");
                _serverProcess = null;
            }
        }

        private ProcessStartInfo BuildStartInfo(int port)
        {
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                Environment =
                {
                    // Set augmented PATH for Unity GUI environment
                    ["PATH"] = BuildAugmentedPath(),
                    // Set PYTHONUNBUFFERED for real-time output
                    ["PYTHONUNBUFFERED"] = "1"
                }
            };

            // Priority 1: Custom command from EditorPrefs
            var customCmd = CustomCommand;
            if (!string.IsNullOrEmpty(customCmd))
            {
                var expanded = customCmd.Replace("{port}", port.ToString());
                return ParseShellCommand(expanded, startInfo);
            }

#if UNITY_BRIDGE_LOCAL_DEV
            // Priority 2: Local development (relay/ exists in project)
            if (!string.IsNullOrEmpty(_localDevPath))
            {
                if (_uvPath != null)
                {
                    startInfo.FileName = _uvPath;
                    startInfo.Arguments = $"run python -m relay.server --port {port}";
                    startInfo.WorkingDirectory = _localDevPath;
                    BridgeLog.Verbose($"Using local development mode: {_localDevPath}");
                    return startInfo;
                }

                var python = FindExecutable("python3") ?? FindExecutable("python");
                if (python != null)
                {
                    startInfo.FileName = python;
                    startInfo.Arguments = $"-m relay.server --port {port}";
                    startInfo.WorkingDirectory = _localDevPath;
                    return startInfo;
                }
            }
#endif

            // Priority 3: uvx (production)
            if (_uvPath != null)
            {
                startInfo.FileName = _uvPath;
                startInfo.Arguments = $"tool run --from {PackageSource} unity-relay --port {port}";
                BridgeLog.Verbose("Using uvx with remote package");
                return startInfo;
            }

            BridgeLog.Error(
                "Could not find uv. Please install uv (https://docs.astral.sh/uv/) or set a custom command.");
            return null;
        }

        private ProcessStartInfo ParseShellCommand(string command, ProcessStartInfo baseInfo)
        {
            // Simple parsing: first word is executable, rest is arguments
            var parts = command.Trim().Split(new[] { ' ' }, 2);
            var executable = parts[0];

            // Resolve common commands to their full paths
            executable = executable switch
            {
                "uv" => _uvPath ?? FindExecutable("uv") ?? executable,
                "python" => FindExecutable("python") ?? executable,
                "python3" => FindExecutable("python3") ?? executable,
                _ => executable
            };

            baseInfo.FileName = executable;
            baseInfo.Arguments = parts.Length > 1 ? parts[1] : "";
            return baseInfo;
        }

        public void Stop()
        {
            if (!IsRunning)
            {
                return;
            }

            try
            {
                BridgeLog.Verbose("Stopping Relay Server...");

                // Try to stop via direct process reference first
                if (_serverProcess != null && !_serverProcess.HasExited)
                {
                    _serverProcess.Kill();
                }
                else
                {
                    // Fallback: use saved PID from SessionState (after domain reload)
                    var savedPid = SessionState.GetInt(SessionStateKeyPid, -1);
                    if (savedPid <= 0) return;
                    BridgeLog.Verbose($"Killing server by saved PID: {savedPid}");
                    KillProcess(savedPid);
                }
            }
            catch (Exception ex)
            {
                BridgeLog.Warn($"Error stopping server: {ex.Message}");
            }
            finally
            {
                try
                {
                    _serverProcess?.Dispose();
                }
                catch
                {
                    // Ignore dispose errors
                }

                _serverProcess = null;

                // Clear SessionState
                SessionState.EraseInt(SessionStateKeyPid);
                SessionState.EraseInt(SessionStateKeyPort);

                BridgeLog.Info("Relay Server stopped");

                // Fire ServerStopped event so listeners can clean up
                // Note: Process.Exited event may not fire when we call Kill() + Dispose()
                try
                {
                    ServerStopped?.Invoke(this, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    BridgeLog.Warn($"ServerStopped event error: {ex.Message}");
                }
            }
        }

        private void OnEditorQuitting()
        {
            Stop();
        }

        public void Dispose()
        {
            EditorApplication.quitting -= OnEditorQuitting;
            Stop();
            _instance = null;
        }

        /// <summary>
        /// Get detected environment info for display
        /// </summary>
        public (string mode, string detail) GetDetectedMode()
        {
            if (!string.IsNullOrEmpty(CustomCommand))
            {
                return ("Custom", CustomCommand);
            }

#if UNITY_BRIDGE_LOCAL_DEV
            if (!string.IsNullOrEmpty(_localDevPath))
            {
                return ("Local Dev (UNITY_BRIDGE_LOCAL_DEV)", _localDevPath);
            }
#endif

            if (_uvPath != null)
            {
                return ("uvx", PackageSource);
            }

            return ("Not Available", "Install uv or set custom command");
        }

        /// <summary>
        /// Get the command that will be executed to start the server.
        /// Returns null if no valid command can be constructed.
        /// </summary>
        public string GetServerCommand(int port = ProtocolConstants.DefaultPort)
        {
            // Priority 1: Custom command
            var customCmd = CustomCommand;
            if (!string.IsNullOrEmpty(customCmd))
            {
                return customCmd.Replace("{port}", port.ToString());
            }

#if UNITY_BRIDGE_LOCAL_DEV
            // Priority 2: Local development
            if (string.IsNullOrEmpty(_localDevPath))
                return _uvPath != null ? $"uvx --from {PackageSource} unity-relay --port {port}" : null;
            if (_uvPath != null)
            {
                return $"cd \"{_localDevPath}\" && uv run python -m relay.server --port {port}";
            }

            var python = FindExecutable("python3") ?? FindExecutable("python");
            if (python != null)
            {
                return $"cd \"{_localDevPath}\" && \"{python}\" -m relay.server --port {port}";
            }
#endif

            // Priority 3: uvx (production)
            return _uvPath != null ? $"uvx --from {PackageSource} unity-relay --port {port}" : null;
        }
    }
}
