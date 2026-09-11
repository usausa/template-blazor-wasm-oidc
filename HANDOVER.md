# HANDOVER (template-blazor-wasm-oidc)

作成日: 2026-08-23。template-blazor-wasm をベースに、認証を自前JWTからOIDC(認可コード+PKCE)へ置き換えた変種の引継ぎ。

## 1. 状態

- ビルド0警告 / UnitTests 8件・IntegrationTests 7件・E2ETests 1件(+条件付き2件スキップ)全通過
- ReSharper(jb inspectcode 2026.2)対応済み: 残指摘はベースと同じ命名SUGGESTION 3件のみ(Log.cs GC表記・FluentUITestBase)
- 実機確認済み(ログイン→CRUD一巡→ログアウト、リロード後のセッション復元、一般ユーザーの削除403)。ただしIdPはKeycloak互換のスタブで確認(下記5章)
- ソリューション名・プロジェクト名・名前空間はベースのまま(リポジトリ名のみで識別)

## 2. 決定事項

- **IdP: Keycloak採用**(指示の推奨どおり)。開発用レルム定義を `keycloak/realm-template.json` に同梱(realm=template、client=template-blazor-wasm、PKCE S256、audienceマッパー、realm rolesをIDトークン/userinfoにも出すマッパー、admin/admin=Administrator、user/user=ロールなし)。起動手順はREADME参照
- **E2Eの対象範囲**: 既定実行はIdP不要な認証ガード(未認証→/authentication/loginへ誘導)のみ。ログイン〜CRUD一巡(DataCrudTest)はKeycloak起動+環境変数 `E2E_OIDC=1` のときのみ実行(未設定時は `Assert.SkipUnless` でスキップ)。KeycloakのリダイレクトURIはポートワイルドカード不可のため、このE2Eは固定ポート8080で起動する
- **トークンキャッシュの型(セマフォ直列化/期限マージン/stale時1回リトライ/null契約)は適用箇所なし**: 標準 `AuthorizationMessageHandler` がトークンの取得・キャッシュ・期限判定を内包しており、自前のアクセストークン取得部が存在しない。IDトークンをJSから直接読む等の拡張時に ref-aws-s3-wasm の `AwsCredentialsProvider` の型を適用する
- 2FAはIdP側の責務のため本テンプレートでは扱わない

## 3. 主な変更点

- Frontend
  - 撤去: TokenStore / JwtParser / JwtAuthenticationStateProvider / JwtAuthorizationMessageHandler / Login画面 / Contracts.Auth
  - `AddOidcAuthentication`(設定は `wwwroot/appsettings.json` のOidcセクション、NameClaim=preferred_username、RoleClaim=role)+ `KeycloakClaimsPrincipalFactory`(realm_access→role展開、パーサは `KeycloakRealmRoles` に分離しユニットテスト)
  - `Authentication.razor`(RemoteAuthenticatorView、遷移中表示はスプラッシュ同デザイン)/ `RedirectToLogin` は `NavigateToLogin`/ ログアウトは `NavigateToLogout`
  - APIクライアントは標準 `AuthorizationMessageHandler`(authorizedUrls=自ホスト限定)。各ページで `AccessTokenNotAvailableException` をcatchして `Redirect()`(参照実装と同パターン)。FluentDataGridのitemsProviderでは空結果を返してからリダイレクト
  - index.html: IdPホストへのpreconnect、`AuthenticationService.js` 追加
- Backend
  - 撤去: /api/auth/login / TokenService / Account系(Accessor/Service/Entity/IPasswordProvider一式)/ Accountテーブル(Database.sql)/ 初期アカウントシード
  - JwtBearer: `Authority` 指定(署名鍵はディスカバリ)、`RequireHttpsMetadata` は設定化(開発http用)、`OnTokenValidated` で `KeycloakClaims.MapRealmRoles`(realm_access→role)。Administratorポリシー・API 401/403挙動は維持
  - AuthSetting: Authority / Audience / RequireHttpsMetadata に変更
- テスト
  - IntegrationTests: `PostConfigure<JwtBearerOptions>` で `options.Configuration` にダミーメタデータ+テスト署名鍵を注入し、ディスカバリなしで検証。Keycloak形クレーム(preferred_username / realm_access)のトークンを発行して、認証必須・ロールマッピング・Administrator限定削除(403/204)を検証
  - UnitTests: JwtParserTest / DefaultPasswordProviderTest を撤去、KeycloakRealmRolesTest を追加

## 4. 知見(ベースにも影響するものを含む)

1. **ビルド時OpenAPI生成(Microsoft.Extensions.ApiDescription.Server)はこの構成では動かない**(ベース共通)。(a) 起動時にPrometheus HttpListenerが起動を試みて失敗する、(b) Program.cs の `Directory.SetCurrentDirectory` によりツールのfile-listキャッシュの相対パスが壊れる、(c) 出力が OpenAPI 3.1 形式・別ファイル名で、3.0前提のnswagと合わない。**openapi/v1.json の更新は、アプリを起動して `/openapi/v1.json` を取得して置き換える**(今回この方法で更新済み。`Prometheus__Uri=` を空にして起動すると副作用がない)
2. oidc-client-ts(Blazor WASM認証の内部実装)は **post_logout_redirect_uri への戻りに `state` パラメータの返却が必須**。IdPがstateを返さないと "Completing logout" で停止する(Keycloakは返す。スタブ作成時に判明)
3. Blazor WASMのOIDCは既定で **userinfoエンドポイントを呼ぶ**(loadUserInfo有効)。ロールをUIで使うにはuserinfo(またはIDトークン)に realm_access が必要 → レルム定義のrealm rolesマッパーで id_token/userinfo にも出している
4. IdP再起動などで署名鍵が変わると、Backendは旧JWKSキャッシュにより一時的に401を返す(ConfigurationManagerのリフレッシュで回復。開発中に鍵が変わったらBackendを再起動するのが早い)
5. E2Eの `Assert.SkipUnless`(xunit.v3)で環境依存テストを条件付き実行にできる(MTPの結果表示は skipped と出る)

## 5. 実機確認の方法と残作業

- 本環境にはPodman/Docker/Javaがなく実Keycloakを起動できないため、**Keycloak互換パス・クレームのスタブIdP**(RS256+ディスカバリ+PKCE+userinfo+logout)をスクラッチ領域に作成して実ブラウザで確認した(リポジトリには含めない)
  - 確認済み: 認可コード+PKCEのリダイレクト一巡 / トークン検証(ディスカバリ+JWKS)/ admin でCRUD一巡 / リロード後のセッション復元(Authorizing表示)/ ログアウト(セッション破棄→再ログイン要求)/ user での削除403(realm_accessマッピング)
- **残作業**: 実Keycloak(README手順)での確認、および `E2E_OIDC=1` でのDataCrudTest実行。Podmanのある環境で `podman run ... --import-realm` → `E2E_OIDC=1` を設定してE2E実行
