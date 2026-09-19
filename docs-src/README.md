# GameCanvas Docs (VitePress)

手書きの教材はパッケージ側が正本です。`docs-src/guide/` の各ページは、同じブランチの `Packages/jp.ac.keio.sfc.sdp/Documentation~` を VitePress の include で読みます。`compatibility.md` だけ、パッケージの `migration.md` を読みます。

相対リンクは `/guide/` 配下に同名ページがある前提です。LICENSE への相対パスはパッケージ目次だけに残し、サイトへは include していません。

## ディレクトリ

```
docs-src/
├── .vitepress/config.mts      サイト設定（サイドバーの API は api/toc.json）
├── index.md                   トップ
├── guide/                     教材の読み込み口
├── api/                       自動生成（GameCanvas.*.md は生成のたびに作り直す）
└── public/                    静的アセット（api.json を含む）
```

API 生成ツールは `scripts/gen-api-docs/` です。同じブランチにあります。

## ローカル

リポジトリの根から次を実行します。API 生成と `docs/` への VitePress 出力まで行います。

```bash
bash scripts/build-docs.sh
```

プレビューだけするときは、生成のあと `docs-src` で `npx vitepress dev` します。公開 URL は `http://localhost:5173/GameCanvas-Unity/` です。

## CI

`.github/workflows/build-docs.yml` は `master` への push（`docs/**` は除く）と手動実行で動きます。同じリビジョンで `scripts/build-docs.sh` を呼び、`docs/` に差分があればコミットします。`docs/` だけが変わった push では再実行しません。
