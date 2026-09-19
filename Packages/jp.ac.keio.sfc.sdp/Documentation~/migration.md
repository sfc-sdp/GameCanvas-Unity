# 以前の書き方から移る

v8 では、よく使う入力・描画・位置情報の入口を足し、廃止予定だった宣言と旧い位置情報の入口を削除しました。新しい課題は、削除した名前に戻さないでください。

機械生成のAPIページは、ソースより遅れていることがあります。移したあとの正しさは `Runtime/Scripts` で確認します。

## ポインター

1本の指やマウスは `gc.Pointer`、複数は `gc.Pointers` です。イベントの発生順が要るときだけ `gc.PointerEvents` を使います。

| 以前 | 今 |
| --- | --- |
| `LastPointerX` / `LastPointerY` | `gc.Pointer.X` / `gc.Pointer.Y` |
| `IsTouchBegan()` | `gc.Pointer.Down` |
| `IsTouched()` | そのフレームに接触があるか。`Down` / `Held` / `Up` を見る |
| `IsTouchEnded()` | `gc.Pointer.Up` |
| `TryGetPointerEventAll` から始める導入 | `gc.Pointers`。順序が要るときだけ `gc.PointerEvents` |
| `GetPointerX(i)` | `gc.Pointers[i].X` |
| 中断が `End` に混ざる | `Cancelled`。タップや普通の解放には数えない |
| 代表の指を離すと、残った指へ引き継ぐ | 引き継がない。残った指は一覧で追う |

`LastPointerX` と `IsTouchBegan` はまだ残っていますが、最後に届いたイベントの位置であり、今つかんでいる指とは限りません。複数指のドラッグには使いません。

`PointerCount` はイベント数です。今ある指の本数ではありません。今の本数は `gc.Pointers.Count` です。押している本数なら、`Held` が真の要素を数えます。

同じ対象を、状態の一覧とイベント列の両方で動かすと二重に処理されます。どちらか一方にします。

## 位置情報

旧い位置情報の入口は削除しました。`GcGeolocationEvent`、`StartGeolocationService`、`StopGeolocationService`、`TryGetGeolocationEvent`、`GeolocationStatus`、`HasUserAuthorizedPermissionGeolocation` はありません。

| 以前 | 今 |
| --- | --- |
| `StartGeolocationService` | `gc.Location.Start()` |
| `StopGeolocationService` | `gc.Location.Stop()` |
| `TryGetGeolocationEvent` | `gc.Location.TryGetSample` |
| `GeolocationStatus` | `gc.Location.Status` |
| 権限要求と起動を別々に書く導入 | `Start` が許可の確認から入る |

値の有無は `TryGetSample` で一度に判断します。Editor では未対応です。緯度経度は `double` です。Editor 用の固定座標を、測位の成功として出さないでください。

## 画像と文字

| 以前 | 今 |
| --- | --- |
| `TryGetImage` してから `DrawImage` | `gc.DrawImage("BlueSky.png", x, y)` |
| `SetStringAnchor` のあと `DrawString` | `gc.DrawString(text, x, y, anchor: GcAnchor.UpperRight)` |
| `SetRectAnchor` を画像にも引き継ぐ | 位置指定の `DrawImage` / `DrawString` は呼び出しの `anchor` だけを見る |
| `DrawRightString` / `DrawCenterString` | `DrawString(..., anchor: ...)` |
| `SetColor` の float と byte の取り違え | `SetColor(int, int, int, int = 255)`。0 から 255。範囲外は丸める |
| 0 から 1 の色 | `GcColor.FromNormalized`。NaN は不可 |
| `DrawCameraImage` が止まっているカメラを再生する | 既定は再生しない。先に `PlayCameraImage`。従来の連携だけ `autoPlay: true` |

図形の `FillRect` や、位置を省略した `DrawImage(image)` / `DrawString(text)` は、これまでどおり `SetRectAnchor` / `SetStringAnchor` を使います。任意の画像の有無で分岐するときや、同じハンドルを繰り返すときだけ `TryGetImage` を使います。

## アクター

| 以前 | 今 |
| --- | --- |
| `GetActorList<T>` が返した `ReadOnlyActorList<T>` | `TryGetActorAll<T>` |

`GetActorList` と `ReadOnlyActorList<T>` はありません。

## キーボード

| 以前 | 今 |
| --- | --- |
| `IsKeyDown(Key.Space)` | `gc.Key(GcKey.Space).Down` |
| `IsKeyPress(Key.Space)` | `gc.Key(GcKey.Space).Held` |
| `IsKeyUp(Key.Space)` | `gc.Key(GcKey.Space).Up` |
| `KeyCode` や文字1つを渡す引数 | 削除済み。`GcKey` を使う |

`UnityEngine.InputSystem.Key` を渡す `IsKeyDown` は残っています。新しい課題は `gc.Key` で書いてください。
