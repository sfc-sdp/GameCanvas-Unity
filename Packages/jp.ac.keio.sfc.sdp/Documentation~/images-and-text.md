# 画像と日本語

画像と音は `Assets/Res` に置きます。フォルダを分けても構いません。キーは `Assets/Res` からの相対パスで、拡張子を含みます。区切りは `/` です。大小文字は区別します。日本語のファイル名も使えます。

```text
Assets/Res/BlueSky.png        →  BlueSky.png
Assets/Res/背景/青空.png      →  背景/青空.png
```

エディタがカタログを更新します。素材を足すためだけに「リソース定義の強制更新」を押す必要はありません。実行中の差し替えは保証していません。再生を止めてから画像を変え、もう一度再生します。

カタログの読み込みに失敗すると Console に `GC-ASSET-*` が出ます。`Assets/GameCanvas/asset-catalog.json` の `errors` も同じ内容です。エラーがあるあいだは、カタログ全体が無効になります。

## 画像を出す

最初の表示はパスをそのまま渡します。`TryGetImage` は不要です。

```csharp
gc.DrawImage("BlueSky.png", 0, 0);
gc.DrawImage("BlueSky.png", 360, 640, rotation: 30, anchor: GcAnchor.MiddleCenter);
gc.DrawImage("BlueSky.png", gc.Pointer.Position);
gc.DrawImage("BlueSky.png", new GcRect(40, 800, 320, 240), rotation: 15);
```

`rotation` は時計回りの度数です。回転の中心は `anchor` で指定した点です。省略すると左上、回転 0 です。位置を指定した `DrawImage` は `SetRectAnchor` を見ません。`GcPoint` も渡せます。`gc.Pointer.Position` がそのまま使えます。

`GcRect` の `Radian` は弧度法です。パスでもハンドルでも文字でも、矩形指定の `rotation` は既定 0、`anchor` は既定で左上です。`rotation` は矩形の回転へ加える度数で、渡した `GcRect` 自体は変わりません。文字の矩形指定も同じです。

画像が見つからないときは、マゼンタの四角とパスを画面に出します。Console には `GC_ASSET_MISSING` が、同じキーについて一度だけ出ます。警告の記録は 256 件で打ち切ります。

同じ画像を何度も使うときや、有無で処理を変えたいときはハンドルを残します。

```csharp
if (GcAssets.TryGetImage("BlueSky.png", out var sky))
{
    gc.DrawImage(sky, 0, 0);
    gc.DrawImage(sky, gc.Pointer.Position);
    gc.DrawImage(sky, new GcRect(40, 800, 320, 240), rotation: 15);
}
```

パス解決の結果は上限 256 件まで覚えます。テクスチャ自体は複製しません。カタログの更新とサブシステムの登録で、この記憶は消えます。

生成定数 `GcImage.BlueSky` は既存素材向けに残っています。新しく足す素材はパスで書いてください。

## 日本語を置く

既定のフォントで日本語を描けます。基準点は「指定した座標に、文字のどの角を乗せるか」です。複数行の行揃えとは別です。

```csharp
gc.SetColor(0, 0, 0);
gc.SetFontSize(48);
gc.DrawString("左上を基準にする", 40, 160);
gc.DrawString("右上を基準にする", 680, 160, anchor: GcAnchor.UpperRight);
gc.DrawString("30度回す", 360, 400, rotation: 30, anchor: GcAnchor.MiddleCenter);
gc.DrawString("ここ", gc.Pointer.Position);
```

位置を指定した `DrawString` は `SetStringAnchor` を見ません。呼び出しごとに `anchor` を渡し、省略すると左上です。位置を省略した `DrawString(text)` だけが、直前の `SetStringAnchor` を使います。

色は 0 から 255 の整数です。`int` の変数でも同じ単位です。範囲外は端の値へ丸めます。0 から 1 で作りたいときは `GcColor.FromNormalized` を使い、NaN は渡せません。

```csharp
gc.SetColor(0, 0, 0);
gc.SetColor(GcColor.FromNormalized(0.2f, 0.4f, 0.9f));
gc.SetBackgroundColor(255, 255, 255);
```

Unity の `Color` を渡す入口は、Unity 側との連携用に残しています。フォントの大きさは `SetFontSize` です。幅だけ先に知りたいときは `gc.CalcStringWidth(text)` があります。

最初の完成例は `Assets/Game.cs` です。

## 音

音も同じフォルダです。短い効果音なら `PlaySE`、BGM ならトラックを指定します。

```csharp
if (GcAssets.TryGetSound("Click1.wav", out var click))
{
    gc.PlaySE(click);
    gc.PlaySound(click, GcSoundTrack.BGM1, loop: true);
}
gc.StopSound(GcSoundTrack.BGM1);
```

トラックは `BGM1`、`BGM2`、`BGM3`、`SE` です。`PlaySE` は `SE` トラックで 1 回鳴らします。
