#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace GameCanvas
{
    public enum GcAccelerationState { Idle, Waiting, Running, Unsupported, Stopped, Failed }

    /// <summary>重力を含むg単位。X/Y/Zはキャンバス向き（生値のX,-Y,-Z）。時刻は入力の秒。</summary>
    public readonly struct GcAccelerationSample
    {
        public float X { get; }
        public float Y { get; }
        public float Z { get; }
        public float RawX { get; }
        public float RawY { get; }
        public float RawZ { get; }
        public double Time { get; }
        public double DeltaTime { get; }
        public GcAccelerationSample(float x, float y, float z, float rawX, float rawY, float rawZ, double time, double deltaTime)
        {
            X = x; Y = y; Z = z; RawX = rawX; RawY = rawY; RawZ = rawZ; Time = time; DeltaTime = deltaTime;
        }
        internal static GcAccelerationSample FromRaw(float rawX, float rawY, float rawZ, double time, double deltaTime)
            => new(rawX, -rawY, -rawZ, rawX, rawY, rawZ, time, deltaTime);
    }

    /// <summary>加速度計の新API。開始は操作時に呼び、中断後はもう一度Startする。</summary>
    public sealed class GcAccelerationService
    {
        internal GcAccelerationService() { }
        readonly List<GcAccelerationSample> events = new(256);
        InputStateHistory? history;
        Accelerometer? device;
        GcAccelerationSample last;
        double previousTime;
        bool hasPreviousTime;
        bool hasValue;
        bool paused;
        bool watching;
        bool closing;

        public GcAccelerationState Status { get; private set; }
        /// <summary>この更新で新しい標本が届いたかどうか。</summary>
        public bool Updated { get; private set; }
        /// <summary>Running中に標本を持っているとき真。0も有効な値。</summary>
        public bool HasValue => hasValue && Status == GcAccelerationState.Running;
        public float X => HasValue ? last.X : 0;
        public float Y => HasValue ? last.Y : 0;
        public float Z => HasValue ? last.Z : 0;
        public double Time => HasValue ? last.Time : 0;
        public double DeltaTime => HasValue ? last.DeltaTime : 0;
        public GcReadOnlyList<GcAccelerationSample> Events => new(events);
        public GcAccelerationSample Last => last;
        /// <summary>実装した環境か、実際に加速度計があるか。Webの加速度は今回の対応範囲外。</summary>
        public bool IsSupported =>
#if UNITY_WEBGL && !UNITY_EDITOR
            false;
#else
            HasPlatformImplementation || Accelerometer.current != null;
#endif

        /// <summary>標本の取得を開始する。周波数は希望のHz。非正・NaN・Infinityは例外で、今の状態は変えない。</summary>
        public void Start(float samplingRate = 60)
        {
            if (float.IsNaN(samplingRate) || float.IsInfinity(samplingRate) || samplingRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(samplingRate));
            Stop();
            if (paused) return;
            if (!IsSupported) { Status = GcAccelerationState.Unsupported; return; }
            var sensor = Accelerometer.current;
            if (sensor == null)
            {
                Status = GcAccelerationState.Unsupported;
                return;
            }
            Watch();
            try
            {
                device = sensor;
                history = new InputStateHistory(sensor.acceleration) { historyDepth = 256 };
                history.StartRecording();
                if (!sensor.enabled) InputSystem.EnableDevice(sensor);
                try { sensor.samplingFrequency = samplingRate; }
                catch (Exception) { }
                Status = GcAccelerationState.Waiting;
            }
            catch (Exception)
            {
                Fail(GcAccelerationState.Failed);
            }
        }

        /// <summary>標本と履歴を捨てて止める。背面からの自動再開はしない。</summary>
        public void Stop()
        {
            Close();
            Status = GcAccelerationState.Stopped;
        }

        internal void SetPaused(bool value)
        {
            paused = value;
            if (value) Stop();
        }

        internal void Tick()
        {
            Updated = false;
            events.Clear();
            if (Status != GcAccelerationState.Waiting && Status != GcAccelerationState.Running) return;
            var sensor = device;
            var recorded = history;
            if (sensor == null || !sensor.added || recorded == null)
            {
                Fail(GcAccelerationState.Failed);
                return;
            }
            try
            {
                var count = recorded.Count;
                for (int i = 0; i < count; i++)
                {
                    var record = recorded[i];
                    // 保存値を読む。device.ReadValue だと全レコードが最新値になる。
                    var raw = record.ReadValue<Vector3>();
                    var time = record.time;
                    if (double.IsNaN(time) || double.IsInfinity(time) || (hasPreviousTime && time < previousTime)) continue;
                    var dt = hasPreviousTime ? time - previousTime : 0;
                    var sample = GcAccelerationSample.FromRaw(raw.x, raw.y, raw.z, time, dt);
                    events.Add(sample);
                    previousTime = time;
                    hasPreviousTime = true;
                    last = sample;
                    hasValue = true;
                    Updated = true;
                    Status = GcAccelerationState.Running;
                }
                recorded.Clear();
            }
            catch (Exception)
            {
                Fail(GcAccelerationState.Failed);
            }
        }

        internal void Dispose()
        {
            Close();
            Status = GcAccelerationState.Stopped;
        }

        void Fail(GcAccelerationState state)
        {
            Close();
            Status = state;
        }

        void Close()
        {
            closing = true;
            Unwatch();
            Updated = false;
            hasValue = false;
            hasPreviousTime = false;
            last = default;
            events.Clear();
            var recorded = history;
            history = null;
            var sensor = device;
            device = null;
            if (recorded != null)
            {
                try { recorded.Dispose(); }
                catch (Exception) { }
            }
            if (sensor != null && sensor.added && sensor.enabled)
            {
                try { InputSystem.DisableDevice(sensor); }
                catch (Exception) { }
            }
            closing = false;
        }

        void Watch()
        {
            if (watching) return;
            InputSystem.onDeviceChange += OnDeviceChange;
            watching = true;
        }

        void Unwatch()
        {
            if (!watching) return;
            InputSystem.onDeviceChange -= OnDeviceChange;
            watching = false;
        }

        void OnDeviceChange(InputDevice changed, InputDeviceChange change)
        {
            if (closing || device == null || changed != device) return;
            if (Status != GcAccelerationState.Waiting && Status != GcAccelerationState.Running) return;
            if (change == InputDeviceChange.Removed || change == InputDeviceChange.Disconnected || change == InputDeviceChange.Disabled)
                Fail(GcAccelerationState.Failed);
        }

        static bool HasPlatformImplementation =>
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            true;
#else
            false;
#endif
    }
}
