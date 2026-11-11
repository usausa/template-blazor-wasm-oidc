# Template project for Blazor WebAssembly (OIDC)

[English](README.md) | 日本語

[Template](https://usausa.github.io/template/) project for Blazor WebAssembly (OIDC).

# ✅ 実装

| 分類 | 機能 |
| --- | --- |
| 認証 | OIDC(認可コード + PKCE) / ログインとログアウトのリダイレクト(RemoteAuthenticatorView) / JWT Bearer(IdP ディスカバリー) / ロールクレームのマッピング(Keycloak `realm_access`) / ロールポリシー |
| IdP | 開発用 IdP(Keycloak・レルムインポート) |
| テスト | IdP なしのトークン検証(テスト用署名鍵) / 認証ガード E2E / ログインから CRUD までの E2E(Keycloak・任意) |

# 📚 ドキュメント

| ドキュメント | 内容 |
| --- | --- |
| [oidc.md](docs/oidc.md) | ベースとの差分と開発用 IdP(Keycloak) |
| [tests.md](docs/tests.md) | テストと E2E テスト(Playwright) |

# 🔗 関連プロジェクト

| プロジェクト | 説明 |
| --- | --- |
| [template-blazor-wasm](https://github.com/usausa/template-blazor-wasm) | Blazor WebAssembly + ASP.NET Core API のテンプレート |
