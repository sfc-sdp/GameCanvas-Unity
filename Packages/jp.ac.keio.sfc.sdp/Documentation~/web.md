# Web で開く

Web は部分対応です。Unity 6000.6.2f1、パッケージ 8.0.0-pre.1 です。実機センサーの課題の代用にはなりません。

## ビルドと開き方

通常のアプリは、ビルド設定で Web を選び、`Assets/Game.unity` をビルドします。出力先は、ビルドのときに指定したフォルダです。診断アプリは `bash scripts/unity-validate.sh web` です。出力は `Build/Validation/Web` です。

ビルドしたフォルダは HTTP サーバで開きます。ローカルのプレビュー例です。

```bash
python3 -m http.server 8000 --bind 127.0.0.1 --directory Build/Validation/Web
```

このコマンドは、その PC を使う本人だけが開くためのものです。外部公開は別です。ブラウザで `http://127.0.0.1:8000` を開きます。

カメラなど権限付き機能には、HTTPS か localhost の安全なコンテキストが要ります。通信は接続先の CORS 許可も要ります。無いと取れません。

## テンプレート

標準テンプレートは `Assets/WebGLTemplates/GameCanvas/index.html` です。画面サイズに合わせ、`autoSyncPersistentDataPath` は真です。既定の Player Settings の WebGL テンプレートは `PROJECT:GameCanvas` です。

## 実装と確認

実装があることと、確認した範囲は別です。カメラの入口は Web にもあります。位置情報と加速度は Web では未対応で、`Start` すると `Unsupported` です。

確認した環境は localhost 上の Codex 内蔵ブラウザ Chromium 153 です。画像と日本語、マウスの `gc.Drag`、Space の Down と Up、HTTP GET の本文と画像、404、Cancel、Timeout、保存と再読み込みを確認しました。画面をクリックしたあと、診断アプリは gc.IsPlayingSound(GcSoundTrack.SE) の戻り値が真であることをログに出しました。耳では再生を確認していません。カメラは許可待ちから `NotGranted` へ完了し、`Stop` で `Stopped` までを確認しました。実映像は未確認です。

Chrome や Safari の本体、スマホのブラウザ、タッチ、複数指、背景からの復帰は未確認です。内蔵ブラウザでの成功を、通常のブラウザの保証にはしません。
