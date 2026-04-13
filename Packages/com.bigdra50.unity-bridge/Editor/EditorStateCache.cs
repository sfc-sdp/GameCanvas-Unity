using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using UnityBridge.Helpers;
using UnityBridge.Tools;
using UnityEditor;
using UnityEditor.Compilation;

namespace UnityBridge
{
    /// <summary>
    /// Monitors editor activity phases and sends STATUS messages to the relay server.
    /// Single source of truth for STATUS publishing — BridgeReloadHandler delegates here.
    /// </summary>
    [InitializeOnLoad]
    internal static class EditorStateCache
    {
        private static string _currentPhase = ActivityPhase.Idle;
        private static string _lastSentPhase;
        private static bool _domainReloadPending;
        private static bool _compilationEventActive;
        private static double _lastUpdateTime;
        private const double MinUpdateIntervalSeconds = 1.0;

        // Watchdog: detect main thread stall (modal dialogs, etc.)
        private static long _lastUpdateTick;
        private static int _blockedSent; // 0 = not blocked, 1 = blocked (int for Interlocked)
        private static Timer _watchdogTimer;
        private const int WatchdogIntervalMs = 5000;
        private static readonly long BlockedThresholdTicks = Stopwatch.Frequency * 5; // 5 seconds

        private static readonly Func<bool> s_isCompiling;

        static EditorStateCache()
        {
            // Cache CompilationPipeline.isCompiling as a delegate to avoid reflection per tick
            var prop = typeof(CompilationPipeline)
                .GetProperty("isCompiling", BindingFlags.Static | BindingFlags.Public);
            if (prop != null)
                s_isCompiling = (Func<bool>)Delegate.CreateDelegate(
                    typeof(Func<bool>), prop.GetGetMethod());
            else
                s_isCompiling = () => EditorApplication.isCompiling;

            CompilationPipeline.compilationStarted += _ =>
            {
                _compilationEventActive = true;
                EvaluatePhase();
            };
            // compilationFinished: clear flag only. Actual transition deferred to watchdog tick
            // because Unity's isCompiling may still return true immediately after this event.
            CompilationPipeline.compilationFinished += _ => { _compilationEventActive = false; };

            EditorApplication.playModeStateChanged += _ => EvaluatePhase();
            EditorApplication.update += OnUpdate;

            // Watchdog timer: runs on background thread to detect main thread stalls
            _lastUpdateTick = Stopwatch.GetTimestamp();
            _watchdogTimer = new Timer(OnWatchdogTick, null, WatchdogIntervalMs, WatchdogIntervalMs);
            AssemblyReloadEvents.beforeAssemblyReload += () => _watchdogTimer?.Dispose();

            BridgeLog.Verbose("[EditorStateCache] Initialized");
        }

        /// <summary>
        /// Called by BridgeReloadHandler before domain reload.
        /// </summary>
        public static void SetDomainReloading(bool value)
        {
            Volatile.Write(ref _domainReloadPending, value);
        }

        /// <summary>
        /// Synchronously flush STATUS "reloading" before domain reload destroys this class.
        /// </summary>
        public static void FlushStatus()
        {
            var manager = BridgeManager.Instance;
            if (manager?.Client is not { IsConnected: true }) return;

            try
            {
                using var cts = new CancellationTokenSource(500);
                var task = manager.Client.SendReloadingStatusAsync();
                task.Wait(cts.Token);
                _lastSentPhase = "reloading";
            }
            catch (OperationCanceledException)
            {
                BridgeLog.Verbose("[EditorStateCache] FlushStatus timed out, relying on status file");
            }
            catch (AggregateException ex)
            {
                BridgeLog.Warn($"[EditorStateCache] FlushStatus failed: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        /// <summary>
        /// Called after reconnect to force-send the current phase.
        /// </summary>
        public static void SyncCurrentState()
        {
            var manager = BridgeManager.Instance;
            if (manager?.Client is not { IsConnected: true }) return;

            Volatile.Write(ref _domainReloadPending, false);
            _lastSentPhase = null; // force re-send
            EvaluatePhase();
        }

        private static void OnUpdate()
        {
            // Update tick for watchdog — must run every frame regardless of connection state
            Volatile.Write(ref _lastUpdateTick, Stopwatch.GetTimestamp());

            // If watchdog sent editor_blocked, force re-evaluation on recovery
            if (Volatile.Read(ref _blockedSent) != 0)
            {
                Volatile.Write(ref _blockedSent, 0);
                _lastSentPhase = null;
                BridgeLog.Verbose("[EditorStateCache] Main thread resumed, clearing editor_blocked");
            }

            // Skip evaluation when not connected — no one to send STATUS to
            var manager = BridgeManager.Instance;
            if (manager?.Client is not { IsConnected: true }) return;

            double now = EditorApplication.timeSinceStartup;
            if (now - _lastUpdateTime < MinUpdateIntervalSeconds) return;
            _lastUpdateTime = now;
            EvaluatePhase();
        }

        private static void EvaluatePhase()
        {
            if (Volatile.Read(ref _domainReloadPending)) return; // BridgeReloadHandler owns this state

            string phase;
            if (Tests.IsRunning)
                phase = ActivityPhase.RunningTests;
            else if (_compilationEventActive || s_isCompiling())
                phase = ActivityPhase.Compiling;
            else if (EditorApplication.isUpdating)
                phase = ActivityPhase.AssetImport;
            else if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
                phase = ActivityPhase.PlaymodeTransition;
            else
                phase = ActivityPhase.Idle;

            _currentPhase = phase;
            SendIfChanged(phase);
        }

        private static async void SendIfChanged(string phase)
        {
            if (phase == _lastSentPhase) return;

            var manager = BridgeManager.Instance;
            if (manager?.Client is not { IsConnected: true }) return;

            try
            {
                if (phase == ActivityPhase.Idle)
                    await manager.Client.SendStatusAsync(InstanceStatus.Ready);
                else
                    await manager.Client.SendStatusAsync(InstanceStatus.Busy, phase);

                _lastSentPhase = phase;
            }
            catch (Exception ex)
            {
                BridgeLog.Warn($"[EditorStateCache] Failed to send phase '{phase}': {ex.Message}");
                // _lastSentPhase not updated — will retry on next evaluation
            }
        }

        public static string CurrentPhase => _currentPhase;

        /// <summary>
        /// Background thread callback: detects when EditorApplication.update has stopped firing
        /// (e.g. modal dialog blocking main thread) and sends STATUS "busy" with "editor_blocked".
        /// </summary>
        private static async void OnWatchdogTick(object state)
        {
            if (Volatile.Read(ref _domainReloadPending)) return;

            long elapsed = Stopwatch.GetTimestamp() - Volatile.Read(ref _lastUpdateTick);
            if (elapsed < BlockedThresholdTicks) return;

            // Atomically claim the right to send — prevents duplicate sends from overlapping timer ticks
            if (Interlocked.CompareExchange(ref _blockedSent, 1, 0) != 0) return;

            var manager = BridgeManager.Instance;
            if (manager?.Client is not { IsConnected: true })
            {
                Volatile.Write(ref _blockedSent, 0);
                return;
            }

            try
            {
                await manager.Client.SendStatusAsync(InstanceStatus.Busy, ActivityPhase.EditorBlocked);
                _currentPhase = ActivityPhase.EditorBlocked;
                _lastSentPhase = ActivityPhase.EditorBlocked;
                BridgeLog.Verbose("[EditorStateCache] Main thread stall detected, sent editor_blocked");
            }
            catch (Exception ex)
            {
                Volatile.Write(ref _blockedSent, 0);
                BridgeLog.Warn($"[EditorStateCache] Failed to send editor_blocked: {ex.Message}");
            }
        }
    }
}
