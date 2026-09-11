# Template project for Blazor WebAssembly (OIDC)

template-blazor-wasm をベースに、認証を自前JWTからOIDC(認可コード+PKCE)へ置き換えた変種。

## ベースとの差分

- Frontend: `Microsoft.AspNetCore.Components.WebAssembly.Authentication` の `AddOidcAuthentication`(認可コード+PKCE)。IdP設定は `wwwroot/appsettings.json`
- Backend: JwtBearer検証をIdPメタデータベース(`Authority` 指定、署名鍵はディスカバリ)へ変更。ロールはKeycloakの `realm_access.roles` を `role` クレームへマッピング
- Account系(テーブル/サービス/シード)と `/api/auth/login` は撤去(ユーザー管理はIdP側)

## 開発用IdP(Keycloak)

Podmanで起動する(初回にレルム `template` を自動インポート)。

```
podman run --rm -p 8180:8080 -e KC_BOOTSTRAP_ADMIN_USERNAME=admin -e KC_BOOTSTRAP_ADMIN_PASSWORD=admin -v .\keycloak:/opt/keycloak/data/import quay.io/keycloak/keycloak:26.4 start-dev --import-realm
```

- レルム: `template` / クライアント: `template-blazor-wasm`(public、PKCE S256、redirect URI `http://localhost:8080/*`)
- 初期ユーザー: `admin` / `admin`(Administratorロール)、`user` / `user`(ロールなし。削除操作は403になる)
- 管理コンソール: http://localhost:8180 (admin / admin)

IdPを変更する場合は、`wwwroot/appsettings.json`(Oidcセクション)、Backendの `Auth` 設定、`index.html` のpreconnect先、ロールクレームのマッピング(`KeycloakClaims` / `KeycloakClaimsPrincipalFactory`)を差し替える。

## テスト

- UnitTests / IntegrationTests: IdP不要(統合テストはテスト用署名鍵でトークンを検証)
- E2ETests: 認証ガードのテストはIdP不要。ログイン〜CRUD一巡(`DataCrudTest`)は上記Keycloakを起動し、環境変数 `E2E_OIDC=1` を設定したときのみ実行される
