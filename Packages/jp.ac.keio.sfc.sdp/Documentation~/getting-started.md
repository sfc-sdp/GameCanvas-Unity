# はじめに

編集するファイルは `Assets/Game.cs` です。Unity の入れ方は [インストール](installation.md) を見てください。

## 画面の流れ

`Game` は `GameBase` を継承します。毎フレーム、次の順で呼ばれます。

- `InitGame` — 起動時と、無効化してから再有効化したとき。キャンバスの大きさや、フィールドの初期化
- `UpdateGame` — 位置や入力など、状態を進める
- `DrawGame` — そのフレームの絵を描く

入力は `UpdateGame` で読み、描画は `DrawGame` で行います。同じフレームなら、どちらから読んでも入力の中身は同じです。`Game` のフィールドは無効化しても残ります。内部の `gc` は作り直されるので、通信の操作などは `InitGame` で捨てて、変数を戻してください。

最初から入っている `Assets/Game.cs` は次の形です。

```csharp
#nullable enable
using GameCanvas;

public sealed class Game : GameBase
{
    int sec = 0;

    public override void InitGame()
    {
        gc.ChangeCanvasSize(720, 1280);
    }

    public override void UpdateGame()
    {
        sec = (int)gc.TimeSinceStartup;
    }

    public override void DrawGame()
    {
        gc.ClearScreen();
        gc.DrawImage("BlueSky.png", 0, 0);
        gc.SetColor(0, 0, 0);
        gc.SetFontSize(48);
        gc.DrawString("この文字と青空の画像が", 40, 160);
        gc.DrawString("見えていれば成功です", 40, 270);
        gc.SetStringAnchor(GcAnchor.UpperRight);
        gc.DrawString($"{sec}s", 630, 10);
        gc.SetStringAnchor(GcAnchor.UpperLeft);
    }
}
```

`gc` が GameCanvas の入口です。キャンバスの初期サイズは 720×1280 です。端末の画面と縦横比が違うと、上下か左右に帯が付きます。

画像は `Assets/Res` からの相対パスで描きます。最初の表示に `TryGetImage` は不要です。秒数は右上を基準にしているので、描いたあと左上へ戻しています。基準点は次の変更まで残ります。

`gc.TimeSinceStartup` は起動からの秒数で、型は `double` です。Unity の単調時計なので、端末の日時を変えても増え続けます。入力の `Duration` や `PointerEvents` / `KeyEvents` の `Time` も同じです。位置を前フレームからの経過で進めるときは `gc.TimeSincePrevFrame` を使います。こちらは `float` の秒です。最初のフレームと、アプリが背面から戻った直後は 0 です。長い経過どうしの差を `float` で取らないでください。壁時計の UNIX 秒は `gc.CurrentTimestamp` です。UTC です。

フレームの間隔は Unity に任せます。`Thread.Sleep` や待ちループでフレームを作らないでください。希望する速さは `gc.SetFrameRate(60)` です。正の整数だけを渡せます。秒で書くなら `gc.SetFrameInterval(1.0 / 30)` です。`1.0 / int.MaxValue` 以上 1 以下の有限の秒で、最も近い整数 fps に丸めます。実際の fps は約束しません。デスクトップで垂直同期が有効なときは画面の更新を優先します。モバイルでは fps の希望値です。

## 座標

原点は左上です。右が X の正方向、下が Y の正方向です。単位はキャンバスの画素です。描画と座標の回転の引数名は `rotation` です。基準点を中心に時計回りで、単位は度です。カメラ映像の補正角度は `gc.Camera.Rotation` で、同じ単位です。`gc.Sin` や `gc.Cos` の `degree` は角度そのものを指すので、名前はそのままです。

色は 0 から 255 の整数です。範囲外は端の値へ丸めます。

```csharp
gc.SetColor(40, 100, 230);
gc.SetRectAnchor(GcAnchor.UpperLeft);
gc.FillRect(80, 400, 200, 120);
```

`SetRectAnchor` は図形と画像の基準点です。文字は `SetStringAnchor` です。引数なし、`GcPoint`、数値の x,y、`GcRect` のどれでも、直前の設定を使います。毎フレームの最初で戻す必要はありません。

## 乱数

`gc.Random()` は 0 以上 1 未満の `float` です。

整数が欲しいときは、上限を含まない範囲を渡します。サイコロなら `gc.Random(1, 7)` で 1 から 6 です。配列の添字なら `gc.Random(array.Length)` で 0 から `Length - 1` です。`gc.Random(6)` は 0 以上 6 未満です。

小数も上限は含みません。`gc.Random(0.5f, 1.5f)` は 0.5 以上 1.5 未満です。

空の範囲や、下限が上限以上の範囲は `ArgumentOutOfRangeException` です。小数の NaN と Infinity も例外です。上下限を黙って入れ替えません。

同じ種から同じ列を出したいときは `gc.SetRandomSeed` です。以前の版と同じ乱数列になることは約束しません。

## 次に進む

画像の足し方は [画像と日本語](images-and-text.md) です。指やマウスは [ポインターで操作する](pointer-input.md) です。
