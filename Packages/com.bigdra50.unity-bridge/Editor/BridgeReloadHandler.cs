using System;
using System.Threading;
using System.Threading.Tasks;
using UnityBridge.Helpers;
using UnityEditor;

namespace UnityBridge
{
    /// <summary>
    /// Handles domain reload events to maintain relay connection stability.
    /// Sends STATUS "reloading" before assembly reload and reconnects after.
    /// </summary>
    [InitializeOnLoad]
    public static class BridgeReloadHandler
    {
        // Use SessionState to persist connection info across domain reloads
        private const string SessionStateKeyWasConnected = "UnityBridge.ReloadHandler.WasConnected";
        private const string SessionStateKeyLastHost = "UnityBridge.ReloadHandler.LastHost";
        private const string SessionStateKeyLastPort = "UnityBridge.ReloadHandler.LastPort";

        private static bool WasConnected
        {
            get => SessionState.GetBool(SessionStateKeyWasConnected, false);
            set => SessionState.SetBool(SessionStateKeyWasConnected, value);
        }

        private static string LastHost
        {
            get => SessionState.GetString(SessionStateKeyLastHost, "127.0.0.1");
            set => SessionState.SetString(SessionStateKeyLastHost, value);
        }

        private static int LastPort
        {
            get => SessionState.GetInt(SessionStateKeyLastPort, ProtocolConstants.DefaultPort);
            set => SessionState.SetInt(SessionStateKeyLastPort, value);
        }

        static BridgeReloadHandler()
        {
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
            EditorApplication.quitting += OnEditorQuitting;

            BridgeLog.Verbose("Reload handler initialized");
        }

        /// <summary>
        /// Register the relay client to be managed during domain reloads
        /// </summary>
        public static void RegisterClient(IRelayClient client, string host, int port)
        {
            if (client != null && client.IsConnected)
            {
                WasConnected = true;
                LastHost = host;
                LastPort = port;
            }
        }

        /// <summary>
        /// Unregister the relay client
        /// </summary>
        public static void UnregisterClient()
        {
            WasConnected = false;
        }

        private static void OnBeforeAssemblyReload()
        {
            BridgeLog.Verbose("Before assembly reload");

            var manager = BridgeManager.Instance;
            if (manager is not { Client: { IsConnected: true } }) return;
            WasConnected = true;
            LastHost = manager.Host;
            LastPort = manager.Port;

            // Delegate STATUS send to EditorStateCache (single publisher)
            EditorStateCache.SetDomainReloading(true);
            EditorStateCache.FlushStatus();

            // File fallback: write status file for relay server to detect
            BridgeStatusFile.WriteStatus(
                manager.Client.InstanceId,
                UnityEngine.Application.productName,
                UnityEngine.Application.unityVersion,
                "reloading",
                manager.Host,
                manager.Port);
        }

        private static void OnAfterAssemblyReload()
        {
            BridgeLog.Verbose("After assembly reload");

            if (!WasConnected) return;
            WasConnected = false;

            // Use update callback instead of delayCall
            // delayCall doesn't fire until editor receives focus
            var host = LastHost;
            var port = LastPort;

            EditorApplication.update += ReconnectOnUpdate;
            return;

            void ReconnectOnUpdate()
            {
                EditorApplication.update -= ReconnectOnUpdate;
                ReconnectAsync(host, port);
            }
        }

        private static async void ReconnectAsync(string host, int port)
        {
            if (string.IsNullOrEmpty(host) || port <= 0)
            {
                BridgeLog.Error($"Reconnection failed: invalid parameters (host={host}, port={port})");
                return;
            }

            try
            {
                var manager = BridgeManager.Instance;
                if (manager == null)
                {
                    BridgeLog.Error("Reconnection failed: BridgeManager.Instance is null");
                    return;
                }

                await manager.ConnectAsync(host, port);
                BridgeLog.Verbose("Reconnected after reload");

                if (manager.Client is not { IsConnected: true }) return;
                EditorStateCache.SyncCurrentState();

                // Update status file to ready
                BridgeStatusFile.WriteStatus(
                    manager.Client.InstanceId,
                    UnityEngine.Application.productName,
                    UnityEngine.Application.unityVersion,
                    "ready",
                    host,
                    port);
            }
            catch (Exception ex)
            {
                BridgeLog.Error($"Reconnection failed: {ex.Message}");
            }
        }

        private static void OnEditorQuitting()
        {
            BridgeLog.Verbose("Editor quitting");

            var manager = BridgeManager.Instance;
            if (manager != null)
            {
                // Delete status file before quitting
                if (manager.Client != null)
                {
                    BridgeStatusFile.DeleteStatus(manager.Client.InstanceId);
                }

                // Fire-and-forget disconnect - don't block editor quit
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await manager.DisconnectAsync().ConfigureAwait(false);
                    }
                    catch
                    {
                        // Ignore - editor is quitting anyway
                    }
                });
            }

            WasConnected = false;
        }
    }
}
