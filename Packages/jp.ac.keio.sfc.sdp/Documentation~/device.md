# 端末の機能

権限が要る機能は、利用者が操作したタイミングで開始します。描画のついでに勝手に始めません。Editor の再生では使えないものがあります。

## 位置情報

`gc.Location` が入口です。Android と iOS の実機向けです。Editor と Web では `IsSupported` が偽で、`Start` すると `Unsupported` になります。同じ例は `Samples~/Tutorial/LocationTap.cs` にあります。地図付きの例は `Samples~/Geolocation/GeolocationSample.cs` です。

```csharp
#nullable enable
using GameCanvas;

public sealed class Game : GameBase
{
    string message = "画面を押すと位置情報を取ります";

    public override void InitGame()
    {
        gc.ChangeCanvasSize(720, 1280);
        gc.SetFontSize(36);
    }

    public override void UpdateGame()
    {
        if (gc.Pointer.Down)
        {
            var s = gc.Location.Status;
            if (s == GcLocationState.Idle || s == GcLocationState.Stopped ||
                s == GcLocationState.Failed || s == GcLocationState.TimedOut ||
                s == GcLocationState.NotGranted || s == GcLocationState.Disabled ||
                s == GcLocationState.Unsupported)
            {
                gc.Location.Start();
            }
        }

        if (gc.Location.TryGetSample(out var sample))
        {
            message = $"緯度 {sample.Latitude:F5}\n経度 {sample.Longitude:F5}";
            if (sample.IsMock) message += "\n模擬の位置です";
        }
        else
        {
            message = ShowState(gc.Location.Status);
        }
    }

    public override void DrawGame()
    {
        gc.ClearScreen();
        gc.SetColor(0, 0, 0);
        gc.DrawString(message, 40, 80);
    }

    static string ShowState(GcLocationState state) => state switch
    {
        GcLocationState.RequestingPermission => "位置情報の許可を確認中です",
        GcLocationState.Waiting => "位置情報を取得中です",
        GcLocationState.NotGranted => "位置情報が許可されていません",
        GcLocationState.Disabled => "端末の位置情報がオフです",
        GcLocationState.TimedOut => "時間内に位置情報を取得できませんでした",
        GcLocationState.Unsupported => "この環境では位置情報を使えません",
        GcLocationState.Failed => "位置情報の取得に失敗しました",
        GcLocationState.Stopped => "停止中です。画面を押すと再開できます",
        GcLocationState.Idle => "画面を押してください",
        _ => "位置情報を確認中です"
    };
}
```

`Start` はボタンやタップから一度呼びます。毎フレーム呼ばないでください。権限待ちは 30 秒、最初の測位待ちは `Start` の引数（初期値 30 秒）で打ち切ります。

`TryGetSample` が真のときは、その標本を使います。偽のときは `Status` を見て理由を出します。緯度経度は `double` の度です。値が 0 でも、それだけで失敗とは限りません。Editor 用の固定座標を、測位できたかのように出さないでください。シミュレータやソフトウェアが作った位置は `IsMock` が真です。

標本の精度はメートル、時刻は UTC の UNIX 秒です。端末が概算だけを許可した場合、`Permission` は `Approximate` です。

アプリが背面に回ると、枠組み側が `Stop` します。標本は消えます。復帰しても自動では再開しません。もう一度 `Start` してください。

`TryGetSample` の成功は、新しい高精度の測位を約束しません。屋内で取れないことと、権限を拒まれたことは別です。詳しい診断には `Permission` と `ErrorCode` があります。

## 保存

端末に残る値は `Save` と `TryLoad` です。キーは文字列、値は `int`、`float`、`string` です。`null` を渡すと、そのキーを消します。

```csharp
gc.Save("score", 12);
if (gc.TryLoad("score", out int score))
{
    gc.DrawString($"得点 {score}", 40, 80);
}
```

## 加速度

加速度計がある端末では、対応を確認してから有効にします。無い環境で有効にすると警告が出ます。ボールが傾きに応じて動く例は `Samples~/Acceleration/AccelerationSample.cs` です。

```csharp
if (gc.IsAccelerometerSupported)
{
    gc.IsAccelerometerEnabled = true;
    var a = gc.LastAccelerationEvent.Acceleration;
}
```

`Acceleration` はキャンバスで使いやすいよう、Y と Z を反転した値です。生の値は `RawAcceleration` です。そのフレームに新しい値が来たかは `DidUpdateAccelerationThisFrame` で分かります。複数の標本を順に読むなら `gc.AccelerationEvents` があります。

## カメラ

カメラも、画面を押してから権限を求め、再生してから描きます。起動時には要求しません。同じ内容は `Samples~/DeviceCamera/DeviceCameraSample.cs` です。`Game.cs` に移すときはクラス名を `Game` に変えます。

```csharp
#nullable enable
using GameCanvas;

public sealed class Game : GameBase
{
    GcCameraDevice? camera;
    bool asking;
    bool playing;
    int requestId;
    string message = "画面を押すとカメラを開始します";

    public override void InitGame()
    {
        gc.ChangeCanvasSize(720, 1280);
        gc.SetFontSize(36);
    }

    public override void UpdateGame()
    {
        if (asking || playing || !gc.Pointer.Down) return;
        asking = true;
        var id = ++requestId;
        if (gc.HasUserAuthorizedPermissionCamera)
        {
            Play(id);
            return;
        }
        gc.RequestUserAuthorizedPermissionCameraAsync(ok =>
        {
            if (id != requestId) return;
            if (ok) Play(id);
            else
            {
                asking = false;
                message = "カメラが許可されていません。画面を押すと再試行できます";
            }
        });
    }

    void Play(int id)
    {
        if (id != requestId) return;
        asking = false;
        if (!gc.TryGetCameraImage(out var device))
        {
            camera = null;
            playing = false;
            message = "カメラがありません";
            return;
        }
        camera = device;
        if (!gc.PlayCameraImage(device, out var size))
        {
            gc.StopCameraImage(device);
            camera = null;
            playing = false;
            message = "カメラを開始できませんでした。画面を押すと再試行できます";
            return;
        }
        gc.ChangeCanvasSize(size.x, size.y);
        playing = true;
        message = $"{device.DeviceName}\n({size.x}x{size.y})";
        if (gc.IsFlippedCameraImage(device)) message += "\nFlipped";
        if (gc.TryGetCameraImageRotation(device, out var rotation) && rotation != 0f) message += $"\nrotation {rotation}";
    }

    public override void DrawGame()
    {
        gc.ClearScreen();
        if (playing && camera != null) gc.DrawCameraImage(camera);
        if (playing)
        {
            gc.SetColor(gc.ColorBlack);
            gc.DrawString(message, 12, 18);
            gc.SetColor(gc.ColorWhite);
            gc.DrawString(message, 10, 15);
        }
        else
        {
            gc.SetColor(gc.ColorBlack);
            gc.DrawString(message, 10, 15);
        }
    }

    public override void PauseGame()
    {
        requestId++;
        if (camera != null) gc.StopCameraImage(camera);
        playing = false;
        asking = false;
        message = "中断しました。画面を押すと再開できます";
    }
}
```

`DrawCameraImage` の `autoPlay` は既定で偽です。先に `PlayCameraImage` が成功してから描きます。基準点は `SetRectAnchor` です。戻り値が偽なら開始できていません。アプリが背面に回ると `PauseGame` で止め、そのときに進めていた権限の要求も無効にします。あとに届いた応答では再生しません。許可の画面で中断した場合も、復帰後にもう一度画面を押して開始します。描画のついでに再生したい従来の書き方だけ、`autoPlay: true` を明示します。iOSシミュレータに実カメラは無いので、映像の確認は実機です。

## 通信

ネット上の画像は、URL を渡して描きます。準備中、成功、失敗は戻り値で分かります。通信の例は `Samples~/Networking/NetworkingSample.cs` です。

```csharp
var url = "https://example.com/image.png";
var state = gc.DrawOnlineImage(url, 0, 0);
if (state == GcAvailability.NotReady) gc.DrawString("読み込み中", 40, 40);
if (state == GcAvailability.NotAvailable) gc.DrawString("取得できません", 40, 40);
```

`url` は実際の画像アドレスに差し替えてください。`DrawOnlineImage` も `SetRectAnchor` を見ます。音声やテキストは `TryGetOnlineText`、`TryGetOnlineSound` です。キャッシュを消すときは `ClearDownloadCache(url)` です。
