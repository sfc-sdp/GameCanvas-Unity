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

実装があることと、確認した範囲は別です。位置情報と加速度は Web では未対応で、`Start` すると `Unsupported` です。カメラの入口は Web にもあります。画面を押してから `Start` します。ページが隠れると中断になり、カメラは止まります。戻っても自動では再開しないので、もう一度 `Start` してください。許可待ちの既定は 30 秒で、変更できます。操作がそれを超えると `TimedOut` になります。許可が済めば、もう一度 `Start` して再試行できます。

確認した環境は、localhost 上の Codex 内蔵ブラウザの Chromium 153、macOS 27 の Safari 27、iOS 26.4 Simulator の Safari 26.4、Pixel 9a（Android 17）の Chrome 145.0.7632.218 です。

Codex 内蔵ブラウザの Chromium 153 では、画像と日本語、マウスの `gc.Drag`、Space の Down と Up、HTTP GET の本文と画像、404、Cancel、Timeout、保存と再読み込みを確認しました。画面をクリックしたあと、`gc.IsPlayingSound(GcSoundTrack.SE)` は真でした。耳では再生を確認していません。カメラは許可待ちから `NotGranted` へ完了し、`Stop` で `Stopped` までを確認しました。実映像は未確認です。

Safari 27 では、画像と日本語、手操作の `gc.Drag`、Space の Down と Up、HTTP GET の本文と画像、404、Cancel、Timeout、保存と再読み込みを確認しました。音は Chromium 153 の結果だけです。別タブへ切り替えると `lifecycle.pause`、元のタブへ戻ると `lifecycle.resume` になり、カメラは `Stopped` でした。保存は 3 から 4 へ増え、通信診断も成功しました。カメラの実映像は見ていません。

iOS 26.4 Simulator の Safari 26.4 では、画像と日本語、HTTP GET の本文と画像、404、Cancel、Timeout を確認しました。シミュレータでマウスを動かし、青い矩形のドラッグを確認しました。Begin と End の座標は違っていました。複数指は未確認です。再読み込み後、保存は 1 から 2 へ増え、通信診断も成功しました。ホームへ移動すると `lifecycle.pause`、Safari に戻ると `lifecycle.resume` になり、カメラは `Stopped` でした。実映像はシミュレータでは見ていません。

Pixel 9a の Chrome では、localhost で画像と日本語、タッチのドラッグ、HTTP GET の本文と画像、404、Cancel、Timeout を確認しました。保存は増え、再読み込み後も残っていました。加速度は `Unsupported` でした。音ボタンのあと、`gc.IsPlayingSound` は真でした。耳では再生を確認していません。カメラは、ブラウザの許可と Android の許可を出したあと、実フレームが `Running` になりました。写っているものの上下と向きは見ていません。許可の操作が既定の待ちを超えた初回は `TimedOut` で終わり、許可のあとでもう一度 `Start` して再試行できました。ブラウザをバックグラウンドへ移すと一時停止し、戻ったあとは `Stopped` で、許可は `Granted` のままでした。開始操作でもう一度始まりました。

通常のデスクトップ Chrome と、HTTPS での公開は未確認です。背景からの復帰は、アプリ内ブラウザでは未確認です。確認した環境での成功を、未確認のブラウザの保証にはしません。
