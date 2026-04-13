import { defineConfig } from 'vitepress'
import apiSidebar from '../api/toc.json' with { type: 'json' }

// scripts/gen-api-docs/MarkdownRenderer.cs の SlugifyHeading と同一ロジック。
// `<see cref>` から生成する内部アンカーリンクと、実際の見出しの HTML id を一致させるために必須。
function slugify(s: string): string {
  const lower = s.toLowerCase()
  let out = ''
  for (const c of lower) {
    const code = c.charCodeAt(0)
    if (/[a-z0-9_\-]/.test(c) || code > 0x7F) out += c
    else out += '-'
  }
  return out.replace(/-+/g, '-').replace(/^-+|-+$/g, '')
}

// GitHub Pages: https://sfc-sdp.github.io/GameCanvas-Unity/
// 公開時はビルド出力を ../docs に切り替える予定 (別PR)
export default defineConfig({
  lang: 'ja',
  title: 'GameCanvas',
  description: '慶應義塾大学 SFC「スマートデバイスプログラミング」教材用 2D ゲームフレームワーク',
  base: '/GameCanvas-Unity/',
  lastUpdated: true,
  cleanUrls: true,
  // ビルド出力先。CI では `VITEPRESS_OUT_DIR` で master の `docs/` を指す絶対パスを注入する。
  // ローカルビルド時は `./dist` に出力 (master の `docs/` は GitHub Pages 配信中なので壊さない)。
  outDir: process.env.VITEPRESS_OUT_DIR ?? './dist',
  srcExclude: ['README.md'],
  markdown: {
    anchor: { slugify }
  },
  head: [
    ['link', { rel: 'icon', href: '/GameCanvas-Unity/logo.png' }]
  ],
  themeConfig: {
    logo: '/logo.png',
    search: {
      provider: 'local',
      options: {
        locales: {
          root: {
            translations: {
              button: { buttonText: '検索', buttonAriaLabel: '検索' },
              modal: {
                displayDetails: '詳細を表示',
                resetButtonTitle: 'リセット',
                noResultsText: '一致する結果がありません',
                footer: {
                  selectText: '選択',
                  navigateText: '移動',
                  closeText: '閉じる'
                }
              }
            }
          }
        }
      }
    },
    nav: [
      { text: 'ガイド', link: '/guide/getting-started' },
      { text: 'API', link: '/api/' },
      {
        text: 'リンク',
        items: [
          { text: 'GitHub', link: 'https://github.com/sfc-sdp/GameCanvas-Unity' },
          { text: 'SDP 講義', link: 'http://web.sfc.keio.ac.jp/~wadari/sdp/' },
          { text: '講義テキスト', link: 'https://github.com/sfc-sdp/SDP-Textbook' }
        ]
      }
    ],
    sidebar: {
      '/guide/': [
        {
          text: 'はじめに',
          items: [
            { text: 'GameCanvas とは', link: '/guide/getting-started' },
            { text: 'インストール', link: '/guide/installation' }
          ]
        },
        {
          text: 'リファレンス',
          items: [
            { text: '旧 API 対応表', link: '/guide/compatibility' }
          ]
        }
      ],
      '/api/': [
        { text: 'API リファレンス', items: [{ text: '概要', link: '/api/' }] },
        ...apiSidebar
      ]
    },
    outline: {
      level: [2, 3],
      label: '目次'
    },
    docFooter: {
      prev: '前のページ',
      next: '次のページ'
    },
    socialLinks: [
      { icon: 'github', link: 'https://github.com/sfc-sdp/GameCanvas-Unity' }
    ],
    footer: {
      message: 'Released under the MIT License.',
      copyright: 'Copyright © 2015-2026 Smart Device Programming.'
    },
    editLink: {
      pattern: 'https://github.com/sfc-sdp/GameCanvas-Unity/edit/master/docs-src/:path',
      text: 'GitHub でこのページを編集'
    },
    lastUpdatedText: '最終更新'
  }
})
