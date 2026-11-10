# Template project for Blazor WebAssembly (OIDC)

English | [日本語](README.ja.md)

A variant based on template-blazor-wasm that replaces the custom JWT authentication with OIDC (authorization code + PKCE).

## Differences from the base

- Frontend: `AddOidcAuthentication` of `Microsoft.AspNetCore.Components.WebAssembly.Authentication` (authorization code + PKCE). The IdP settings are in `wwwroot/appsettings.json`
- Backend: The JwtBearer validation is changed to be based on the IdP metadata (`Authority` specified, signing keys from discovery). Roles are mapped from `realm_access.roles` of Keycloak to the `role` claim
- The Account parts (tables/services/seed) and `/api/auth/login` were removed (users are managed on the IdP side)

## IdP for development (Keycloak)

Start it with Podman (the realm `template` is imported automatically on the first start).

```
podman run --rm -p 8180:8080 -e KC_BOOTSTRAP_ADMIN_USERNAME=admin -e KC_BOOTSTRAP_ADMIN_PASSWORD=admin -v .\keycloak:/opt/keycloak/data/import quay.io/keycloak/keycloak:26.4 start-dev --import-realm
```

- Realm: `template` / client: `template-blazor-wasm` (public, PKCE S256, redirect URI `http://localhost:8080/*`)
- Initial users: `admin` / `admin` (Administrator role), `user` / `user` (no role. Delete operations return 403)
- Admin console: http://localhost:8180 (admin / admin)

To change the IdP, replace `wwwroot/appsettings.json` (the Oidc section), the `Auth` settings of the Backend, the preconnect target in `index.html`, and the mapping of the role claims (`KeycloakClaims` / `KeycloakClaimsPrincipalFactory`).

## Tests

- UnitTests / IntegrationTests: No IdP needed (the integration tests validate the tokens with a test signing key)
- E2ETests: The authentication guard tests need no IdP. The round trip from login through CRUD (`DataCrudTest`) runs only when the Keycloak above is started and the environment variable `E2E_OIDC=1` is set

The E2E tests (`tests/Template.BlazorWasm.E2ETests`) run with Playwright. They are not part of the daily runs, and are run when needed and before releases. Run the commands in PowerShell at the repository root.

```powershell
# Get the browsers (first time. Uses the script output by the build)
dotnet build tests/Template.BlazorWasm.E2ETests
pwsh tests/Template.BlazorWasm.E2ETests/bin/Debug/net10.0/playwright.ps1 install

# Run (Chromium and headless by default)
dotnet run --project tests/Template.BlazorWasm.E2ETests

# Run with the browser shown
$env:HEADED = "1"; dotnet run --project tests/Template.BlazorWasm.E2ETests; Remove-Item Env:HEADED

# Switch the browser (chromium / firefox / webkit)
$env:BROWSER = "firefox"; dotnet run --project tests/Template.BlazorWasm.E2ETests; Remove-Item Env:BROWSER

# Run step by step with Playwright Inspector
$env:PWDEBUG = "1"; dotnet run --project tests/Template.BlazorWasm.E2ETests; Remove-Item Env:PWDEBUG
```

- Only for failed tests, a trace that records the screen, DOM, network, and console of each action is saved to `tests/Template.BlazorWasm.E2ETests/bin/Debug/net10.0/playwright-traces/<test display name>.zip`. Open it with `pwsh tests/Template.BlazorWasm.E2ETests/bin/Debug/net10.0/playwright.ps1 show-trace <zip>` or https://trace.playwright.dev
