# 以前の書き方から移る

v8 では、よく使う入力・描画・乱数・位置情報・カメラの入口を足し、廃止予定だった宣言と旧い入口を削除しました。新しい課題は、削除した名前に戻さないでください。

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
| `TimeSinceStartup` が `float` | `double` の秒。Unity の単調時計。端末の日時変更には影響されない |
| ポインター / キーの `Duration` | どちらも `double` の秒 |
| イベントの `Time` | `double` の秒 |
| `TimeSincePrevFrame` | `float` の秒のまま。座標の更新に使う。初回とアプリが背面から戻った直後は 0 |
| `CurrentTimestamp` | UTC の UNIX 秒 |
| `Thread.Sleep` や待ちループでフレームを待つ | しない。待機は Unity へ |
| `SetFrameRate` | 正の整数 |
| `SetFrameInterval` | `1.0 / int.MaxValue` 以上 1 以下の有限秒。最寄り整数 fps へ丸める |
| 実 fps の保証、以前の手動待機の精度 | 保証しない。デスクトップで垂直同期が有効なときは画面の更新を優先。モバイルでは fps の希望値 |
| 無効化して再有効化 | 内部サービスを再生成し、`InitGame` を再実行する。`Game` のフィールドは残る |

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

## カメラ

旧いカメラの入口は削除しました。`IInputCamera` は残っており、新しいメンバーは `Camera` と `DrawCamera` です。旧いメンバーと `IInputCameraEx` はありません。コールバック、`requestId`、許可待ちの変数、`PauseGame`、再生中フラグは、簡単な例では不要です。

| 以前 | 今 |
| --- | --- |
| `PlayCameraImage` | `gc.Camera.Start()`。許可から入る。希望の幅・高さ・fps は引数 |
| `StopCameraImage` | `gc.Camera.Stop()`。許可待ちも取り消す |
| `RequestUserAuthorizedPermissionCameraAsync` | 削除。`Start` が許可の確認から入る |
| `HasUserAuthorizedPermissionCamera` | `gc.Camera.Permission` |
| `PauseCameraImage` | 一時停止の入口はない。止めるなら `Stop`。背面では GameCanvas が止める |
| `IsPlayingCameraImage` | `gc.Camera.Status == GcCameraState.Running`。最初の有効な映像を受け取ったとき。`PlayCameraImage` の成功とは違う |
| `DidUpdateCameraImageThisFrame` | `gc.Camera.Updated` |
| `TryGetCameraImage` / `TryGetCameraImageAll` / `CameraDevices` / `CameraDeviceCount` | 許可のあと `gc.Camera.Devices`。使用中のカメラは `gc.Camera.Device`。`Count` と添字で読む |
| `TryGetCameraImage(deviceName)` | `Devices` から名前で探すか、`Start(device)` |
| `UpdateCameraDevice` | 削除。`Start` の許可のあと調べる |
| `TryGetCameraImageSize` | `gc.Camera.Width` / `Height`。向きを補正した整数。`Running` 以外は 0 |
| `TryGetCameraImageRotation` | `gc.Camera.Rotation`。以前は `Repeat(-nativeAngle, 360)`。今は `Repeat(nativeAngle, 360)` の時計回りの補正です。符号と向きが逆です。`DrawCamera` は自動で直します |
| `IsFlippedCameraImage` | `gc.Camera.IsMirrored` |
| `TryChangeCameraImageResolution` | `Start` の希望値。実際の大きさは映像のあと |
| `GetPrimaryCameraResolution` | 削除。希望値は `Start` の引数 |
| `FocusCameraImage` | `gc.Camera.Focus(x, y)`。左下 (0, 0)、右上 (1, 1)。解除は `ResetFocus` |
| `DrawCameraImage` | `gc.DrawCamera`。基準点は `SetRectAnchor`。描画は開始しない。`GcPoint` と `float2` の位置もある。`DrawCamera(GcRect)` は `rect.Rotation` を使う。追加の回転は数値や点の引数だけ |
| `DrawCameraImage(..., autoPlay)` | 削除。描画のついでに開始しない |
| `DrawCameraImage` の width / height | 今は描く先の大きさ。以前は元映像の解像度が掛かっていた。引数なしの実寸はこの修正の対象外です |
| `GetOrCreateCameraTexture` | 削除。低レベルのテクスチャは出さない |
| 複数カメラの同時再生 | このサービスでは非対応 |
| デプスカメラ | このサービスでは非対応。デプスカメラは一覧に出ません |

`Start` は操作のときに呼びます。呼び直すと前の処理を止めて始め直します。`Running` は最初の有効な映像を受け取った状態です。アプリが背面に回ると GameCanvas が止め、自動では再開しません。許可の画面で中断した場合も、戻ってからもう一度 `Start` します。

## 加速度

旧い加速度の入口は削除しました。`IInputAcceleration` / `IInputAccelerationEx`、`GcAccelerationEvent`、`IsAccelerometerEnabled`、`AccelerationEvents` はありません。入口は `gc.Acceleration` です。

| 以前 | 今 |
| --- | --- |
| `IsAccelerometerEnabled = true` | `gc.Acceleration.Start()`。希望の周波数は引数のHz |
| `IsAccelerometerEnabled = false` | `gc.Acceleration.Stop()`。値と履歴を消す |
| `IsAccelerometerSupported` | `gc.Acceleration.IsSupported` |
| `LastAccelerationEvent.Acceleration` | `gc.Acceleration.X` / `Y` / `Z`。有無は `HasValue` |
| `LastAccelerationEvent.RawAcceleration` | `gc.Acceleration.Last` の `RawX` / `RawY` / `RawZ` |
| `LastAccelerationEvent.Time` / `DeltaTime` | `gc.Acceleration.Time` / `DeltaTime`。どちらも `double` の秒。最初の `DeltaTime` は 0 |
| `DidUpdateAccelerationThisFrame` | `gc.Acceleration.Updated` |
| `AccelerationEvents` / `AccelerationEventCount` / `TryGetAccelerationEvent` | `gc.Acceleration.Events`。`Count` と添字で読む |
| `AccelerometerSamplingRate` | `Start` の引数 |
| `GcAccelerationEvent` の `float3` | 削除。Unity の型は出さない |
| 有効にしたまま背面から戻る | 自動では再開しない。もう一度 `Start` |

`Start` は操作のときに呼びます。呼び直すと前の処理を止めて始め直します。`Running` は最初の標本を受け取った状態です。値は重力を含む g です。キャンバス向きの X,-Y,-Z は以前と同じです。アプリが背面に回ると GameCanvas が止め、自動では再開しません。Editor と、Input System がセンサーを登録していない Web では未対応です。実測していない値を、測れたかのように出さないでください。
## 通信

旧い通信の入口は削除しました。`TryGetOnlineImage`、`TryGetOnlineSound`、`TryGetOnlineText`、`DrawOnlineImage`、`GetOnlineImageSize`、`ClearDownloadCache`、`GcAvailability` はありません。

| 以前 | 今 |
| --- | --- |
| `TryGetOnlineImage` | `gc.Network.GetImage(url)`。戻りは `GcImageRequest` |
| `TryGetOnlineSound` | `gc.Network.GetSound(url, GcSoundFormat)`。戻りは `GcSoundRequest` |
| `TryGetOnlineText` | `gc.Network.GetText(url)`。戻りは `GcTextRequest` |
| `DrawOnlineImage` | 取得は `GetImage`、描画は `gc.DrawImage(request, x, y)` |
| `GetOnlineImageSize` | `request.Width` / `Height`。成功前と破棄後は 0 |
| `ClearDownloadCache` / `ClearDownloadCacheAll` | 削除。成功データは `request.Dispose` まで残る |
| `GcAvailability` | `GcRequestState`。`Pending` / `Succeeded` / `Failed` / `Cancelled` / `TimedOut` / `Disposed` |
| 同じ URL を毎フレーム渡して進める | 返った request を残す。呼び出しのたびに新しい通信が始まる |
| POST の自動再試行 | しない。やり直すなら先に `Dispose` してから新しい操作 |
| pause で `Cancelled` になった GET | タップで `Dispose` してから新しい GET。毎フレーム再試行しない |
| 無効化して再有効化 | 新しい `gc` になる。`InitGame` で古い操作を `Dispose` し、変数を `null` へ戻す |

アプリが背面に回ると、待ち中の通信は `CancelAll` されます。成功済みのデータは残ります。サーバ側の処理まで止まったとは限りません。

## ドラッグと当たり判定

| 以前 | 今 |
| --- | --- |
| `StartX` / `StartY` の範囲比較 | `rect.Contains` か `gc.Contains(rect, point)` |
| 手で ID とずれを覚える導入 | 1個なら `gc.Drag(drag, ref rect)`。操作ごとにインスタンスを1つ |
| 右端・下端を含む判定 | 含まない |
| 残った指へ乗り移る | 乗り移らない。中断や消失では開始位置へ戻る |
| 重なりの手前を自動で掴む | 自動ではない。手前は自分で決める |

`rect.Contains` は左上基準の幾何です。描画と同じ `RectAnchor` と座標系で見るときは `gc.Contains` です。`gc.Drag` も `gc.Contains` と同じ判定です。`UpdateGame` で対象ごとに1回呼びます。

## 画像と文字と矩形

| 以前 | 今 |
| --- | --- |
| `TryGetImage` してから `DrawImage` | `gc.DrawImage("BlueSky.png", x, y)` |
| `DrawRightString` / `DrawCenterString` | `gc.SetStringAnchor(...)` のあと `DrawString` |
| `SetColor` の float と byte の取り違え | `SetColor(int, int, int, int = 255)`。0 から 255。範囲外は丸める |
| 0 から 1 の色 | `GcColor.FromNormalized`。NaN は不可 |
| `GcRect` の `Radian` | 公開しない。`Rotation` に度数を入れる |
| `rect.Degree()` | `rect.Rotation` |
| `new GcRect(x, y, w, h, radian)` | `new GcRect(x, y, w, h)`。回転は `{ Rotation = 30 }` か `GcRect.FromDegrees(x, y, w, h, 30)` |
| 描画や座標回転の `degree` 引数 | `rotation`。時計回りの度。`Sin` / `Cos` の `degree` はそのまま |

図形、画像、Texture、カメラ映像、通信で取った画像は `SetRectAnchor` です。文字は `SetStringAnchor` です。引数なし、`GcPoint`、数値の x,y、`GcRect` のどれでも、直前の設定を使います。任意の画像の有無で分岐するときや、同じハンドルを繰り返すときだけ `TryGetImage` を使います。カメラ映像は `gc.DrawCamera` です。幅と高さは描く先の大きさです。

矩形指定の `DrawImage` / `DrawString` の `rotation` は、矩形の回転へ加算します。渡した `GcRect` 自体は変わりません。

## アクター

| 以前 | 今 |
| --- | --- |
| `GetActorList<T>` が返した `ReadOnlyActorList<T>` | `TryGetActorAll<T>` |

`GetActorList` と `ReadOnlyActorList<T>` はありません。
