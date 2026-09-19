# gen-api-docs

`Packages/jp.ac.keio.sfc.sdp/Runtime/Scripts` の C# を Roslyn で読み、VitePress 向けの API ページを出します。DLL は作りません。Unity への依存もありません。

サイト全体の生成はリポジトリ根から `bash scripts/build-docs.sh` です。API Markdown、`api.json`、ガイドのビルド、`docs/` への出力まで行います。

## 単体で回す

```bash
dotnet run --project scripts/gen-api-docs -- \
  --source Packages/jp.ac.keio.sfc.sdp/Runtime/Scripts \
  --output docs-src/api \
  --namespace-root GameCanvas
```

| フラグ | 意味 | 既定 |
| --- | --- | --- |
| `--source` | C# のディレクトリ（再帰） | 必須 |
| `--output` | 出力先 | 必須 |
| `--namespace-root` | リンクの起点になる名前空間 | `GameCanvas` |

## 出力

`--output` に次を書きます。

- `GameCanvas.*.md` — 型ごと 1 ファイル
- `index.md` — 型一覧
- `toc.json` — VitePress のサイドバー
- `api.json` — 型とメンバーの一覧。`scripts/build-docs.sh` はこれを `docs-src/public/api.json` へコピーする

出力先にある既存の `GameCanvas.*.md` は、生成の前に消します。削除した API のページを残さないためです。出力ディレクトリへの書き換えがあります。入力のソースは触りません。

`Obsolete`、`EditorBrowsable(Never)`、`GameCanvas.Editor`、内部専用の型は出ません。

## 制限

- `<inheritdoc/>` は、このリポジトリ内のインターフェイスからだけ解決します
- 解決できない `cref` はインラインのコードのまま残します
- ジェネリックの引数名はソースのまま出します
