# GameCanvas Docs (VitePress)

GameCanvas for Unity のドキュメントサイト。[VitePress](https://vitepress.dev/) で構築している。

このディレクトリおよび `../scripts/gen-api-docs/` は `docs/vitepress` ブランチ専用で、`master` ブランチには含まれない。利用者 (学生) が `master` を clone しても Node.js / .NET 依存が降ってこないようにしている。

## ディレクトリ構成

```
docs-src/                       (本ディレクトリ、docs/vitepress ブランチにのみ存在)
├── .vitepress/config.mts      サイト設定 (サイドバーは api/toc.json を import)
├── index.md                   トップ (hero)
├── guide/                     手書きガイド
│   ├── getting-started.md
│   ├── installation.md
│   └── compatibility.md       旧 DocFX の note/compatibility.md を移植
├── api/                       (自動生成、.gitignore 対象)
│   ├── index.md               API リファレンス目次
│   ├── toc.json               サイドバー定義 (VitePress config.mts が import)
│   └── GameCanvas.*.md        各型 1 ファイル
└── public/                    静的アセット

../scripts/gen-api-docs/        Roslyn ベースの API 生成ツール (.NET 8, docs/vitepress ブランチ)
```

## ローカル開発

本ブランチは orphan ブランチで Unity 本体のソースを持たないため、Roslyn ツールの入力になる `Packages/jp.ac.keio.sfc.sdp/Runtime/Scripts/` が存在しない。ローカルで作業するには `master` を別パスに clone しておく。

```
workspace/
├── GameCanvas-Unity/           master (Unity プロジェクト本体)
└── GameCanvas-Unity-docs/      docs/vitepress (本ブランチ)
```

```bash
cd GameCanvas-Unity-docs/docs-src
npm install

# master の Packages を明示指定して API Markdown を生成
dotnet run --project ../scripts/gen-api-docs -- \
  --source ../../GameCanvas-Unity/Packages/jp.ac.keio.sfc.sdp/Runtime/Scripts \
  --output api

# VitePress を起動 (predev/prebuild フックを経由しない)
npx vitepress dev        # http://localhost:5173/GameCanvas-Unity/
npx vitepress build      # docs-src/dist にビルド
```

`package.json` の `predev` / `prebuild` フックは同一ワークツリー上に `../Packages` がある想定 (CI の `actions/checkout` で両ブランチを同じワークスペースに展開するケース)。orphan ブランチ単独で `npm run dev` / `npm run build` を実行すると `Packages` が見つからず失敗するため、上記のように個別に呼ぶこと。

## CI / デプロイ

`.github/workflows/build-docs.yml` が以下のタイミングで発火する。

- **master への push** — フレームワーク本体の変更で API が変わった可能性があるため再生成
- **docs/vitepress への push** — 手書き MD・サイト設定・生成ツールの変更
- **手動トリガー** (`workflow_dispatch`)

workflow は次の順で動く。

1. `master` と `docs/vitepress` を別パスに checkout
2. Roslyn ツールで `main/Packages/.../Runtime/Scripts` → `docs-branch/docs-src/api/*.md` を生成
3. `npx vitepress build` で `main/docs/` (= `master:docs/`) にビルド出力
4. `master:docs/` に差分があれば通常コミットとして push

`paths-ignore: ['docs/**']` によって、自動コミット (変更は `docs/**` のみ) 単独の push では本 workflow が再発火せず、無限ループしない。

GitHub Pages は `master` ブランチの `/docs` を配信している。ここに commit された成果物がそのまま公開される。

## 切替 PR で対応すべき残タスク

- **旧 URL 救済** — 旧 DocFX の `/api/*.html`, `/note/compatibility.html` への静的リダイレクトページを生成する (GitHub Pages では HTTP 301 を設定できないため、該当パスに `<meta http-equiv="refresh">` の HTML を吐く)。
- **メタデータ移行** — `docs/favicon.ico` / `manifest.json` / OGP / Twitter card / `theme-color` を `docs-src/public/` に移して `config.mts` の `head` に追加。
- **compatibility.md の API リンク復元** — 旧 DocFX xref 記法を inline code `Name` に単純変換した状態。自動生成後は `[\`Name\`](./api/GameCanvas.XXX.md#yyy)` のような実リンクに戻す後処理を `scripts/gen-api-docs` または別スクリプトで実装する。
- **旧 `docs/` 削除** — DocFX ビルド成果物 (`docs/api/*.html` など) を VitePress のビルド成果物で完全に置換する PR。

## 移行メモ

- 旧 DocFX ソースリポジトリ: <https://github.com/sfc-sdp/GameCanvas-Unity-DocFX> (参考、今後は廃止予定)
- VitePress は 1.6.x 系を採用 (2.x はまだ next alpha)
