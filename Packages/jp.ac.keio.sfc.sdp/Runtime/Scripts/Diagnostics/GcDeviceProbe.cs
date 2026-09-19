#nullable enable
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace GameCanvas.Diagnostics
{
    /// <summary>実機の初期検証用。授業用APIの完成を示すサンプルではありません。</summary>
    public sealed class GcDeviceProbe : GameBase
    {
        public Font UiFont = null!;
        string location = "未要求", camera = "未要求", network = "未実行";
        bool locationBusy, cameraBusy, networkBusy, simulatorSmoke;
        WebCamTexture? cameraTexture;
        GUIStyle? label, button;
        GcImage sky;
        Coroutine? locationRoutine, cameraRoutine, networkRoutine;
        bool pointerMode;
        readonly GcPointerDragDemo pointerDemo = new();
        int cameraFrames;
        double cameraDeadline;

        public override void InitGame()
        {
            gc.ChangeCanvasSize(720, 1280);
            Log("asset.image", GcAssets.TryGetImage("BlueSky.png", out sky) ? "catalog-loaded" : "missing");
            Log("startup", $"Unity={Application.unityVersion}; OS={SystemInfo.operatingSystem}; model={SystemInfo.deviceModel}");
#if UNITY_IOS && !UNITY_EDITOR
            simulatorSmoke = GcProbeShouldRunSimulatorSmoke() != 0;
            if (simulatorSmoke) StartCoroutine(SimulatorSmoke());
#endif
        }

        public override void DrawGame()
        {
            gc.ClearScreen();
            gc.DrawImage("BlueSky.png", 0, 0);
            gc.SetColor(0, 0, 0);
            gc.SetFontSize(36);
            gc.SetStringAnchor(GcAnchor.UpperLeft);
            gc.DrawString("画像と日本語の表示確認", 28, 60);
            gc.DrawString("青空・漢字・ひらがな・カタカナ", 28, 115);
            gc.DrawString($"時刻 {gc.TimeSinceStartup:F1} 秒", 28, 165);
            if (pointerMode) pointerDemo.Draw(gc);
        }

        public override void UpdateGame()
        {
            if (pointerMode) pointerDemo.Update(gc);
            var key = gc.Key(GcKey.Space);
            if (key.Down || key.Up || key.Cancelled)
                Log("key.space", $"down={key.Down}; held={key.Held}; up={key.Up}; cancelled={key.Cancelled}; duration={key.Duration:F3}");
            if (cameraTexture != null && cameraTexture.didUpdateThisFrame && cameraTexture.width > 16)
            {
                if (cameraFrames++ == 0) Log("camera.frame", $"{cameraTexture.width}x{cameraTexture.height}; rotation={cameraTexture.videoRotationAngle}; mirrored={cameraTexture.videoVerticallyMirrored}");
                camera = $"映像を取得 {cameraTexture.width}x{cameraTexture.height} / {cameraFrames}";
            }
            if (cameraTexture != null && cameraFrames == 0 && Time.realtimeSinceStartupAsDouble > cameraDeadline)
            {
                StopCamera();
                camera = "映像待ちが30秒を超えました";
                Log("camera.timeout", camera);
            }
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
            GUI.Box(new Rect(12, 260, 696, 1008), "");
            if (GUI.Button(new Rect(28, 280, 320, 65), "位置情報を取得", button) && !locationBusy && !cameraBusy)
                locationRoutine = StartCoroutine(Location());
            if (GUI.Button(new Rect(370, 280, 320, 65), "位置情報を停止", button))
            {
                if (locationRoutine != null) StopCoroutine(locationRoutine);
                locationBusy = false; gc.Location.Stop(); Input.location.Stop(); location = "停止"; Log("location.stop", location);
            }
            GUI.Label(new Rect(28, 355, 655, 115), location, label);
            if (GUI.Button(new Rect(28, 480, 320, 65), "カメラを開始", button) && !cameraBusy)
                cameraRoutine = StartCoroutine(Camera());
            if (GUI.Button(new Rect(370, 480, 320, 65), "カメラを停止", button)) StopCamera();
            GUI.Label(new Rect(28, 555, 655, 80), camera, label);
            if (cameraTexture != null && cameraFrames > 0)
                GUI.DrawTexture(new Rect(28, 640, 320, 240), cameraTexture, ScaleMode.ScaleToFit);
            if (GUI.Button(new Rect(28, 910, 655, 65), "HTTPS通信を確認", button) && !networkBusy)
                networkRoutine = StartCoroutine(Network());
            GUI.Label(new Rect(28, 985, 655, 100), network, label);
            GUI.Label(new Rect(28, 1110, 655, 140), "権限は各ボタンから要求します。\n映像の向きと日本語は目で確認してください。\n検証用アプリ / Unity " + Application.unityVersion, label);
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

        IEnumerator Camera()
        {
            cameraBusy = true;
            camera = "権限を確認中";
#if UNITY_ANDROID && !UNITY_EDITOR
            yield return Engine.GcAndroidPermission.Request(new[] { UnityEngine.Android.Permission.Camera });
            var granted = UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera);
#else
            var request = Application.RequestUserAuthorization(UserAuthorization.WebCam);
            var deadline = Time.realtimeSinceStartupAsDouble + 30;
            while (!request.isDone && Time.realtimeSinceStartupAsDouble < deadline) yield return null;
            var granted = request.isDone && Application.HasUserAuthorization(UserAuthorization.WebCam);
#endif
            if (!granted) { camera = "カメラが未許可です"; Log("camera.permission", camera); cameraBusy = false; yield break; }
            var devices = WebCamTexture.devices;
            if (devices.Length == 0) { camera = "カメラがありません"; cameraBusy = false; yield break; }
            StopCamera();
            cameraTexture = new WebCamTexture(devices[0].name, 640, 480, 30);
            cameraFrames = 0; cameraDeadline = Time.realtimeSinceStartupAsDouble + 30;
            cameraTexture.Play(); camera = "映像待ち"; cameraBusy = false;
        }

        IEnumerator Network()
        {
            networkBusy = true; network = "通信中";
            using var request = UnityWebRequest.Get("https://example.com/");
            request.timeout = 15;
            yield return request.SendWebRequest();
            network = $"{request.result} / HTTP {request.responseCode}\n{request.error ?? "HTTPSで受信しました"}";
            Log("network.result", network); networkBusy = false;
        }

        void StopCamera()
        {
            if (cameraTexture != null) { cameraTexture.Stop(); Destroy(cameraTexture); cameraTexture = null; }
            camera = "停止"; Log("camera.stop", camera);
        }

        public override void PauseGame()
        {
            if (locationRoutine != null) StopCoroutine(locationRoutine);
            if (cameraRoutine != null) StopCoroutine(cameraRoutine);
            if (networkRoutine != null) StopCoroutine(networkRoutine);
            locationBusy = cameraBusy = networkBusy = false;
            gc.Location.Stop(); Input.location.Stop(); location = "中断しました。再度取得してください";
            StopCamera(); Log("lifecycle.pause", "stopped");
        }
        public override void ResumeGame() => Log("lifecycle.resume", "操作で再開してください");

        static void Log(string kind, string detail)
            => Debug.Log("GC_PROBE " + JsonUtility.ToJson(new ProbeEvent { kind = kind, detail = detail }));
        [System.Serializable] sealed class ProbeEvent { public string kind = "", detail = ""; }
    }
}
