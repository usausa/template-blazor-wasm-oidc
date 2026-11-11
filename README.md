# Template project for Blazor WebAssembly (OIDC)

English | [日本語](README.ja.md)

[Template](https://usausa.github.io/template/) project for Blazor WebAssembly (OIDC).

# ✅ Implement

| Category | Feature |
| --- | --- |
| Auth | OIDC (authorization code + PKCE) / Login and logout redirects (RemoteAuthenticatorView) / JWT Bearer (IdP discovery) / Role claim mapping (Keycloak `realm_access`) / Role policy |
| IdP | IdP for development (Keycloak, realm import) |
| Test | Token validation without the IdP (test signing key) / Auth guard E2E / Login to CRUD E2E (Keycloak, opt-in) |

# 📚 Docs

The documents in `docs` are in Japanese.

| Document | Contents |
| --- | --- |
| [oidc.md](docs/oidc.md) | Differences from the base and the IdP for development (Keycloak) |
| [tests.md](docs/tests.md) | Tests and E2E tests (Playwright) |

# 🔗 Related Projects

| Project | Description |
| --- | --- |
| [template-blazor-wasm](https://github.com/usausa/template-blazor-wasm) | Blazor WebAssembly + ASP.NET Core API template |
