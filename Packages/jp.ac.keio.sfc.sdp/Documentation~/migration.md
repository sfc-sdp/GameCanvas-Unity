# 以前の書き方から移る

v8 では、よく使う入力・描画・乱数・位置情報の入口を足し、廃止予定だった宣言と旧い入口を削除しました。新しい課題は、削除した名前に戻さないでください。

機械生成のAPIページは、ソースより遅れていることがあります。移したあとの正しさは `Runtime/Scripts` で確認します。

## ポインター

1本の指やマウスは `gc.Pointer`、複数は `gc.Pointers` です。イベントの発生順が要るときだけ `gc.PointerEvents` を使います。タップは `gc.Taps` です。

| 以前 | 今 |
| --- | --- |
| `LastPointerX` / `LastPointerY` | `gc.Pointer.X` / `gc.Pointer.Y` |
| `LastPointerPoint` / `LastPointerEvent` / `LastPointerTime` / `LastPointerFrame` | `gc.Pointer` か `gc.PointerEvents` |
| `IsTouchBegan()` | `gc.Pointer.Down` |
| `IsTouched()` | そのフレームに接触があるか。`Down` / `Held` / `Up` を見る |
| `IsTouchEnded()` | `gc.Pointer.Up` |
| `IsTapped()` | `gc.Taps.Count != 0` |
| `IsTapped(out point)` | `gc.Taps` の `Count > 0` を確認してから `gc.Taps[0]` を使う。位置は押し始めた位置 |
| `TryGetPointerEventAll` から始める導入 | `gc.Pointers`。順序が要るときだけ `gc.PointerEvents` |
| `TryGetPointerEvent` / `TryGetPointerTrace` | 削除。`gc.Pointers` か `gc.PointerEvents` |
| `GetPointerX(i)` | `gc.Pointers[i].X` |
| `PointerCount` | 今の一覧は `gc.Pointers.Count`。変化の個数は `gc.PointerEvents.Count` |
| `PointerBeginCount` / `PointerEndCount` | `Down` / `Up` を数える |
| `PointerTapCount` / `TryGetPointerTapPoint` | `gc.Taps.Count` / `gc.Taps[i]` |
| `GcPointerTrace` | 削除。状態は `gc.Pointers[i]` |
| 中断が `End` に混ざる | `Cancelled`。タップや普通の解放には数えない |
| 代表の指を離すと、残った指へ引き継ぐ | 引き継がない。残った指は一覧で追う |

`LastPointer*` は過去フレームの最後のイベントも残していたが、`Pointer` と `PointerEvents` は同じ意味ではなく現在のフレームを指すので、過去の座標を使う場合は自分で値を保存する。

タップは、短時間で動きが小さい正常な解放だけです。押し始めの `Down` とは別です。位置は押し始めた点です。感度は `gc.TapSettings` のままです。`MaxDistance` は途中の移動を足した距離（キャンバス単位）、`MaxDuration` は秒です。既定は 25 と 0.125 です。

同じ対象を、状態の一覧とイベント列の両方で動かすと二重に処理されます。どちらか一方にします。

## キーボード

| 以前 | 今 |
| --- | --- |
| `IsKeyDown(Key.Space)` | `gc.Key(GcKey.Space).Down` |
| `IsKeyPress(Key.Space)` | `gc.Key(GcKey.Space).Held` |
| `IsKeyHold(Key.Space)` | 押した瞬間を除く押し続け。今は `Held` が真かつ `Down` が偽 |
| `IsKeyUp(Key.Space)` | `gc.Key(GcKey.Space).Up` |
| `IsAnyKey` / `IsAnyKeyDown` / `IsAnyKeyHold` / `IsAnyKeyPress` / `IsAnyKeyUp` | 削除。見たいキーを `gc.Key` で読む |
| `TryGetKeyEvent` / `TryGetKeyEventAll` | `gc.KeyEvents` |
| `TryGetKeyTrace` / `TryGetKeyTraceAll` / `GcKeyTrace` | 削除。状態は `gc.Key`、変化は `gc.KeyEvents` |
| `KeyDownCount` / `KeyHoldCount` / `KeyPressCount` / `KeyUpCount` | 削除。`gc.KeyEvents.Count` は Down と Up と Cancelled をすべて含む変化の数で、DownCount や UpCount そのものではない。必要な Phase で数える。押し続けは `gc.Key(...).Held` を使う。Hold イベントは存在しない |
| `KeyEscape` | `gc.Key(GcKey.Escape)` |
| `GetKeyPressDuration` / `GetKeyPressFrameCount` | `GetKeyPressDuration` は `gc.Key(...).Duration` へ置換できる。`GetKeyPressFrameCount` は削除で、直接の代替はない。`Duration` は秒で、フレーム数ではない。フレーム数が必要なら、自分で数える |
| `KeyCode` や文字1つを渡す引数 | 削除済み。`GcKey` を使う |
| イベントの `Phase.Hold` | 削除。押し続けは `gc.Key(...).Held` |

`gc.KeyEvents` は `GcReadOnlyList<GcKeyEvent>` です。`Count` と添字で、このフレームの入力変化を順に読みます。`Key` は `GcKey`、`Phase` は `Down` / `Up` / `Cancelled` です。変化がなければ空です。`PointerEvents` と同じく、フレームをまたいで貯める一覧ではありません。

## 時間

| 以前 | 今 |
| --- | --- |
| `TimeSinceStartup` が `float` | `double` の秒 |
| ポインター / キーの `Duration` | どちらも `double` の秒 |
| イベントの `Time` | `double` の秒 |
| `TimeSincePrevFrame` | `float` の秒のまま。座標の更新に使う |

長い経過どうしの差を `float` で取らないでください。

## 乱数

整数の範囲は、上限を含む仕様から、上限を含まない仕様へ変わりました。小数の範囲は以前から上限を含みません。

| 以前 | 今 |
| --- | --- |
| `gc.Random()` | 0 以上 1 未満の `float`。同じ |
| なし | `gc.Random(6)` は 0 以上 6 未満の整数 |
| `gc.Random(1, 6)` が 1 から 6 | `gc.Random(1, 7)` が 1 から 6。サイコロはこちら |
| `gc.Random(0, array.Length - 1)` | `gc.Random(array.Length)` |
| 空や逆の範囲 | `ArgumentOutOfRangeException` |
| 小数の NaN / Infinity | `ArgumentOutOfRangeException` |

`gc.SetRandomSeed` で再現はできます。以前の版と同じ乱数列になることは約束しません。

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

## 画像と文字と矩形

| 以前 | 今 |
| --- | --- |
| `TryGetImage` してから `DrawImage` | `gc.DrawImage("BlueSky.png", x, y)` |
| `DrawRightString` / `DrawCenterString` | `gc.SetStringAnchor(...)` のあと `DrawString` |
| `SetColor` の float と byte の取り違え | `SetColor(int, int, int, int = 255)`。0 から 255。範囲外は丸める |
| 0 から 1 の色 | `GcColor.FromNormalized`。NaN は不可 |
| `DrawCameraImage` が止まっているカメラを再生する | 既定は再生しない。先に `PlayCameraImage`。従来の連携だけ `autoPlay: true` |
| `GcRect` の `Radian` | 公開しない。`Rotation` に度数を入れる |
| `rect.Degree()` | `rect.Rotation` |
| `new GcRect(x, y, w, h, radian)` | `new GcRect(x, y, w, h)`。回転は `{ Rotation = 30 }` か `GcRect.FromDegrees(x, y, w, h, 30)` |
| 描画や座標回転の `degree` 引数 | `rotation`。時計回りの度。`Sin` / `Cos` の `degree` はそのまま |
| カメラの取得角度 | `TryGetCameraImageRotation(..., out var rotation)` |

図形、画像、Texture、カメラ画像、オンライン画像は `SetRectAnchor` です。文字は `SetStringAnchor` です。引数なし、`GcPoint`、数値の x,y、`GcRect` のどれでも、直前の設定を使います。任意の画像の有無で分岐するときや、同じハンドルを繰り返すときだけ `TryGetImage` を使います。

矩形指定の `DrawImage` / `DrawString` の `rotation` は、矩形の回転へ加算します。渡した `GcRect` 自体は変わりません。

## アクター

| 以前 | 今 |
| --- | --- |
| `GetActorList<T>` が返した `ReadOnlyActorList<T>` | `TryGetActorAll<T>` |

`GetActorList` と `ReadOnlyActorList<T>` はありません。
