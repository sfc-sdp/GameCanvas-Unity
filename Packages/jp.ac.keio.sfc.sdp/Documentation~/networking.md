# 通信する

ネットから画像・音・テキストを取り、テキストを送ります。入口は `gc.Network` です。呼び出すたびに新しい通信が始まります。返ってきた操作を変数に残し、毎フレーム同じ呼び出しをしないでください。

次の完全例は画像だけです。画像と音を両方取る例は `Samples~/Networking/NetworkingSample.cs` です。地図タイルは `Samples~/Geolocation/GeolocationSample.cs` です。`Game.cs` に移すときはクラス名を `Game` に変えます。

## 画像を取る

```csharp
#nullable enable
using GameCanvas;

public sealed class Game : GameBase
{
    const string url = "https://images.unsplash.com/photo-1512692723619-8b3e68365c9c?fit=crop&w=720&q=80";
    GcImageRequest? image;

    public override void InitGame()
    {
        gc.ChangeCanvasSize(720, 1280);
        gc.SetFontSize(32);
        image?.Dispose();
        image = null;
    }

    public override void UpdateGame()
    {
        if (image == null) image = gc.Network.GetImage(url);

        if (gc.Pointer.Down && image != null &&
            (image.Status == GcRequestState.Failed ||
             image.Status == GcRequestState.Cancelled ||
             image.Status == GcRequestState.TimedOut))
        {
            image.Dispose();
            image = gc.Network.GetImage(url);
        }
    }

    public override void DrawGame()
    {
        gc.ClearScreen();
        if (image != null && image.Status == GcRequestState.Succeeded)
        {
            gc.SetColor(255, 255, 255);
            gc.DrawImage(image, 0, 0);
        }
        gc.SetColor(0, 0, 0);
        if (image == null || image.Status == GcRequestState.Pending) gc.DrawString("読み込み中です", 12, 12);
        else if (image.Status != GcRequestState.Succeeded) gc.DrawString("取得できません。画面を押すとやり直します", 12, 12);
    }
}
```

`GetImage` は通信を開始して `GcImageRequest` を返します。できた画像は `gc.DrawImage(image, x, y)` です。幅と高さを渡すと伸縮します。回転も渡せます。描画は通信を始めません。成功する前は何も描きません。基準点は `SetRectAnchor` です。`ClearScreen` は色を戻さないので、画像の前に白、文字の前に黒へ戻します。

大きさは `image.Width` と `image.Height` です。成功前と破棄後は 0 です。

タイムアウトは秒です。省略すると 30 秒です。`gc.Network.GetImage(url, 30)` のように書きます。

無効化して再有効化すると、新しい `gc` になります。`InitGame` で古い操作を `Dispose` し、変数を `null` へ戻します。

## 状態を見る

| 状態 | 意味 |
| --- | --- |
| `Pending` | 通信の途中 |
| `Succeeded` | 成功。データは `Dispose` まで残る |
| `Failed` | 失敗 |
| `Cancelled` | 取り消した。アプリが背面に回ったときも、待ち中はこちらになる |
| `TimedOut` | 時間切れ |
| `Disposed` | 破棄した |

`Status` が今の状態です。`IsDone` は待ち中以外で真です。`ResponseCode` は HTTP の応答番号です。`ErrorCode` は失敗の分類で、応答本文や URL の秘密は入りません。待ち中なら `Cancel()` で止められます。サーバ側の処理まで止まったとは限りません。

成功したデータは、自分で `Dispose` するまで残ります。`gc` が破棄されると、残っていた操作も解放されます。以前のダウンロードキャッシュ入口はありません。

## 音を取る

形式を指定します。`Wav`、`Mp3`、`Ogg` です。

```csharp
if (sound == null) sound = gc.Network.GetSound(url, GcSoundFormat.Mp3);
if (sound.Status == GcRequestState.Succeeded && gc.Pointer.Down)
{
    gc.PlaySound(sound, GcSoundTrack.BGM1, loop: true);
}
```

`PlaySound` は成功前と破棄後は偽を返します。長さは `sound.Duration` で、単位は秒です。成功前と破棄後は 0 です。再生は画面を押すなど、利用者の操作のあとにします。通信の完了だけでは始めません。

失敗、取消、時間切れのあとに取り直すときは、画像と同じく古い操作を `Dispose` してから新しい `GetSound` を呼びます。毎フレーム呼び出さないでください。

## テキストを取る

```csharp
if (textRequest == null) textRequest = gc.Network.GetText(url);
if (textRequest.Status == GcRequestState.Succeeded)
{
    gc.DrawString(textRequest.Text, 12, 12);
}
```

成功した本文は `Text` です。取得前と破棄後は空文字です。

## サーバへ送る

送り先は自分で用意した HTTPS の URL に差し替えます。次の URL はそのまま使わないでください。実データも、教材の起動確認から勝手に送らないでください。

```csharp
var post = gc.Network.PostText("https://example.com/score", "12");
```

JSON なら内容の種類を変えます。

```csharp
var post = gc.Network.PostText(
    "https://example.com/score",
    "{\"score\":12}",
    "application/json");
```

フォームはキーと値の組です。`using System.Collections.Generic;` が要ります。

```csharp
var fields = new Dictionary<string, string>
{
    { "name", "player" },
    { "score", "12" }
};
var post = gc.Network.PostForm("https://example.com/score", fields);
```

どちらも戻りは `GcTextRequest` です。応答本文は成功後の `Text` です。失敗しても自動では再試行しません。毎フレーム `PostText` や `PostForm` を呼び出して送り直さないでください。やり直すなら、次の節のとおり新しい操作を始めます。

## やり直す・やめる

同じ操作をもう一度始める入口はありません。先に古い操作を `Dispose` してから、新しい呼び出しをします。音を再生中なら、先に止めてから破棄します。

```csharp
gc.StopSound(GcSoundTrack.BGM1);
sound.Dispose();
sound = gc.Network.GetSound(url, GcSoundFormat.Mp3);
```

アプリが背面に回ると、待ち中の通信はすべて取り消されます。成功済みのデータは残ります。サーバ側が止まったとは限りません。`Cancelled`、`Failed`、`TimedOut` の GET は、画面を押したときだけ新しい操作を始めてください。毎フレームの再試行と、POST の自動再試行はしません。

## HTTPS と Web

URL は `https://` から書いてください。`http://` も受け付けますが、教材では HTTPS を使います。

Web では次を分けて考えます。

- 接続先の CORS 許可。無ければ通信できません
- ブラウザの安全なコンテキスト。HTTPS や localhost などです
- 音の再生。通信の成功とは別に、利用者の操作のあとで始めます

素材の URL は公開されている外部サーバです。応答は接続先と CORS の許可に依存します。
