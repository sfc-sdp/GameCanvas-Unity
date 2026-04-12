# GameCanvas-Unity

慶應SFC「スマートデバイスプログラミング」講座向け教育用2Dゲームフレームワーク (Unity)

## Project Structure

- `Packages/GameCanvas/` — フレームワーク本体 (UPM パッケージ `jp.ac.keio.sfc.sdp`)
- `Assets/Game.cs` — 学生が編集するゲームロジック (`GameBase` を継承)
- `docs/` — DocFX 生成の API ドキュメント

## Build & Test

```bash
# EditMode テスト実行
unity-editor -batchmode -nographics -runTests -testPlatform EditMode -testResults ./test-result-editmode.xml

# PlayMode テスト実行
unity-editor -batchmode -nographics -runTests -testPlatform PlayMode -testResults ./test-result-playmode.xml

# コード整形
dotnet format ./Packages/GameCanvas/
```

## unity-cli (AI Agent Integration)

Unity Editor が起動中であれば、以下のコマンドでエディタを操作できる。

### Setup

```bash
# CLI インストール
uv tool install git+https://github.com/bigdra50/unity-cli

# Unity Editor 内で Window > Unity Bridge → Start Server → Connect
```

### Commands

```bash
u state                      # 接続状態確認
u console get                # コンソールログ取得
u console get -l E           # エラーのみ取得
u console clear              # コンソールクリア
u tests run edit             # EditMode テスト実行
u tests run play             # PlayMode テスト実行
u play                       # Play モード開始
u stop                       # Play モード停止
u refresh                    # アセット再読み込み
u screenshot                 # スクリーンショット取得
u scene hierarchy            # シーン階層表示
```

### Workflow for AI Agents

1. コード変更後は `u console get -l E` でコンパイルエラーを確認
2. テストは `u tests run edit` / `u tests run play` で実行
3. UI 変更後は `u screenshot` で視覚確認
4. アセット変更後は `u refresh` で再読み込み

## Conventions

- C# 11, nullable reference types 有効 (`csc.rsp`: `-nullable -langversion:latest`)
- 角度は度数法 (API 側)。内部でラジアンに変換
- リソースは型安全な struct (`GcImage`, `GcSound`, `GcFont`) で参照
- コミットメッセージ: `feat:`, `fix:`, `refactor:`, `chore:`, `docs:` prefix
