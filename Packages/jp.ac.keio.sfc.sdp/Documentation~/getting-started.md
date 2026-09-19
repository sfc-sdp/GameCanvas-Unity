# はじめに

編集するファイルは `Assets/Game.cs` です。Unity の入れ方は [インストール](installation.md) を見てください。

## 画面の流れ

`Game` は `GameBase` を継承します。毎フレーム、次の順で呼ばれます。

- `InitGame` — 起動時に一度だけ。キャンバスの大きさなど
- `UpdateGame` — 位置や入力など、状態を進める
- `DrawGame` — そのフレームの絵を描く

入力は `UpdateGame` で読み、描画は `DrawGame` で行います。同じフレームなら、どちらから読んでも入力の中身は同じです。

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

## 座標

原点は左上です。右が X の正方向、下が Y の正方向です。単位はキャンバスの画素です。画像と文字の回転は、指定した基準点を中心に時計回りで、単位は度です。

色は 0 から 255 の整数です。範囲外は端の値へ丸めます。

```csharp
gc.SetColor(40, 100, 230);
gc.SetRectAnchor(GcAnchor.UpperLeft);
gc.FillRect(80, 400, 200, 120);
```

`SetRectAnchor` は図形と画像の基準点です。文字は `SetStringAnchor` です。引数なし、`GcPoint`、数値の x,y、`GcRect` のどれでも、直前の設定を使います。毎フレームの最初で戻す必要はありません。

## 次に進む

画像の足し方は [画像と日本語](images-and-text.md) です。指やマウスは [ポインターで操作する](pointer-input.md) です。
