#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

namespace GameCanvas.Engine
{
    internal readonly struct GcKeyRecord
    {
        internal readonly Key Key;
        internal readonly GcKeyEventPhase Phase;
        internal readonly double Time;
        internal GcKeyRecord(Key key, GcKeyEventPhase phase, double time)
        { Key = key; Phase = phase; Time = time; }
    }
    internal sealed class GcKeySource : IInputStateChangeMonitor, IDisposable
    {
        internal const int Capacity = 256;
        sealed class State { internal bool Held, Suppressed; }
        readonly Dictionary<KeyControl, State> controls = new();
        readonly List<KeyControl> scratch = new();
        readonly int[] holds = new int[Capacity];
        internal readonly List<GcKeyRecord> Pending = new(32);
        bool suspended;
        internal GcKeySource()
        {
            InputSystem.onDeviceChange += DeviceChange;
            foreach (var device in InputSystem.devices) Add(device);
        }
        void Add(InputDevice device)
        {
            if (device is not Keyboard keyboard || !device.enabled) return;
            foreach (var key in keyboard.allKeys)
            {
                if (controls.ContainsKey(key) || (int)key.keyCode >= Capacity) continue;
                controls.Add(key, new State { Suppressed = key.isPressed });
                InputState.AddChangeMonitor(key, this);
            }
        }
        void Cancel(KeyControl key, State state)
        {
            if (!state.Held) return;
            state.Held = false;
            if (--holds[(int)key.keyCode] == 0)
                Pending.Add(new GcKeyRecord(key.keyCode, GcKeyEventPhase.Cancelled, InputState.currentTime));
        }
        void DeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (device is not Keyboard) return;
            if (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected || change == InputDeviceChange.Enabled)
            { Add(device); return; }
            bool remove = change == InputDeviceChange.Removed || change == InputDeviceChange.Disconnected || change == InputDeviceChange.Disabled;
            if (!remove && change != InputDeviceChange.SoftReset && change != InputDeviceChange.HardReset) return;
            scratch.Clear();
            foreach (var pair in controls)
                if (pair.Key.device == device)
                {
                    Cancel(pair.Key, pair.Value);
                    pair.Value.Suppressed = true;
                    if (remove) scratch.Add(pair.Key);
                }
            foreach (var key in scratch) { InputState.RemoveChangeMonitor(key, this); controls.Remove(key); }
        }
        internal void SetSuspended(bool value)
        {
            if (suspended == value) return;
            suspended = value;
            foreach (var pair in controls)
            {
                if (value) Cancel(pair.Key, pair.Value);
                pair.Value.Suppressed = value || pair.Key.isPressed;
            }
        }
        public void NotifyControlStateChanged(InputControl control, double time, InputEventPtr eventPtr, long monitorIndex)
        {
            if (suspended || InputState.currentUpdateType == InputUpdateType.Editor || control is not KeyControl key || !controls.TryGetValue(key, out var state)) return;
            bool held = key.isPressed;
            if (state.Suppressed) { if (held) return; state.Suppressed = false; }
            if (state.Held == held) return;
            state.Held = held;
            int index = (int)key.keyCode;
            if (held && holds[index]++ == 0) Pending.Add(new GcKeyRecord(key.keyCode, GcKeyEventPhase.Down, time));
            else if (!held && --holds[index] == 0) Pending.Add(new GcKeyRecord(key.keyCode, GcKeyEventPhase.Up, time));
        }
        public void NotifyTimerExpired(InputControl control, double time, long monitorIndex, int timerIndex) { }
        public void Dispose()
        {
            InputSystem.onDeviceChange -= DeviceChange;
            foreach (var key in controls.Keys) InputState.RemoveChangeMonitor(key, this);
            controls.Clear(); Pending.Clear();
        }
    }
}
