#nullable enable
using System.Collections;
using UnityEngine;

namespace GameCanvas.Diagnostics
{
    /// <summary>実機の初期検証用。授業用APIの完成を示すサンプルではありません。</summary>
    public sealed class GcDeviceProbe : GameBase
    {
        public Font UiFont = null!;
        string location = "未要求", camera = "未要求", network = "未実行";
        bool locationBusy, cameraBusy, networkBusy, simulatorSmoke;
        GUIStyle? label, button;
        GcImage sky;
        Coroutine? locationRoutine, networkRoutine;
        bool pointerMode;
        readonly GcPointerDragDemo pointerDemo = new();
        int cameraFrames;
        GcCameraState lastCameraState;

        public override void InitGame()
        {
            gc.ChangeCanvasSize(720, 1280);
            Log("asset.image", GcAssets.TryGetImage("BlueSky.png", out sky) ? "catalog-loaded" : "missing");
            Log("startup", $"Unity={Application.unityVersion}; OS={SystemInfo.operatingSystem}; model={SystemInfo.deviceModel}");
            if (CameraSmokeRequested()) StartCoroutine(CameraSmoke());
            if (FeatureSmokeRequested()) StartCoroutine(FeatureSmoke());
#if UNITY_ANDROID && !UNITY_EDITOR
            if (AndroidFlag("gcLocationSmoke")) locationRoutine = StartCoroutine(Location());
#endif
#if UNITY_IOS && !UNITY_EDITOR
            simulatorSmoke = GcProbeShouldRunSimulatorSmoke() != 0;
            if (simulatorSmoke) StartCoroutine(SimulatorSmoke());
#endif
        }

        public override void DrawGame()
        {
            gc.ClearScreen();
            gc.SetColor(255, 255, 255);
            gc.SetRectAnchor(GcAnchor.UpperLeft);
            gc.DrawImage("BlueSky.png", 0, 0);
            gc.SetColor(0, 0, 0);
            gc.SetFontSize(36);
            gc.SetStringAnchor(GcAnchor.UpperLeft);
            gc.DrawString("画像と日本語の表示確認", 28, 60);
            gc.DrawString("青空・漢字・ひらがな・カタカナ", 28, 115);
            gc.DrawString($"時刻 {gc.TimeSinceStartup:F1} 秒", 28, 165);
            if (pointerMode) pointerDemo.Draw(gc);
            else if (gc.Camera.Status == GcCameraState.Running)
            {
                gc.SetColor(255, 255, 255);
                gc.SetRectAnchor(GcAnchor.UpperLeft);
                var scale = Mathf.Min(320f / gc.Camera.Width, 260f / gc.Camera.Height);
                gc.DrawCamera(28, 640, gc.Camera.Width * scale, gc.Camera.Height * scale);
            }
        }

        public override void UpdateGame()
        {
            if (pointerMode) pointerDemo.Update(gc);
            var key = gc.Key(GcKey.Space);
            if (key.Down || key.Up || key.Cancelled)
                Log("key.space", $"down={key.Down}; held={key.Held}; up={key.Up}; cancelled={key.Cancelled}; duration={key.Duration:F3}");
            cameraBusy = gc.Camera.Status == GcCameraState.RequestingPermission || gc.Camera.Status == GcCameraState.Waiting;
            if (lastCameraState != gc.Camera.Status)
            {
                lastCameraState = gc.Camera.Status;
                Log("camera.state", $"{gc.Camera.Status}; permission={gc.Camera.Permission}; error={gc.Camera.ErrorCode}");
            }
            camera = $"{gc.Camera.Status} / {gc.Camera.Permission}\n{gc.Camera.ErrorCode}";
            if (gc.Camera.Updated)
            {
                if (cameraFrames++ == 0) Log("camera.frame", $"{gc.Camera.Width}x{gc.Camera.Height}; rotation={gc.Camera.Rotation}; mirrored={gc.Camera.IsMirrored}; device={gc.Camera.Device?.DeviceName}");
            }
            if (gc.Camera.Status == GcCameraState.Running) camera = $"映像を取得 {gc.Camera.Width}x{gc.Camera.Height} / {cameraFrames}";
        }

        void OnGUI()
        {
            if (label == null)
            {
                label = new GUIStyle(GUI.skin.label) { font = UiFont, fontSize = 26, wordWrap = true };
                label.normal.textColor = Color.black;
                button = new GUIStyle(GUI.skin.button) { font = UiFont, fontSize = 28 };
            }
            var scale = Mathf.Min(Screen.width / 720f, Screen.height / 1280f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 720 * scale) / 2, (Screen.height - 1280 * scale) / 2, 0), Quaternion.identity, new Vector3(scale, scale, 1));
            if (GUI.Button(new Rect(370, 190, 320, 60), pointerMode ? "端末機能へ戻る" : "ポインターを確認", button))
            {
                pointerDemo.Reset();
                pointerMode = !pointerMode;
            }
            if (pointerMode) return;

            if (GUI.Button(new Rect(28, 280, 320, 65), "位置情報を取得", button) && !locationBusy && !cameraBusy)
                locationRoutine = StartCoroutine(Location());
            if (GUI.Button(new Rect(370, 280, 320, 65), "位置情報を停止", button))
            {
                if (locationRoutine != null) StopCoroutine(locationRoutine);
                locationBusy = false; gc.Location.Stop(); Input.location.Stop(); location = "停止"; Log("location.stop", location);
            }
            GUI.Label(new Rect(28, 355, 655, 115), location, label);
            if (GUI.Button(new Rect(28, 480, 320, 65), "カメラを開始", button) && !cameraBusy)
                StartCamera();
            if (GUI.Button(new Rect(370, 480, 320, 65), "カメラを停止", button)) StopCamera();
            GUI.Label(new Rect(28, 555, 655, 80), camera, label);
            if (GUI.Button(new Rect(28, 910, 655, 65), "HTTPS通信を確認", button) && !networkBusy)
                networkRoutine = StartCoroutine(Network());
            GUI.Label(new Rect(28, 985, 655, 100), network, label);
            if (GUI.Button(new Rect(28, 1090, 655, 65), "音を確認", button)) PlayProbeSound();
            GUI.Label(new Rect(28, 1170, 655, 110), "権限は各ボタンから要求します。\n映像の向きと日本語は目で確認してください。\n検証用アプリ / Unity " + Application.unityVersion, label);
        }

        IEnumerator Location()
        {
            locationBusy = true;
            location = "権限を確認中";
#if UNITY_WEBGL && !UNITY_EDITOR
            location = "この検証アプリのWeb測位は未対応です"; locationBusy = false; yield break;
#endif
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            gc.Location.Start();
            while (gc.Location.Status == GcLocationState.RequestingPermission || gc.Location.Status == GcLocationState.Waiting)
            {
                location = gc.Location.Status == GcLocationState.Waiting ? "測位を待っています（最大30秒）" : "権限を確認中";
                yield return null;
            }
            if (gc.Location.TryGetSample(out var sample))
            {
                location = $"{(sample.IsMock ? "模擬測位成功" : "測位成功")} / {gc.Location.Permission} / 精度 {sample.AccuracyMeters:F1} m";
                Log("location.sample", $"permission={gc.Location.Permission}; accuracy={sample.AccuracyMeters}; timestamp={sample.UnixTimeSeconds}; mock={sample.IsMock}");
            }
            else
            {
                location = $"{gc.Location.Status} / {gc.Location.Permission}\n{gc.Location.ErrorCode}";
                Log("location.failed", location);
            }
#else
            Input.location.Start(10, 1);
            var deadline = Time.realtimeSinceStartupAsDouble + 30;
            while (Input.location.status == LocationServiceStatus.Initializing && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            if (Input.location.status == LocationServiceStatus.Running)
            {
                var data = Input.location.lastData;
                location = $"測位成功 / 精度 {data.horizontalAccuracy:F1} m";
                Log("location.legacy.sample", $"accuracy={data.horizontalAccuracy}; timestamp={data.timestamp}");
            }
            else
            {
                location = $"測位 {Input.location.status} / enabled={Input.location.isEnabledByUser}";
                Log("location.legacy.failed", location);
            }
            Input.location.Stop();
#endif
            locationBusy = false;
        }

#if UNITY_IOS && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        static extern int GcProbeShouldRunSimulatorSmoke();
        [System.Runtime.InteropServices.DllImport("__Internal")]
        static extern int GcProbeShouldRunCameraSmoke();
        [System.Runtime.InteropServices.DllImport("__Internal")]
        static extern int GcProbeShouldRunFeatureSmoke();

        // simctlで許可と模擬座標を設定した専用シミュレータからだけ明示的に実行する。
        IEnumerator SimulatorSmoke()
        {
            yield return new WaitForSeconds(1);
            Log("simulator.smoke", "開始。位置はsimctlによる模擬データであり、実測ではありません");
            yield return Location();
            var firstRunning = gc.Location.Status == GcLocationState.Running;
            gc.Location.Stop();
            Log("simulator.stop", $"wasRunning={firstRunning}; hasSample={gc.Location.TryGetSample(out _)}; status={gc.Location.Status}");
            yield return Location();
            var hasRestarted = gc.Location.TryGetSample(out var restarted);
            Log("simulator.restart", $"status={gc.Location.Status}; hasSample={hasRestarted}; mock={(hasRestarted && restarted.IsMock)}");
            yield return Network();
            Log("simulator.camera", $"devices={WebCamTexture.devices.Length}; 実カメラの検証ではありません");
            Log("simulator.smoke", "完了");
        }
#endif

        void StartCamera()
        {
            cameraFrames = 0;
            gc.Camera.Start();
        }

        // Explicit diagnostic launch only. Exercises the public service; does not synthesize UI input.
        IEnumerator CameraSmoke()
        {
            yield return null;
            foreach (var facing in new[] { GcCameraFacing.Rear, GcCameraFacing.Front, GcCameraFacing.Front })
            {
                cameraFrames = 0;
                gc.Camera.Start(facing);
                var deadline = Time.realtimeSinceStartupAsDouble + 35;
                while ((gc.Camera.Status == GcCameraState.RequestingPermission || gc.Camera.Status == GcCameraState.Waiting) && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
                Log("camera.smoke.started", $"facing={facing}; status={gc.Camera.Status}; size={gc.Camera.Width}x{gc.Camera.Height}; device={gc.Camera.Device?.DeviceName}");
                yield return new WaitForSecondsRealtime(10);
                var frames = cameraFrames;
                gc.Camera.Stop();
                Log("camera.smoke.stopped", $"facing={facing}; frames={frames}; status={gc.Camera.Status}; size={gc.Camera.Width}x{gc.Camera.Height}; updated={gc.Camera.Updated}");
                yield return new WaitForSecondsRealtime(1);
            }
            Log("camera.smoke.complete", "done");
        }
        static bool CameraSmokeRequested()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
            using var intent = activity.Call<AndroidJavaObject>("getIntent");
            return intent.Call<bool>("getBooleanExtra", "gcCameraSmoke", false);
#elif UNITY_IOS && !UNITY_EDITOR
            return GcProbeShouldRunCameraSmoke() != 0;
#else
            return System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-gcCameraSmoke") >= 0;
#endif
        }

        IEnumerator Network()
        {
            networkBusy = true; network = "通信中";
            using var request = gc.Network.GetText("https://httpbingo.org/get", timeoutSeconds: 15);
            while (!request.IsDone) yield return null;
            network = $"{request.Status} / HTTP {request.ResponseCode}\n{request.ErrorCode}";
            Log("network.result", network); networkBusy = false;
        }

        public void PlayProbeSound()
        {
            if (!GcAssets.TryGetSound("Click1.wav", out var sound)) { Log("audio.result", "missing"); return; }
            gc.PlaySound(sound, GcSoundTrack.SE);
            Log("audio.result", $"started={gc.IsPlayingSound(GcSoundTrack.SE)}; clip=Click1.wav");
        }
#if UNITY_ANDROID && !UNITY_EDITOR
        static bool AndroidFlag(string key)
        {
            using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
            using var intent = activity.Call<AndroidJavaObject>("getIntent");
            return intent.Call<bool>("getBooleanExtra", key, false);
        }
#endif
        bool FeatureSmokeRequested()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
            using var intent = activity.Call<AndroidJavaObject>("getIntent");
            return intent.Call<bool>("getBooleanExtra", "gcFeatureSmoke", false);
#elif UNITY_IOS && !UNITY_EDITOR
            return GcProbeShouldRunFeatureSmoke() != 0;
#elif UNITY_WEBGL && !UNITY_EDITOR
            return Application.absoluteURL.Contains("gcFeatureSmoke=1");
#else
            return System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-gcFeatureSmoke") >= 0;
#endif
        }
        IEnumerator FeatureSmoke()
        {
            yield return null;
            Log("feature.start", "network/storage/acceleration; no synthetic input");
            const string key = "__gc_validation_reload_count";
            gc.TryLoad(key, out int stored); gc.Save(key, stored + 1);
            gc.TryLoad(key, out int loaded);
            Log("storage.roundtrip", $"previous={stored}; current={loaded}; equal={loaded == stored + 1}");
            using var text = gc.Network.GetText("https://httpbingo.org/get", timeoutSeconds: 15);
            using var image = gc.Network.GetImage("https://httpbingo.org/image/png", timeoutSeconds: 15);
            using var bad = gc.Network.GetText("https://httpbingo.org/status/404", timeoutSeconds: 15);
            using var cancelled = gc.Network.GetText("https://httpbingo.org/delay/2", timeoutSeconds: 5);
            cancelled.Cancel();
            using var timed = gc.Network.GetText("https://httpbingo.org/delay/2", timeoutSeconds: .25);
            while (!text.IsDone || !image.IsDone || !bad.IsDone || !timed.IsDone) yield return null;
            network = $"GET {text.Status} / image {image.Status}\n404 {bad.ResponseCode} / timeout {timed.Status}";
            Log("network.smoke", $"text={text.Status}/{text.ResponseCode}; image={image.Status}/{image.Width}x{image.Height}; bad={bad.Status}/{bad.ResponseCode}; cancel={cancelled.Status}; timeout={timed.Status}");
            gc.Acceleration.Start();
            int samples = 0; double until = Time.realtimeSinceStartupAsDouble + 3;
            while (Time.realtimeSinceStartupAsDouble < until)
            {
                samples += gc.Acceleration.Events.Count; yield return null;
            }
            Log("acceleration.smoke", $"status={gc.Acceleration.Status}; samples={samples}; x={gc.Acceleration.X:F3}; y={gc.Acceleration.Y:F3}; z={gc.Acceleration.Z:F3}; time={gc.Acceleration.Time:F6}; dt={gc.Acceleration.DeltaTime:F6}");
            gc.Acceleration.Stop();
            Log("acceleration.stop", $"status={gc.Acceleration.Status}; hasValue={gc.Acceleration.HasValue}; events={gc.Acceleration.Events.Count}");
            Log("feature.complete", "done");
        }

        void StopCamera()
        {
            gc.Camera.Stop();
            camera = "停止"; Log("camera.stop", camera);
        }

        public override void PauseGame()
        {
            if (locationRoutine != null) StopCoroutine(locationRoutine);
            if (networkRoutine != null) StopCoroutine(networkRoutine);
            locationBusy = cameraBusy = networkBusy = false;
            gc.Location.Stop(); Input.location.Stop(); location = "中断しました。再度取得してください";
            Log("lifecycle.pause", "stopped");
        }
        public override void ResumeGame() => Log("lifecycle.resume", "操作で再開してください");

        static void Log(string kind, string detail)
            => Debug.Log("GC_PROBE " + JsonUtility.ToJson(new ProbeEvent { kind = kind, detail = detail }));
        [System.Serializable] sealed class ProbeEvent { public string kind = "", detail = ""; }
    }
}
