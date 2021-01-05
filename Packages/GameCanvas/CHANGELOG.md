# CHANGE LOG
## 5.0.12
### 不具合修正
- [#186](https://github.com/sfc-sdp/GameCanvas-Unity/issues/186) 整数が保存できない不具合への暫定対処

## 5.0.11
### 機能追加
- 位置情報系APIのサンプルプロジェクトを同梱 (Packages/GameCanvas/Sample~/Geolocation)
- カメラ系APIのサンプルプロジェクトを同梱 (Packages/GameCanvas/Sample~/DeviceCamera)
### 不具合修正
- [#177](https://github.com/sfc-sdp/GameCanvas-Unity/issues/177) gc.TryGetCameraImageAll でカメラの一覧が取得できない不具合の修正
- [#178](https://github.com/sfc-sdp/GameCanvas-Unity/issues/178) gc.PlayCameraImage で返る解像度が壊れていたのを修正
- [#179](https://github.com/sfc-sdp/GameCanvas-Unity/issues/179) gc.IsFlippedCameraImage が正しい値を返すように
- [#180](https://github.com/sfc-sdp/GameCanvas-Unity/issues/180) gc.DrawTexture(Texture, GcRect) の描画不具合を修正
- [#182](https://github.com/sfc-sdp/GameCanvas-Unity/issues/182) Android: ランタイムパーミッションの結果を取得できない不具合の修正
- [#183](https://github.com/sfc-sdp/GameCanvas-Unity/issues/183) 実機でのカメラ描画エラーの修正
### 仕様変更
- [#181](https://github.com/sfc-sdp/GameCanvas-Unity/issues/181) gc.DrawCameraImage はカメラの回転や上下反転を考慮して描画するように
- 推奨エディタバージョンを 2020.1.17f1 に変更

## 5.0.10
### 機能追加
- ネットワーク系APIのサンプルプロジェクトを同梱 (Packages/GameCanvas/Sample~/Networking)
### 不具合修正
- [#173](https://github.com/sfc-sdp/GameCanvas-Unity/issues/173) gc.DrawTexture が正しく描画されない不具合の修正
- [#174](https://github.com/sfc-sdp/GameCanvas-Unity/issues/174) IL2CPPビルドエラーの解消
### 仕様変更
- 推奨エディタバージョンを 2020.1.15f1 に変更

## 5.0.9
### 機能追加
- GcAABB に `float2 GetPoint(GcAnchor)` 関数を追加
### 仕様変更
- 拡張メソッド群のクラス名を変更
- `inheritdoc` 属性を追加
- 推奨エディタバージョンを 2020.1.13f1 に変更

## 5.0.8
### 仕様変更
- 推奨エディタバージョンを 2020.1.12f1 に変更

## 5.0.7
### 性能向上
- gc.AccelerationEvents 呼び出し時の boxing を回避

## 5.0.6
### 機能追加
- 加速度計関連のAPI追加
    - gc.DidUpdateAccelerationThisFrame
    - gc.AccelerationEvents
- 数学関連のAPI追加
    - gc.Repeat (GcMath.Repeat)
- 加速度計取り扱いのサンプルプロジェクトを同梱 (Packages/GameCanvas/Sample~/Acceleration)
### 仕様変更
- キャンバス座標系で扱いやすいように GcAccelerationEvent.Acceleration の Y軸, Z軸 それぞれの値を反転

## 5.0.5
### 不具合修正
- [#166](https://github.com/sfc-sdp/GameCanvas-Unity/issues/166) gc.GetPointerX, gc.GetPointerY を書いてもコンパイルエラーにならないように
- [#167](https://github.com/sfc-sdp/GameCanvas-Unity/issues/167) gc.CheckHitCircle, gc.CheckHitRect を書いてもコンパイルエラーにならないように

## 5.0.4
### 不具合修正
- [#164](https://github.com/sfc-sdp/GameCanvas-Unity/issues/164) GcAABB.XYWH が意図通りの値を返さない不具合の修正
- [#165](https://github.com/sfc-sdp/GameCanvas-Unity/issues/165) gc.TapSettings の初期値を正しいものに修正

## 5.0.3
### 不具合修正
- [#163](https://github.com/sfc-sdp/GameCanvas-Unity/issues/163) 文字色が反映されない不具合の修正

## 5.0.2
### 仕様変更
- Git インストール不要で動作するように

## 5.0.1
### 仕様変更
- [#161](https://github.com/sfc-sdp/GameCanvas-Unity/issues/161) JetBrains Rider サポート

## 5.0.0
### 新機能
- [#89](https://github.com/sfc-sdp/GameCanvas-Unity/issues/89) 描線の太さと端点を自由に設定できるように
- [#141](https://github.com/sfc-sdp/GameCanvas-Unity/issues/141) スクリプトのホットリロードに対応
- [#142](https://github.com/sfc-sdp/GameCanvas-Unity/issues/142) 矩形の原点を左上以外にして描画できるように
- [#143](https://github.com/sfc-sdp/GameCanvas-Unity/issues/143) アフィン変換行列をサポート
- [#144](https://github.com/sfc-sdp/GameCanvas-Unity/issues/144) キャンバス外の黒帯を 任意の色に変更できるように
- [#145](https://github.com/sfc-sdp/GameCanvas-Unity/issues/145) 文字列描画の回転・拡縮サポート
- [#146](https://github.com/sfc-sdp/GameCanvas-Unity/issues/146) 衝突判定：跳ね返りと壁ずりの便利関数を追加
- [#147](https://github.com/sfc-sdp/GameCanvas-Unity/issues/147) ポインターイベントをまとめて取得できるように
- [#148](https://github.com/sfc-sdp/GameCanvas-Unity/issues/148) 基本図形構造体の定義
- [#149](https://github.com/sfc-sdp/GameCanvas-Unity/issues/149) リソース名の命名規則・連番制限を無くし、自由なIDを設定できるように
- [#150](https://github.com/sfc-sdp/GameCanvas-Unity/issues/150) 任意のテクスチャーを描画できるように
- [#151](https://github.com/sfc-sdp/GameCanvas-Unity/issues/151) 任意のオーディオクリップを再生できるように
- [#152](https://github.com/sfc-sdp/GameCanvas-Unity/issues/152) 端末のデプスカメラ（深度カメラ）を取得・描画できるように
- [#153](https://github.com/sfc-sdp/GameCanvas-Unity/issues/153) 複数のカメラを同時に取得・描画できるように
- [#154](https://github.com/sfc-sdp/GameCanvas-Unity/issues/154) カメラ映像入力の解像度を変更できるように
- [#155](https://github.com/sfc-sdp/GameCanvas-Unity/issues/155) ポイントフォーカス（カメラの任意点に焦点をあわせる機能）のサポート
- [#156](https://github.com/sfc-sdp/GameCanvas-Unity/issues/156) カメラのランタイムパーミッション対応
- [#157](https://github.com/sfc-sdp/GameCanvas-Unity/issues/157) 位置情報のランタイムパーミッション対応
- [#158](https://github.com/sfc-sdp/GameCanvas-Unity/issues/158) 位置情報の精度を指定できるように
- [#159](https://github.com/sfc-sdp/GameCanvas-Unity/issues/159) オンラインの音声リソースをダウンロードできるように
### 仕様変更
- 描画処理の全面刷新
- Transparent シェーダーを単純化
- 画像の単色加算描画APIを削除
- 画像のクリッピング描画APIを削除
- ランダム生成器を UnityEngine.Random から Unity.Mathematics.Random に変更
- 初期解像度を Game のインスペクタで設定する機能を削除
- EDownloadState 構造体を GcAvailability 構造体に置き換え
- com.unity.collections パッケージに依存するように
- 内部クラスの命名規則を変更
- デフォルトリソースの保存場所を変更
- EditorBrowsable 属性の追加
- テストコードの用意
- v3以前の旧APIとの互換対応を削除
- エディタバージョン下限を 2020.1.8f1 に変更
### 不具合修正
- 衝突判定の不具合修正
- 一部環境でUnityの名前空間と競合する現象を回避

## 4.2.0
### 新機能
- [#140](https://github.com/sfc-sdp/GameCanvas-Unity/issues/140) 汎用的なシーンと遷移の仕組みを追加
    - シーン：画面遷移とアクター登録機構を持つオブジェクト
    - アクター：更新関数と描画関数を持つ汎用オブジェクト
- [#139](https://github.com/sfc-sdp/GameCanvas-Unity/issues/139) 画面塗りつぶし色を指定できるように (`gc.FillScreen`)
- [#138](https://github.com/sfc-sdp/GameCanvas-Unity/issues/138) 衝突判定の刷新 (`gc.HitTest`, `gc.CrossTest`, `gc.SweepTest`)
- [#138](https://github.com/sfc-sdp/GameCanvas-Unity/issues/138) プリミティブ構造体の追加 (`Box`, `Circle`, `Line`, `Segment`)
- [#138](https://github.com/sfc-sdp/GameCanvas-Unity/issues/138) 数学系便利関数の拡充
    - 浮動小数点演算 (`gc.Round`, `gc.Abs`, `gc.Clamp`, `gc.Min`, `gc.Max`, `gc.Sqrt`, `gc.Sin`, `gc.AlmostZero`, `gc.AlmostSame`)
    - ベクトル演算 (`gc.Rotate`, `gc.Dot`, `gc.Cross`)
- 2次元アフィン変換クラスの用意 (`AffineTransform`)
### 仕様変更
- [#137](https://github.com/sfc-sdp/GameCanvas-Unity/issues/137) `com.unity.mathematics` への依存関係を追加
- [#137](https://github.com/sfc-sdp/GameCanvas-Unity/issues/137) `UnityEngine.Vector2` 構造体を `Unity.Mathematics.float2` 構造体に置き換え
- [#138](https://github.com/sfc-sdp/GameCanvas-Unity/issues/138) Collisionエンジンの廃止、Physicsエンジンの新規追加
- 構造体引数へのin修飾子の適用
- キャンバス座標系を浮動小数点で処理するように
- `GameBase` クラスを `GameCanvas` 名前空間に移動
### 不具合修正
- [#135](https://github.com/sfc-sdp/GameCanvas-Unity/issues/135) エディタ実行時に最初の描画フレームが破棄されることがある現象への対策
- [#134](https://github.com/sfc-sdp/GameCanvas-Unity/issues/134) VSync有効時に描画が乱れる不具合の修正
## 4.1.1
- [#131](https://github.com/sfc-sdp/GameCanvas-Unity/issues/131) 起動時に iOS, Android 以外のビルドターゲットだったら警告する
- [#132](https://github.com/sfc-sdp/GameCanvas-Unity/issues/132) 起動時に Game.unity を自動で開く
- [#133](https://github.com/sfc-sdp/GameCanvas-Unity/issues/133) エディタ挙動のカスタマイズ機能
## 4.1.0
- [#53](https://github.com/sfc-sdp/GameCanvas-Unity/issues/53) 一時停止イベントと再開イベントの追加 (`PauseGame`, `ResumeGame`)
- [#127](https://github.com/sfc-sdp/GameCanvas-Unity/issues/127) より正確なフレームレートの実現 (`gc.SetFrameInterval`)
- [#128](https://github.com/sfc-sdp/GameCanvas-Unity/issues/128) タップイベントの追加 (`gc.IsTapped`)
- [#129](https://github.com/sfc-sdp/GameCanvas-Unity/issues/129) ローカルストレージAPIの変更 (`gc.TryLoad`, `gc.Save`)
- [#130](https://github.com/sfc-sdp/GameCanvas-Unity/issues/130) 描画不具合の修正 (`gc.DrawScaledRotatedImage`)
- エディタバージョン下限を 2019.4.5f1 に変更
## 4.0.3
- [#126](https://github.com/sfc-sdp/GameCanvas-Unity/issues/126) Res.asset 参照の自動修正機能の追加
- エラーメッセージの改善
## 4.0.2
- [#125](https://github.com/sfc-sdp/GameCanvas-Unity/issues/125) 特定環境でのビルドエラーを回避
## 4.0.1
- CHANGELOG.md の追加
- Documentation~フォルダの追加
## 4.0.0
- Initial release
