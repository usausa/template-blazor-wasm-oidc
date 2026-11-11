# Template project for Blazor WebAssembly (OIDC)

English | [日本語](README.ja.md)

[Template](https://usausa.github.io/template/) project for Blazor WebAssembly (OIDC).

# ✅ Implement

| Category | Feature |
| --- | --- |
| Auth | OIDC (authorization code + PKCE) / Login and logout redirects (RemoteAuthenticatorView) / JWT Bearer (IdP discovery) / Role claim mapping (Keycloak `realm_access`) / Role policy |
| IdP | IdP for development (Keycloak, realm import) |
| Test | Token validation without the IdP (test signing key) / Auth guard E2E / Login to CRUD E2E (Keycloak, opt-in) |

# 🔀 Differences from the base

- Frontend: `AddOidcAuthentication` of `Microsoft.AspNetCore.Components.WebAssembly.Authentication` (authorization code + PKCE). The IdP settings are in `wwwroot/appsettings.json`
- Backend: The JwtBearer validation is changed to be based on the IdP metadata (`Authority` specified, signing keys from discovery). Roles are mapped from `realm_access.roles` of Keycloak to the `role` claim
- The self-issued JWT (`JwtTokenProvider`), the refresh tokens (tables/services), the Account parts (tables/services/seed/PBKDF2), and `/api/auth/login`, `/api/auth/refresh` and `/api/auth/logout` were removed (users are managed on the IdP side)
- The login screen of the Frontend and the token storage and refresh (`TokenStore` / `TokenRefreshService` / `JwtAuthorizationMessageHandler` / `JwtAuthenticationStateProvider`) were removed (replaced by `AddOidcAuthentication`)

# 🔐 IdP for development (Keycloak)

Start it with Podman (the realm `template` is imported automatically on the first start).

```
podman run --rm -p 8180:8080 -e KC_BOOTSTRAP_ADMIN_USERNAME=admin -e KC_BOOTSTRAP_ADMIN_PASSWORD=admin -v .\keycloak:/opt/keycloak/data/import quay.io/keycloak/keycloak:26.4 start-dev --import-realm
```

- Realm: `template` / client: `template-blazor-wasm` (public, PKCE S256, redirect URI `http://localhost:8080/*`)
- Initial users: `admin` / `admin` (Administrator role), `user` / `user` (no role. Delete operations return 403)
- Admin console: http://localhost:8180 (admin / admin)

To change the IdP, replace `wwwroot/appsettings.json` (the Oidc section), the `Auth` settings of the Backend, the preconnect target in `index.html`, and the mapping of the role claims (`KeycloakClaims` / `KeycloakClaimsPrincipalFactory`).

# 🧪 Tests

- UnitTests / IntegrationTests: No IdP needed (the integration tests validate the tokens with a test signing key)
- E2ETests: The authentication guard tests need no IdP. The round trip from login through CRUD (`DataCrudTests`) runs only when the Keycloak above is started and the environment variable `E2E_OIDC=1` is set

# 🔗 Related Projects

| Project | Description |
| --- | --- |
| [template-blazor-wasm](https://github.com/usausa/template-blazor-wasm) | Blazor WebAssembly + ASP.NET Core API template |
