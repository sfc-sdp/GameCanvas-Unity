# CHANGE LOG
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
