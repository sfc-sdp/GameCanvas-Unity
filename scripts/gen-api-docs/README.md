# gen-api-docs

GameCanvas の C# ソースから VitePress 向けの API リファレンス Markdown を生成する Roslyn ベースのツール。

## 設計方針

- **ソースコード直読み**: `dotnet build` で DLL を作らず、`Microsoft.CodeAnalysis.CSharp` で `.cs` を直接パースする。Unity や UnityEngine への依存を持ち込まない。
- **単一入力・単一出力**: 入力ディレクトリ以下の `*.cs` を再帰的に読み、出力ディレクトリに `*.md` と `toc.json` を書く。副作用なし。
- **フィルタは旧 DocFX の `filter.yml` を踏襲**: `Obsolete` / `EditorBrowsable(Never)` / `GameCanvas.Editor` 名前空間 / 内部専用型を除外。

## 使い方

```bash
dotnet run --project scripts/gen-api-docs -- \
  --source Packages/jp.ac.keio.sfc.sdp/Runtime/Scripts \
  --output docs-src/api \
  --namespace-root GameCanvas
```

フラグ:

| フラグ | 説明 | デフォルト |
| --- | --- | --- |
| `--source` | C# ソースディレクトリ (再帰) | 必須 |
| `--output` | Markdown 出力先 | 必須 |
| `--namespace-root` | ルート名前空間 (リンク生成時の起点) | `GameCanvas` |

## 出力

- `<output>/<TypeName>.md` — 各 public 型 1 ファイル
- `<output>/index.md` — 型一覧 (namespace ごとにグループ化)
- `<output>/.toc.json` — VitePress `config.mts` が読み込むサイドバー定義

## 制限

- `<inheritdoc/>` の解決は同リポ内のインターフェイスからのみ行う。外部アセンブリのドキュメントは参照しない
- `cref` は可能な限り内部リンクに変換するが、解決できない参照は inline code のまま残す
- ジェネリック型の引数は `T` / `TKey` 等の名前でそのまま表示する
