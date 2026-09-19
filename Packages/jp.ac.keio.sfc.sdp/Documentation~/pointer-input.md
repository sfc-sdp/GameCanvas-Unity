# ポインターで操作する

タッチ、マウスの左ボタン、ペン先を同じ書き方で扱います。入力は`UpdateGame`で読み、描画は`DrawGame`で行います。

## まず1本で動かす

`gc.Pointer`で代表のポインターを読みます。次の例は四角形を押した位置のずれを保って動かします。マウスなら左ボタンでドラッグできます。

```csharp
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

`Down`は押し始め、`Held`は押している状態、`Up`は正常な解放です。`Cancelled`はアプリの中断や接続解除などで操作が取り消された状態です。この例では中断すると四角形を元へ戻します。

押したフレームも`Held`は真です。ただし、次の更新までの間に押して離した場合は`Down`と`Up`が両方とも真になり、`Held`は偽になります。その場合も開始位置と最後の位置を使えるため、短い操作が抜けません。

`StartX`・`StartY`は押し始めた位置、`X`・`Y`は最後の位置です。押下と移動が同じフレームに届く場合があるので、当たり判定は開始位置で行います。最後の移動を取りこぼさないように、`Held`だけでなく`Up`でも位置を反映します。

## 複数の指を順に読む

`gc.Pointers`には、そのフレームのポインターが入っています。まずは`for`で、個数と添字を使って読みます。

```csharp
for (int i = 0; i < gc.Pointers.Count; i++)
{
    var p = gc.Pointers[i];
    // ここでp.Down、p.Held、p.Up、p.Cancelledを調べます。
}
```

`gc.Pointer`と`gc.Pointers[i]`は同じ型です。1本のときに覚えた読み方をそのまま使えます。

`i`は一覧の何番目かを示します。指を離すと並び順は変わるため、対象を動かしている指の記録には`p.Id`を使います。独立した2本指のドラッグでは、四角形ごとにID、押した位置のずれ、元の位置を保存します。別の指に操作中の四角形を奪わせないようにします。

`Count`は「今押している指の本数」ではありません。離した指はそのフレームまで、マウスやペンのホバーは押していなくても一覧に含まれます。押している本数が必要なら`Held`が真の要素を数えます。

代表の指を離しても、`gc.Pointer`はすでに押している別の指へ乗り移りません。次の新しい押下を待ちます。複数の指を追い続ける用途には一覧を使います。

## foreachで短く書く

添字を使わずに全要素を順に読む場合は、次のようにも書けます。

```csharp
foreach (var p in gc.Pointers)
{
    // forの例と同じ処理を書けます。
}
```

`foreach`は、このような繰り返しを短く書くための糖衣構文です。ここでは列挙子の仕組みを覚える必要はありません。`for`で処理を理解してから、短い書き方として使ってください。一般のC#では`foreach`と添字の`for`が常に同じ仕組みになるわけではありません。

この一覧は具体型のまま`for`または`foreach`で読む限り、列挙のためにヒープへ確保しません。`IEnumerable<T>`などのインターフェース型へ変換した場合はこの保証の対象外です。

## 状態の意味と寿命

| プロパティ | 意味 |
| --- | --- |
| `Id` | 操作中に変わらない番号。OSの指番号ではなく、機器をまたいでも衝突しない |
| `Kind` | `Touch`、`Mouse`、`Stylus`などの種別 |
| `Present` | 位置が有効か。入力がないときは偽。中断時の最後の位置は`Cancelled`でも確認できる |
| `Inside` | 最後の位置がキャンバス内か。外でもドラッグは継続できる |
| `Position` / `X`, `Y` | キャンバス上の最後の位置。右がX、下がYの正方向 |
| `StartPosition` / `StartX`, `StartY` | 押し始めた位置。ホバー中は操作の開始位置として使わない |
| `Delta` | 前フレーム末からの変位。新しい接触では開始位置からの変位 |
| `Duration` | 押下からの秒数。終了フレームでは終了時点までの秒数。ホバーだけなら0 |

読み取りで状態は消費されません。`UpdateGame`と`DrawGame`では同じフレームの状態を読めます。一覧のビューは次の更新で内容が変わります。後から使う値は`var saved = gc.Pointers[i];`のようにコピーしてください。

マウスとペンの位置は、登録だけで原点に作らず、最初の入力通知から公開します。

マウスの基本の押下は左ボタン、ペンはペン先です。右ボタン・ホイール・筆圧の新APIはこの入口にはまだ含みません。タッチにホバーはありません。

## 正確な操作順が必要な場合

状態は指ごとに集約されます。同じ対象を一方の指が離した直後に別の指が押す、といった操作順を厳密に扱う場合は`gc.PointerEvents`を使います。

```csharp
for (int i = 0; i < gc.PointerEvents.Count; i++)
{
    var e = gc.PointerEvents[i];
    // e.Id、e.Phase、e.X、e.Yを発生順に処理します。
}
```

段階は`Begin`、`Hold`、`End`、`Cancelled`、`Hover`です。この一覧は入力変化の記録であり、静止している指の`Hold`が毎フレーム入るとは限りません。同じIDが何度も現れる場合があります。OSが同時にまとめて届けた変化に、実際には分からない前後関係を付け足すことはしません。

通常の独立ドラッグには状態の一覧を使います。同じゲーム操作を状態とイベント列の両方で処理すると二重に動くため、必要な方を選んでください。
