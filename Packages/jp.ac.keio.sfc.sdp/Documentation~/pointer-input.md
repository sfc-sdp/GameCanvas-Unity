# ポインターで操作する

タッチ、マウスの左ボタン、ペン先を同じ書き方で扱います。入力は `UpdateGame` で読み、描画は `DrawGame` で行います。

同じ例は `Samples~/Tutorial/PointerOne.cs` と `PointerMany.cs`、`Taps.cs`、`KeySpace.cs` にあります。`Game.cs` に移すときはクラス名を `Game` に変えます。

## まず1本で動かす

四角形を動かすなら、操作ごとに `GcDrag` を1つ用意します。`UpdateGame` で `gc.Drag` を1回呼びます。描画と同じ `SetRectAnchor` と座標系を使ってください。マウスなら左ボタンでドラッグできます。

```csharp
#nullable enable
using GameCanvas;

public sealed class Game : GameBase
{
    GcRect box = new GcRect(100, 300, 180, 180);
    GcDrag drag = new GcDrag();

    public override void InitGame()
    {
        gc.ChangeCanvasSize(720, 1280);
        gc.SetRectAnchor(GcAnchor.UpperLeft);
    }

    public override void UpdateGame()
    {
        gc.Drag(drag, ref box);
    }

    public override void DrawGame()
    {
        gc.ClearScreen();
        gc.SetColor(40, 100, 230);
        gc.FillRect(box);
    }
}
```

`gc.Drag` は、押した指の ID だけを追います。短い押下で同じフレームに離しても、最後の位置まで動かします。中断や指の消失では、押し始める前の位置へ戻します。残っている別の指へ乗り移りません。

重なった複数の対象から、手前のものを自動では選びません。手前を自分で決めてから、その `GcDrag` だけを進めてください。

`GcDrag` の `Started` / `Ended` / `Cancelled` は、そのフレームだけ真です。`Active` は操作中ずっと真です。`PointerId` は掴んでいる指の番号で、掴んでいなければ空です。途中でやめたいときは `drag.Cancel(ref box)` です。矩形の位置は `box.X` / `box.Y`、大きさは `box.Width` / `box.Height` です。

`Down` はこのフレームに押し始めたこと、`Held` はフレーム末に押していること、`Up` は普通に離したことです。`Cancelled` は、アプリの中断や接続解除などで操作が取り消された状態です。押し始めたフレームでも `Held` は真です。ただし、次の更新までのあいだに押して離すと、`Down` と `Up` が両方真になり、`Held` は偽になります。その場合も開始位置と最後の位置は残るので、短い操作は抜けません。

当たり判定には押し始めの位置を使います。押下と移動が同じフレームに届くことがあるため、最後の位置 `X`・`Y` で掴むと、指が乗っていない対象を掴むことがあります。離す直前の移動を取りこぼさないように、位置の反映は `Held` だけでなく `Up` でも行います。`gc.Drag` も同じです。

`FillRect` は図形なので `SetRectAnchor` を使います。画像も同じです。文字は `SetStringAnchor` です。

代表の指を離しても、`gc.Pointer` はすでに押している別の指へ乗り移りません。次の新しい押下を待ちます。残った指を追い続けるなら、次の一覧を使います。手で指を追う1本の例は `PointerOne.cs` です。

## 当たり判定

`rect.Contains(point)` と `rect.Contains(x, y)` は、左上を基準にした矩形の内側です。回転は見ます。今の `RectAnchor` や座標系は見ません。右端と下端は含みません。

```csharp
var box = new GcRect(100, 300, 180, 180);
box.Contains(100, 300); // 内側
box.Contains(280, 300); // 右端なので外側
box.Contains(100, 480); // 下端なので外側
```

描画と同じ基準点と座標系で判定するときは `gc.Contains(rect, point)` です。`gc.Drag` もこちらと同じ判定です。

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

添字を使わずに全要素を順に読むなら、次のようにも書けます。`gc.Taps` と `gc.PointerEvents`、`gc.KeyEvents` も同じです。

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
| `Duration` | 押下からの秒数。型は `double`。終了フレームでは終了時点までの秒数。ホバーだけなら0 |

読み取りで状態は消費されません。`UpdateGame` と `DrawGame` では、同じフレームの状態を読めます。一覧の中身は次の更新で変わります。後から使う値は `var saved = gc.Pointers[i];` のようにコピーしてください。

マウスとペンの位置は、機器の登録だけでは原点に作りません。最初の入力通知から公開します。マウスの基本の押下は左ボタン、ペンはペン先です。タッチにホバーはありません。

位置を前フレームからの経過で進めるときは `gc.TimeSincePrevFrame` を使います。こちらは `float` の秒です。最初のフレームと、アプリが背面から戻った直後は 0 です。押している長さや起動からの時刻は `double` の秒です。長い経過どうしの差を `float` で取らないでください。

## タップ

タップは、短時間で動きが小さいまま普通に離したときに成立します。アプリの中断などの `Cancelled` は含みません。押し始めの `Down` とは別です。ボタンを押す・離す操作にはタップを使い、ドラッグの開始には `Down` を使います。

`gc.Taps` は、このフレームに成立したタップの押し始めた位置の一覧です。型は `GcReadOnlyList<GcPoint>` です。`Count` と添字で読みます。成立がなければ空です。フレームをまたいで貯める一覧ではありません。

同じ例は `Samples~/Tutorial/Taps.cs` にあります。

```csharp
#nullable enable
using GameCanvas;

public sealed class Game : GameBase
{
    int count;
    float lastX, lastY;

    public override void InitGame()
    {
        gc.ChangeCanvasSize(720, 1280);
    }

    public override void UpdateGame()
    {
        for (int i = 0; i < gc.Taps.Count; i++)
        {
            var p = gc.Taps[i];
            lastX = p.X;
            lastY = p.Y;
            count++;
        }
    }

    public override void DrawGame()
    {
        gc.ClearScreen();
        gc.SetColor(0, 0, 0);
        gc.SetFontSize(32);
        gc.DrawString($"タップ {count} 回", 40, 40);
        if (count > 0)
        {
            gc.SetRectAnchor(GcAnchor.MiddleCenter);
            gc.SetColor(40, 100, 230);
            gc.FillRect(lastX, lastY, 40, 40);
        }
    }
}
```

感度は `gc.TapSettings` です。`MaxDistance` は途中の移動を足した距離で、単位はキャンバスの画素です。始点と終点の直線距離ではありません。`MaxDuration` は秒です。既定は 25 と 0.125 です。

```csharp
gc.TapSettings = new GcTapSettings(40, 0.2f);
```

## 正確な操作順が必要な場合

状態は指ごとにまとまります。同じ対象を一方の指が離した直後に別の指が押す、といった操作順を厳密に扱う場合は `gc.PointerEvents` を使います。

```csharp
for (int i = 0; i < gc.PointerEvents.Count; i++)
{
    var e = gc.PointerEvents[i];
    // e.Id、e.Phase、e.X、e.Y、e.Time を発生順に処理します。
}
```

段階は `Begin`、`Hold`、`End`、`Cancelled`、`Hover` です。`Time` は起動からの秒数で、型は `double` です。この一覧は入力変化の記録であり、静止している指の `Hold` が毎フレーム入るとは限りません。同じ ID が何度も現れる場合があります。OSが同時にまとめて届けた変化に、実際には分からない前後関係を付け足すことはしません。変化がなければ空です。フレームをまたいで貯める一覧ではありません。

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

`Down` / `Held` / `Up` / `Cancelled` の意味はポインターと同じです。`Duration` は押してからの秒数で、型は `double` です。`GcKey` はキーの物理的な位置で、文字入力や配列の変換ではありません。画面上のキーボードは `gc.ShowScreenKeyboard()` です。

押し続けは `gc.Key(GcKey.Space).Held` で読みます。変化の列には押し続けは出ません。

## キーの変化を順に読む

複数のキーが同じフレームに押された、離された、といった順番が要るときは `gc.KeyEvents` を使います。型は `GcReadOnlyList<GcKeyEvent>` です。`Count` と添字で読みます。

```csharp
for (int i = 0; i < gc.KeyEvents.Count; i++)
{
    var e = gc.KeyEvents[i];
    // e.Key は GcKey、e.Phase は Down / Up / Cancelled、e.Time は double の秒です。
}
```

`Phase` は `Down`、`Up`、`Cancelled` です。押し続けの段階はありません。変化がなければ空です。`PointerEvents` と同じく、フレームをまたいで貯める一覧ではありません。

同じゲーム操作を `gc.Key(...)` と `gc.KeyEvents` の両方で処理すると二重に動くため、必要な方を選んでください。
