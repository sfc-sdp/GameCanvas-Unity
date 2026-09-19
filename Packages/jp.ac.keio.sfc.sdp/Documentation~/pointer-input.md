# ポインターで操作する

タッチ、マウスの左ボタン、ペン先を同じ書き方で扱います。入力は `UpdateGame` で読み、描画は `DrawGame` で行います。

同じ例は `Samples~/Tutorial/PointerOne.cs` と `PointerMany.cs` にあります。`Game.cs` に移すときはクラス名を `Game` に変えます。

## まず1本で動かす

`gc.Pointer` で代表の1本を読みます。次の例は、四角形を押した位置のずれを保ったまま動かします。マウスなら左ボタンでドラッグできます。

```csharp
#nullable enable
using GameCanvas;

public sealed class Game : GameBase
{
    float x = 100, y = 300;
    float offsetX, offsetY, originalX, originalY;
    int? dragId;

    public override void InitGame()
    {
        gc.ChangeCanvasSize(720, 1280);
    }

    public override void UpdateGame()
    {
        var p = gc.Pointer;
        if (p.Down && p.StartX >= x && p.StartX < x + 180 &&
                      p.StartY >= y && p.StartY < y + 180)
        {
            dragId = p.Id;
            offsetX = p.StartX - x;
            offsetY = p.StartY - y;
            originalX = x;
            originalY = y;
        }
        if (dragId != p.Id) return;
        if (p.Held || p.Up)
        {
            x = p.X - offsetX;
            y = p.Y - offsetY;
        }
        if (p.Cancelled)
        {
            x = originalX;
            y = originalY;
        }
        if (p.Up || p.Cancelled) dragId = null;
    }

    public override void DrawGame()
    {
        gc.ClearScreen();
        gc.SetRectAnchor(GcAnchor.UpperLeft);
        gc.SetColor(40, 100, 230);
        gc.FillRect(x, y, 180, 180);
    }
}
```

`Down` はこのフレームに押し始めたこと、`Held` はフレーム末に押していること、`Up` は普通に離したことです。`Cancelled` は、アプリの中断や接続解除などで操作が取り消された状態です。この例では中断すると、四角形を押し始める前の位置へ戻します。

押し始めたフレームでも `Held` は真です。ただし、次の更新までのあいだに押して離すと、`Down` と `Up` が両方真になり、`Held` は偽になります。その場合も開始位置と最後の位置は残るので、短い操作は抜けません。

当たり判定には `StartX` と `StartY` を使います。押下と移動が同じフレームに届くことがあるため、最後の位置 `X`・`Y` で掴むと、指が乗っていない対象を掴むことがあります。離す直前の移動を取りこぼさないように、位置の反映は `Held` だけでなく `Up` でも行います。

`FillRect` は図形なので `SetRectAnchor` を使います。画像も同じです。文字は `SetStringAnchor` です。

代表の指を離しても、`gc.Pointer` はすでに押している別の指へ乗り移りません。次の新しい押下を待ちます。残った指を追い続けるなら、次の一覧を使います。

## 複数の指を順に読む

`gc.Pointers` には、そのフレームのポインターが入っています。まずは `for` で、個数と添字を使って読みます。

```csharp
for (int i = 0; i < gc.Pointers.Count; i++)
{
    var p = gc.Pointers[i];
    // ここで p.Down、p.Held、p.Up、p.Cancelled を調べます。
}
```

`gc.Pointer` と `gc.Pointers[i]` は同じ型です。1本のときに覚えた読み方を、そのまま使えます。

`i` は一覧の何番目かです。指を離すと並びは変わるので、動かしている指の記録には `p.Id` を使います。独立した2本のドラッグでは、四角形ごとに ID、押した位置のずれ、元の位置を保存します。別の指に、操作中の四角形を渡してはいけません。

`Count` は「今押している指の本数」ではありません。離した指はそのフレームまで残り、マウスやペンのホバーは押していなくても一覧に入ります。押している本数が必要なら、`Held` が真の要素を数えます。

次の例は、2つの四角形を2本の指で別々に動かします。手前の四角形を後から描いているので、掴むときは後ろから調べます。

```csharp
#nullable enable
using GameCanvas;

public sealed class Game : GameBase
{
    sealed class Box
    {
        public float X, Y, OffsetX, OffsetY, OriginalX, OriginalY;
        public int? Id;
    }

    readonly Box[] boxes =
    {
        new Box { X = 100, Y = 450 },
        new Box { X = 400, Y = 450 }
    };

    public override void InitGame()
    {
        gc.ChangeCanvasSize(720, 1280);
    }

    public override void UpdateGame()
    {
        for (int i = 0; i < gc.Pointers.Count; i++)
        {
            var p = gc.Pointers[i];
            if (p.Down)
            {
                for (int j = boxes.Length - 1; j >= 0; j--)
                {
                    var b = boxes[j];
                    if (p.StartX < b.X || p.StartX >= b.X + 180 ||
                        p.StartY < b.Y || p.StartY >= b.Y + 180) continue;
                    if (b.Id.HasValue) break;
                    b.Id = p.Id;
                    b.OffsetX = p.StartX - b.X;
                    b.OffsetY = p.StartY - b.Y;
                    b.OriginalX = b.X;
                    b.OriginalY = b.Y;
                    break;
                }
            }

            for (int j = 0; j < boxes.Length; j++)
            {
                var b = boxes[j];
                if (b.Id != p.Id) continue;
                if (p.Held || p.Up)
                {
                    b.X = p.X - b.OffsetX;
                    b.Y = p.Y - b.OffsetY;
                }
                if (p.Cancelled)
                {
                    b.X = b.OriginalX;
                    b.Y = b.OriginalY;
                }
                if (p.Up || p.Cancelled) b.Id = null;
            }
        }
    }

    public override void DrawGame()
    {
        gc.ClearScreen();
        gc.SetRectAnchor(GcAnchor.UpperLeft);
        gc.SetColor(40, 100, 230);
        gc.FillRect(boxes[0].X, boxes[0].Y, 180, 180);
        gc.SetColor(230, 100, 40);
        gc.FillRect(boxes[1].X, boxes[1].Y, 180, 180);
    }
}
```

右ボタン、ホイール、筆圧の入口はまだありません。Android 実機ではポインターに加え、キーの押下と解放を確認しています。iOSシミュレータではタップ、ドラッグ、ホームへ移ったあとの復帰と、そのあとの新しいドラッグ、日本語表示を確認しています。iOSの複数指と、接触中のキャンセルは未確認です。

## foreach で短く書く

添字を使わずに全要素を順に読むなら、次のようにも書けます。

```csharp
foreach (var p in gc.Pointers)
{
    // for の例と同じ処理を書けます。
}
```

`foreach` は、このような繰り返しを短く書く糖衣構文です。ここでは列挙子の仕組みを覚える必要はありません。`for` で処理を理解してから、短い書き方として使ってください。一般のC#では、`foreach` と添字の `for` が常に同じ仕組みになるわけではありません。

この一覧を具体型のまま `for` か `foreach` で読む分には、繰り返しのたびに新しい配列は作りません。`IEnumerable<T>` などのインターフェース型へ入れ替えた場合は、その限りではありません。

## 状態の意味と寿命

| プロパティ | 意味 |
| --- | --- |
| `Id` | 操作中に変わらない番号。OSの指番号ではなく、機器をまたいでも衝突しない |
| `Kind` | `Touch`、`Mouse`、`Stylus` などの種別 |
| `Present` | 位置が有効か。入力がないときは偽。中断時の最後の位置は `Cancelled` でも確認できる |
| `Inside` | 最後の位置がキャンバス内か。外でもドラッグは続けられる |
| `Position` / `X`, `Y` | キャンバス上の最後の位置。右がX、下がYの正方向。`GcPoint` なので `gc.DrawImage("BallRed.png", p.Position)` に渡せる |
| `StartPosition` / `StartX`, `StartY` | 押し始めた位置。ホバー中は操作の開始位置として使わない |
| `Delta` | 前フレーム末からの変位。新しい接触では開始位置からの変位 |
| `Duration` | 押下からの秒数。終了フレームでは終了時点までの秒数。ホバーだけなら0 |

読み取りで状態は消費されません。`UpdateGame` と `DrawGame` では、同じフレームの状態を読めます。一覧の中身は次の更新で変わります。後から使う値は `var saved = gc.Pointers[i];` のようにコピーしてください。

マウスとペンの位置は、機器の登録だけでは原点に作りません。最初の入力通知から公開します。マウスの基本の押下は左ボタン、ペンはペン先です。タッチにホバーはありません。

## 正確な操作順が必要な場合

状態は指ごとにまとまります。同じ対象を一方の指が離した直後に別の指が押す、といった操作順を厳密に扱う場合は `gc.PointerEvents` を使います。

```csharp
for (int i = 0; i < gc.PointerEvents.Count; i++)
{
    var e = gc.PointerEvents[i];
    // e.Id、e.Phase、e.X、e.Y を発生順に処理します。
}
```

段階は `Begin`、`Hold`、`End`、`Cancelled`、`Hover` です。この一覧は入力変化の記録であり、静止している指の `Hold` が毎フレーム入るとは限りません。同じ ID が何度も現れる場合があります。OSが同時にまとめて届けた変化に、実際には分からない前後関係を付け足すことはしません。

通常の独立ドラッグには状態の一覧を使います。同じゲーム操作を状態とイベント列の両方で処理すると二重に動くため、必要な方を選んでください。

## キーボード

キーもポインターと同じ読み方です。同じ例は `Samples~/Tutorial/KeySpace.cs` にあります。

```csharp
#nullable enable
using GameCanvas;

public sealed class Game : GameBase
{
    float y = 600;

    public override void InitGame()
    {
        gc.ChangeCanvasSize(720, 1280);
    }

    public override void UpdateGame()
    {
        var space = gc.Key(GcKey.Space);
        if (space.Down) y -= 80;
        y += 2;
        if (y > 600) y = 600;
        if (space.Cancelled) y = 600;
    }

    public override void DrawGame()
    {
        gc.ClearScreen();
        gc.SetRectAnchor(GcAnchor.UpperLeft);
        gc.SetColor(40, 100, 230);
        gc.FillRect(270, y, 180, 180);
        gc.SetColor(0, 0, 0);
        gc.SetFontSize(32);
        var space = gc.Key(GcKey.Space);
        gc.DrawString($"Space Held={space.Held} / {space.Duration:0.00}s", 40, 40);
    }
}
```

`Down` / `Held` / `Up` / `Cancelled` / `Duration` の意味はポインターと同じです。`GcKey` はキーの物理的な位置で、文字入力や配列の変換ではありません。画面上のキーボードは `gc.ShowScreenKeyboard()` です。
