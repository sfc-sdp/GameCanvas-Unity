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

`gc.Camera` が入口です。画面を押してから開始します。起動時には要求しません。コールバックや許可待ちの変数は、この例では不要です。同じ内容は `Samples~/DeviceCamera/DeviceCameraSample.cs` です。`Game.cs` に移すときはクラス名を `Game` に変えます。

```csharp
#nullable enable
using GameCanvas;

public sealed class Game : GameBase
{
    string message = "画面を押すとカメラを開始します";

    public override void InitGame()
    {
        gc.ChangeCanvasSize(720, 1280);
        gc.SetFontSize(36);
    }

    public override void UpdateGame()
    {
        if (gc.Pointer.Down)
        {
            var state = gc.Camera.Status;
            if (state == GcCameraState.Running ||
                state == GcCameraState.RequestingPermission ||
                state == GcCameraState.Waiting)
            {
                gc.Camera.Stop();
            }
            else
            {
                gc.Camera.Start();
            }
        }

        if (gc.Camera.Status == GcCameraState.Running)
        {
            message = $"{gc.Camera.Width}x{gc.Camera.Height}";
            var device = gc.Camera.Device;
            if (device != null) message = $"{device.DeviceName}\n{message}";
            message += "\n画面を押すと停止します";
        }
        else
        {
            message = ShowState(gc.Camera.Status);
        }
    }

    public override void DrawGame()
    {
        gc.ClearScreen();
        if (gc.Camera.Status == GcCameraState.Running)
        {
            gc.SetColor(255, 255, 255);
            var scale = 680f / gc.Camera.Width;
            if (gc.Camera.Height * scale > 980f)
            {
                scale = 980f / gc.Camera.Height;
            }
            gc.DrawCamera(20, 240, gc.Camera.Width * scale, gc.Camera.Height * scale);
        }
        gc.SetColor(0, 0, 0);
        gc.DrawString(message, 40, 80);
    }

    static string ShowState(GcCameraState state) => state switch
    {
        GcCameraState.RequestingPermission => "カメラの許可を確認中です\n画面を押すと取り消します",
        GcCameraState.Waiting => "カメラ映像を待っています\n画面を押すと取り消します",
        GcCameraState.NotGranted => "カメラが許可されていません\n画面を押すと再試行できます",
        GcCameraState.NoDevice => "カメラがありません",
        GcCameraState.TimedOut => "時間内にカメラを開始できませんでした\n画面を押すと再試行できます",
        GcCameraState.Unsupported => "この環境ではカメラを使えません",
        GcCameraState.Failed => "カメラの開始に失敗しました\n画面を押すと再試行できます",
        GcCameraState.Stopped => "停止中です。画面を押すと再開できます",
        GcCameraState.Idle => "画面を押すとカメラを開始します",
        _ => "カメラを確認中です"
    };
}
```

`Start` はタップやボタンから一度呼びます。毎フレーム呼ばないでください。呼び直すと、進めていた処理を止めて始め直します。待ち時間は許可と最初の映像を合わせた秒数で、初期値は 30 秒です。幅・高さ・fps は希望値です。実際の大きさは映像が届いてから `Width` と `Height` に入ります。

既定の `Start()` は `GcCameraFacing.Any` です。背面を優先し、無ければ最初のカメラを使います。`Front` と `Rear` は指定した側だけです。無いときは `NoDevice` になります。引数を書くなら `gc.Camera.Start(GcCameraFacing.Front, width: 640, height: 480, fps: 30, timeoutSeconds: 30)` です。

画面を押すと、`Running` か `RequestingPermission` か `Waiting` なら `Stop`、それ以外なら `Start` です。待ちの途中でも取り消せます。`Stop` は許可待ちも取り消して、映像とカメラを解放します。OS の許可画面そのものは閉じられません。止めたあとで遅れて届いた許可では始まりません。アプリが背面に回るか無効になると、GameCanvas が `Stop` します。復帰しても自動では再開しません。許可の画面で中断した場合も `Stopped` のままなので、戻ってからもう一度画面を押して `Start` してください。`PauseGame` で止める必要はありません。

`Start` しただけでは足りません。最初の有効な映像を受け取ると `Status` が `Running` になります。許可は `Unknown` / `NotGranted` / `Granted` です。詳しい診断は `ErrorCode` です。`Width` と `Height` は向きを補正した整数で、`Running` 以外は 0 です。そのフレームに新しい映像が届いたかは `Updated` です。補正の角度は `Rotation`、元映像の上下反転は `IsMirrored` です。`DrawCamera` はどちらも自動で直します。

`gc.DrawCamera()` は原点に実寸で描きます。位置、幅と高さ、`GcRect` も渡せます。`GcPoint` と `Unity.Mathematics.float2` の位置もあります。基準点は `SetRectAnchor` です。描画は開始しません。`Running` でなければ何も描きません。指定した幅と高さの比が映像と違うと伸びます。キャンバスに収めるときは、`Width` と `Height` から同じ比の描画サイズを計算します。カメラは回転するので、映像の前に一度だけ `ChangeCanvasSize` で合わせないでください。`DrawCamera(GcRect)` は `rect.Rotation` を使います。追加の回転は数値や点の引数だけです。幅と高さの指定は描く先の大きさで、実寸の描画はこの限りではありません。上の例は、文字を (40, 80) に出し、映像を (20, 240) からの 680x980 に収めています。`ClearScreen` は色を戻さないので、映像の前に `SetColor(255, 255, 255)` を呼びます。`Running` のとき、幅 720 に比を合わせて描く例です。

```csharp
if (gc.Camera.Status == GcCameraState.Running)
{
    gc.DrawCamera(0, 0, 720, 720f * gc.Camera.Height / gc.Camera.Width);
}
```

許可のあと `Devices` に一覧が入ります。`Count` と添字で読みます。使用中のカメラは `Device` です。選んだカメラを `Start(device)` すると、そのときもう一度確認し、外されていたら `NoDevice` です。同時に複数の映像を流す入口と、デプスカメラの入口はありません。デプスカメラは一覧に出ません。`GcCameraDevice` の欄は以前どおり `DeviceName`、`IsFront`、`CanFocusPoint`、`Resolutions`、`IsDepth` です。ピントは左下を (0, 0)、右上を (1, 1) として `Focus` します。非対応や起動前は偽を返します。解除は `ResetFocus` です。`IsSupported` は実装した環境かどうかで、許可やカメラの有無ではありません。

iOS シミュレータに実カメラは無く、機器が無い状態の確認だけです。実機の iOS カメラは未確認です。Editor の実カメラは確認していません。Web のカメラとブラウザ、Windows のカメラ、macOS のカメラは未確認です。

## 通信

ネット上の画像は、URL を渡して描きます。準備中、成功、失敗は戻り値で分かります。通信の例は `Samples~/Networking/NetworkingSample.cs` です。

```csharp
var url = "https://example.com/image.png";
var state = gc.DrawOnlineImage(url, 0, 0);
if (state == GcAvailability.NotReady) gc.DrawString("読み込み中", 40, 40);
if (state == GcAvailability.NotAvailable) gc.DrawString("取得できません", 40, 40);
```

`url` は実際の画像アドレスに差し替えてください。`DrawOnlineImage` も `SetRectAnchor` を見ます。音声やテキストは `TryGetOnlineText`、`TryGetOnlineSound` です。キャッシュを消すときは `ClearDownloadCache(url)` です。
