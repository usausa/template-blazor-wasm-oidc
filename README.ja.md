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

- Frontend: `Microsoft.AspNetCore.Components.WebAssembly.Authentication` の `AddOidcAuthentication`(認可コード+PKCE)。IdP設定は `wwwroot/appsettings.json`
- Backend: JwtBearer検証をIdPメタデータベース(`Authority` 指定、署名鍵はディスカバリ)へ変更。ロールはKeycloakの `realm_access.roles` を `role` クレームへマッピング
- 自前のJWT発行(`JwtTokenProvider`)・リフレッシュトークン(テーブル/サービス)・Account系(テーブル/サービス/シード/PBKDF2)と `/api/auth/login`・`/api/auth/refresh`・`/api/auth/logout` は撤去(ユーザー管理はIdP側)
- Frontendのログイン画面とトークンの保持・更新(`TokenStore` / `TokenRefreshService` / `JwtAuthorizationMessageHandler` / `JwtAuthenticationStateProvider`)は撤去(`AddOidcAuthentication` に置き換え)

# 🔐 開発用IdP(Keycloak)

Podmanで起動する(初回にレルム `template` を自動インポート)。

```
podman run --rm -p 8180:8080 -e KC_BOOTSTRAP_ADMIN_USERNAME=admin -e KC_BOOTSTRAP_ADMIN_PASSWORD=admin -v .\keycloak:/opt/keycloak/data/import quay.io/keycloak/keycloak:26.4 start-dev --import-realm
```

- レルム: `template` / クライアント: `template-blazor-wasm`(public、PKCE S256、redirect URI `http://localhost:8080/*`)
- 初期ユーザー: `admin` / `admin`(Administratorロール)、`user` / `user`(ロールなし。削除操作は403になる)
- 管理コンソール: http://localhost:8180 (admin / admin)

IdPを変更する場合は、`wwwroot/appsettings.json`(Oidcセクション)、Backendの `Auth` 設定、`index.html` のpreconnect先、ロールクレームのマッピング(`KeycloakClaims` / `KeycloakClaimsPrincipalFactory`)を差し替える。

# 🧪 テスト

- UnitTests / IntegrationTests: IdP不要(統合テストはテスト用署名鍵でトークンを検証)
- E2ETests: 認証ガードのテストはIdP不要。ログイン〜CRUD一巡(`DataCrudTests`)は上記Keycloakを起動し、環境変数 `E2E_OIDC=1` を設定したときのみ実行される

# 🔗 関連プロジェクト

| プロジェクト | 説明 |
| --- | --- |
| [template-blazor-wasm](https://github.com/usausa/template-blazor-wasm) | Blazor WebAssembly + ASP.NET Core API のテンプレート |
