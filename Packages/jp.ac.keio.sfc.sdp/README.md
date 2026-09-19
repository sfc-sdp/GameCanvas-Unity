# GameCanvas for Unity
Copyright (c) 2015-2026 Smart Device Programming.

This software is released under the MIT License, see [LICENSE](LICENSE.md).

慶應義塾大学SFCの講義「スマートデバイスプログラミング」向けの、2Dゲームの枠組みです。Unity は 6000.6.2f1 に固定しています。パッケージ版は 8.0.0-pre.1 です。プレリリースであり、正式版ではありません。

学生が触るのはプロジェクト側の `Assets/Game.cs` です。読み方は [Documentation~/jp.ac.keio.sfc.sdp.md](Documentation~/jp.ac.keio.sfc.sdp.md) から進めてください。関数の一覧は機械生成のAPIリファレンスを見ます。入口の有無は `Runtime/Scripts` が正です。

よく使う入口は次のとおりです。

- 画像: `gc.DrawImage("BlueSky.png", x, y)`
- 文字: `gc.DrawString(text, x, y)`
- 基準点: 図形・画像は `gc.SetRectAnchor`、文字は `gc.SetStringAnchor`
- 色: `gc.SetColor(r, g, b)` （0 から 255）
- 入力: `gc.Pointer` / `gc.Pointers` / `gc.Taps` / `gc.Key(GcKey.Space)`
- 乱数: `gc.Random()` は 0 以上 1 未満。整数は上限を含まない。サイコロは `gc.Random(1, 7)`
- 位置情報: `gc.Location.Start` / `Stop` / `TryGetSample`
