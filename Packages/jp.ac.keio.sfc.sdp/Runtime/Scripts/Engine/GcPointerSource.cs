#nullable enable
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

namespace GameCanvas.Engine
{
    internal readonly struct GcPointerRecord
    {
        public readonly int Device, Contact;
        public readonly GcPointerType Kind;
        public readonly GcPointerEventPhase Phase;
        public readonly float2 Screen;
        public readonly double Time;
        public readonly bool Present, CancelDevice;
        public GcPointerRecord(int device, int contact, GcPointerType kind, GcPointerEventPhase phase,
            float2 screen, double time, bool present = true, bool cancelDevice = false)
        {
            Device = device; Contact = contact; Kind = kind; Phase = phase;
            Screen = screen; Time = time; Present = present; CancelDevice = cancelDevice;
        }
    }

    // 状態変更直後に値をコピーする。後からcontrolの最新値を読み、履歴を潰してはいけない。
    internal sealed class GcPointerSource : IInputStateChangeMonitor, IDisposable
    {
        sealed class Cursor
        {
            public bool Held, Suppressed, Present;
            public float2 Position;
        }
        readonly List<InputControl> controls = new(24);
        readonly Dictionary<int, Cursor> cursors = new();
        internal readonly List<GcPointerRecord> Pending = new(64);
        sealed class MergeSetting
        {
            internal int Users;
            internal bool Previous;
        }
        static readonly Dictionary<InputSettings, MergeSetting> mergeSettings = new();
        readonly InputSettings settings;
        bool suspended, disposed;

        internal GcPointerSource()
        {
            // UnityはDownと後続の移動も統合するため、短いドラッグの開始位置が失われる。
            // 複数のGameCanvasで共有し、最後の利用者が破棄されたときだけ元へ戻す。
            settings = InputSystem.settings;
            if (!mergeSettings.TryGetValue(settings, out var merge))
                mergeSettings.Add(settings, merge = new MergeSetting { Previous = settings.disableRedundantEventsMerging });
            merge.Users++;
            settings.disableRedundantEventsMerging = true;
            InputSystem.onDeviceChange += OnDeviceChange;
            foreach (var device in InputSystem.devices) AddDevice(device);
        }
        void AddDevice(InputDevice device)
        {
            if (!device.enabled) return;
            for (int i = 0; i < controls.Count; i++) if (controls[i].device == device) return;
            if (device is Touchscreen screen)
            {
                foreach (var touch in screen.touches) Watch(touch);
            }
            else if (device is Mouse || device is Pen)
            {
                var pointer = (Pointer)device;
                cursors[device.deviceId] = new Cursor { Position = pointer.position.ReadValue(),
                    Suppressed = pointer.press.isPressed };
                Watch(device);
                // Androidが登録する未使用のMouseもある。登録だけで原点に
                // ホバーを作らず、実際に届いた状態変更から座標を公開する。
            }
        }
        void Watch(InputControl control)
        {
            controls.Add(control);
            InputState.AddChangeMonitor(control, this);
        }
        void RemoveDevice(InputDevice device)
        {
            for (int i = controls.Count - 1; i >= 0; i--)
                if (controls[i].device == device)
                {
                    InputState.RemoveChangeMonitor(controls[i], this);
                    controls.RemoveAt(i);
                }
            cursors.Remove(device.deviceId);
        }
        void Cancel(int device) => Pending.Add(new GcPointerRecord(device, 0, GcPointerType.Unknown,
            GcPointerEventPhase.Cancelled, default, InputState.currentTime, false, true));
        void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (change == InputDeviceChange.Added || change == InputDeviceChange.Reconnected || change == InputDeviceChange.Enabled)
                AddDevice(device);
            else if (change == InputDeviceChange.Removed || change == InputDeviceChange.Disconnected || change == InputDeviceChange.Disabled)
            { Cancel(device.deviceId); RemoveDevice(device); }
            else if (change == InputDeviceChange.SoftReset || change == InputDeviceChange.HardReset)
            {
                Cancel(device.deviceId);
                if (cursors.TryGetValue(device.deviceId, out var cursor))
                { cursor.Held = false; cursor.Suppressed = true; cursor.Present = false; }
            }
        }
        internal void SetSuspended(bool value)
        {
            if (suspended == value) return;
            suspended = value;
            if (value) Cancel(-1);
            foreach (var pair in cursors)
            {
                pair.Value.Held = false;
                pair.Value.Suppressed = value ||
                    (InputSystem.GetDeviceById(pair.Key) is Pointer pointer && pointer.press.isPressed);
                pair.Value.Present = false;
            }
        }
        public void NotifyControlStateChanged(InputControl control, double time, InputEventPtr eventPtr, long monitorIndex)
        {
            if (suspended || InputState.currentUpdateType == InputUpdateType.Editor) return;
            if (control is TouchControl touch)
            {
                var state = touch.ReadValue();
                var phase = state.phase.ToGcPointerPhase();
                if (phase != GcPointerEventPhase.Invalid)
                    Pending.Add(new GcPointerRecord(touch.device.deviceId, state.touchId,
                        GcPointerType.Touch, phase, state.position, time));
            }
            else if (control is Pointer pointer) CaptureCursor(pointer, time);
        }
        void CaptureCursor(Pointer pointer, double time)
        {
            if (suspended || !cursors.TryGetValue(pointer.deviceId, out var cursor)) return;
            bool held = pointer.press.isPressed;
            var position = (float2)pointer.position.ReadValue();
            bool present = pointer is Mouse || held || ((Pen)pointer).inRange.isPressed;
            if (cursor.Suppressed)
            {
                if (held) return;
                cursor.Suppressed = false;
            }
            var phase = !present && cursor.Held ? GcPointerEventPhase.Cancelled :
                held && !cursor.Held ? GcPointerEventPhase.Begin :
                !held && cursor.Held ? GcPointerEventPhase.End :
                held ? GcPointerEventPhase.Hold : GcPointerEventPhase.Hover;
            if (held == cursor.Held && present == cursor.Present && math.all(position == cursor.Position)) return;
            Pending.Add(new GcPointerRecord(pointer.deviceId, 0,
                pointer is Mouse ? GcPointerType.Mouse : GcPointerType.Stylus, phase, position, time, present));
            cursor.Held = held; cursor.Position = position; cursor.Present = present;
        }
        public void NotifyTimerExpired(InputControl control, double time, long monitorIndex, int timerIndex) { }
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            InputSystem.onDeviceChange -= OnDeviceChange;
            foreach (var control in controls) InputState.RemoveChangeMonitor(control, this);
            controls.Clear(); cursors.Clear(); Pending.Clear();
            var merge = mergeSettings[settings];
            if (--merge.Users == 0)
            {
                if (settings.disableRedundantEventsMerging)
                    settings.disableRedundantEventsMerging = merge.Previous;
                mergeSettings.Remove(settings);
            }
        }
    }
}
