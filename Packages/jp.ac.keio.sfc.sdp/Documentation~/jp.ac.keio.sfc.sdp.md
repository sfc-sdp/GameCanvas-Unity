# GameCanvas for Unity
Copyright (c) 2015-2026 Smart Device Programming.

This software is released under the MIT License, see [LICENSE](../LICENSE.md).

慶應義塾大学SFCの講義「スマートデバイスプログラミング」向けの、2Dゲームの枠組みです。学生が触るのは主に `Assets/Game.cs` です。関数の一覧は機械生成のAPIリファレンスを見ます。こちらの文章は、読んで動かすための教材です。入口の有無は `Runtime/Scripts` が正です。

## 読む順

1. [はじめに](getting-started.md) — `Game.cs`、座標、時間、乱数、画面のループ
2. [インストール](installation.md) — Unity 6000.6.2f1
3. [画像と日本語](images-and-text.md) — パスで画像を出す、画像と文字の基準点
4. [ポインターで操作する](pointer-input.md) — 1本の操作、forによる複数、タップ、キーボード
5. [端末の機能](device.md) — 位置情報、保存、加速度、カメラ、通信
6. [AIに頼む](ai.md) — 編集の頼み方と、成功の見分け方
7. [以前の書き方から移る](migration.md) — 削除した入口との対応

実行できる例は `Assets/Game.cs` と `Samples~/Tutorial/` にあります。実機への書き出しは [講義テキスト](https://github.com/sfc-sdp/SDP-Textbook) を見てください。
