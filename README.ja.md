# Template project for Blazor WebAssembly (OIDC)

[English](README.md) | 日本語

[Template](https://usausa.github.io/template/) project for Blazor WebAssembly (OIDC).

# ✅ 実装

| 分類 | 機能 |
| --- | --- |
| 認証 | OIDC(認可コード + PKCE) / ログインとログアウトのリダイレクト(RemoteAuthenticatorView) / JWT Bearer(IdP ディスカバリー) / ロールクレームのマッピング(Keycloak `realm_access`) / ロールポリシー |
| IdP | 開発用 IdP(Keycloak・レルムインポート) |
| テスト | IdP なしのトークン検証(テスト用署名鍵) / 認証ガード E2E / ログインから CRUD までの E2E(Keycloak・任意) |

# 🔀 ベースとの差分

- Frontend: `Microsoft.AspNetCore.Components.WebAssembly.Authentication` の `AddOidcAuthentication`(認可コード+PKCE)。IdP 設定は `wwwroot/appsettings.json`
- Backend: JwtBearer 検証を IdP メタデータベース(`Authority` 指定、署名鍵はディスカバリ)へ変更。ロールは Keycloak の `realm_access.roles` を `role` クレームへマッピング
- 自前の JWT 発行(`JwtTokenProvider`)・リフレッシュトークン(テーブル/サービス)・Account 系(テーブル/サービス/シード/PBKDF2)と `/api/auth/login`・`/api/auth/refresh`・`/api/auth/logout` は撤去(ユーザー管理は IdP 側)
- Frontend のログイン画面とトークンの保持・更新(`TokenStore` / `TokenRefreshService` / `JwtAuthorizationMessageHandler` / `JwtAuthenticationStateProvider`)は撤去(`AddOidcAuthentication` に置き換え)

# 🔐 開発用 IdP(Keycloak)

Podman で起動する(初回にレルム `template` を自動インポート)。

```
podman run --rm -p 8180:8080 -e KC_BOOTSTRAP_ADMIN_USERNAME=admin -e KC_BOOTSTRAP_ADMIN_PASSWORD=admin -v .\keycloak:/opt/keycloak/data/import quay.io/keycloak/keycloak:26.4 start-dev --import-realm
```

- レルム: `template` / クライアント: `template-blazor-wasm`(public、PKCE S256、redirect URI `http://localhost:8080/*`)
- 初期ユーザー: `admin` / `admin`(Administrator ロール)、`user` / `user`(ロールなし。削除操作は 403 になる)
- 管理コンソール: http://localhost:8180 (admin / admin)

IdP を変更する場合は、`wwwroot/appsettings.json`(Oidc セクション)、Backend の `Auth` 設定、`index.html` の preconnect 先、ロールクレームのマッピング(`KeycloakClaims` / `KeycloakClaimsPrincipalFactory`)を差し替える。

# 🧪 テスト

- UnitTests / IntegrationTests: IdP 不要(統合テストはテスト用署名鍵でトークンを検証)
- E2ETests: 認証ガードのテストは IdP 不要。ログイン〜CRUD 一巡(`DataCrudTests`)は上記 Keycloak を起動し、環境変数 `E2E_OIDC=1` を設定したときのみ実行される

# 🔗 関連プロジェクト

| プロジェクト | 説明 |
| --- | --- |
| [template-blazor-wasm](https://github.com/usausa/template-blazor-wasm) | Blazor WebAssembly + ASP.NET Core API のテンプレート |
