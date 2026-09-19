# インストール

Unity の版は 6000.6.2f1 に固定しています。別の版ではモジュールの名前や開き方が違います。

## 用意するもの

1. [Unity Hub](https://unity3d.com/jp/get-unity/download) を入れ、6000.6.2f1 を選びます。
2. 実機へ出す予定があるなら、Android Build Support（SDK & NDK Tools と OpenJDK を含む）と iOS Build Support、日本語言語パックも付けます。Android Build Support だけだと SDK が入らないことがあります。
3. このリポジトリのフォルダを、Unity Hub の「リストに追加」から登録します。
4. プロジェクトを開き、`Assets/Game.unity` を表示した状態で再生ボタンを押します。

青空の画像と日本語、右上の経過秒数が出れば、エディタ側の準備は足りています。再生で何も出ないときは、開いているシーンが `Assets/Game.unity` かを見てください。メニューの `GameCanvas/エディタ設定/起動時に Game.unity を開く` を入れておくと、次から迷いにくくなります。

Windows での導入は、このリポジトリでは確認していません。Android 実機ではポインターとキーの押下・解放を確認しています。iOSシミュレータではタップ、ドラッグ、ホームへ移ったあとの復帰と、そのあとの新しいドラッグ、日本語表示を確認しています。iOSの複数指と、接触中のキャンセルは未確認です。

実機への書き出しは、メニューの `GameCanvas/アプリをビルドする` と [講義テキスト](https://github.com/sfc-sdp/SDP-Textbook) を見てください。
