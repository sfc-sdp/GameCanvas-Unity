# AIに頼む

GameCanvas の編集をAIに頼むときは、バージョンと、触ってよいファイルを先に固定します。Unity は 6000.6.2f1 です。学生が書く場所は `Assets/Game.cs` です。画像と音は `Assets/Res` です。

関数の有無は、説明文より `Packages/jp.ac.keio.sfc.sdp/Runtime/Scripts` を正とします。生成されたAPIページや、ネット上の古いサンプルは、今のソースと違うことがあります。ソースに無い呼び出しは使わないでください。

## 頼むときの型

次のように頼むと、足りない前提を補われにくくなります。

> このプロジェクトは Unity 6000.6.2f1 に固定しています。作業ルールと `scripts/doctor.py --json` を先に読んでください。素材は `Assets/GameCanvas/asset-catalog.json` にあるものだけを使い、`Assets/Game.cs` を変更してください。コンパイル、テスト、再生で目に見えた結果を分けて報告してください。既存の変更と素材は消さないでください。

入力なら [ポインターで操作する](pointer-input.md)、画像なら [画像と日本語](images-and-text.md) を読ませてから頼んでください。`IsTouchBegan`、`IsTapped`、`IsKeyDown`、`LastPointerX`、`StartGeolocationService`、`PlayCameraImage`、`DrawCameraImage`、`RequestUserAuthorizedPermissionCameraAsync`、`TryGetImage` を必須にした最初の表示は、今の入口ではありません。入力は `gc.Pointer` / `gc.Pointers` / `gc.Taps` / `gc.Key(GcKey)` です。カメラは `gc.Camera.Start` と `gc.DrawCamera` です。

## エディタを操作する

このリポジトリには Unity Bridge が入っています。Unity エディタを開いた状態で、`Window > Unity Bridge` からサーバを開始し、接続します。接続できると、別の端末からエディタを操作できます。

```bash
uv tool install git+https://github.com/bigdra50/unity-cli
```

よく使う操作です。

```bash
u state
u console get -l E
u play
u stop
u screenshot
u tests run edit
u tests run play
```

コードを変えたあとは、コンソールのエラーを見てください。画面が絡む変更は、再生してスクリーンショットを撮るか、自分の目で確認します。AIの「動くはずです」だけでは成功にしません。

教材の完全な C# 例と `Samples~` を、一時的に Unity へ入れてコンパイルと起動の確認まで回すときは次です。

```bash
python3 scripts/validate-examples.py
```

サイトを出すときはリポジトリの根から `bash scripts/build-docs.sh` です。

`scripts/doctor.py` は、Unity の版やモジュールの不足を出します。`--json` はAIからも同じ内容を読めます。認証情報や端末の識別子を渡す必要はありません。Windows 11 向けの診断結果は、このリポジトリでは確認していません。

配布 ZIP はコミット済みのファイルだけから作ります。`python3 scripts/package-project.py` です。未コミットの変更や未追跡ファイル、追跡されている `design/` や `scripts/rendering/` があると拒否します。出力は ZIP と SHA256 と `release-info.json` です。Unity は 6000.6.2f1、パッケージ版は 8.0.0-pre.1 です。

## 成功の分け方

次は別の話です。混ぜて報告させないでください。

- コードがコンパイルできた
- EditMode / PlayMode のテストが通った
- エディタの再生で、期待した絵が出た
- 実機で同じ操作ができた

位置情報は Editor では `Unsupported` です。シミュレータの模擬位置は `IsMock` で区別します。実機の結果とエディタの再生を取り違えないでください。iOS シミュレータに実カメラは無く、機器が無い状態の確認だけです。実機の iOS カメラは未確認です。Web のカメラとブラウザ、Windows のカメラ、macOS のカメラは未確認です。
