namespace Template.BlazorWasm.Frontend.App.Infrastructure.Authentication;

using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;

// Keycloakのrealm_accessクレームをroleクレームへ展開する(UserOptions.RoleClaim="role"で参照される)
public sealed class KeycloakClaimsPrincipalFactory : AccountClaimsPrincipalFactory<RemoteUserAccount>
{
    public KeycloakClaimsPrincipalFactory(IAccessTokenProviderAccessor accessor)
        : base(accessor)
    {
    }

    // accountは基底の注釈に反して未認証時にnullが渡されるため、null許容で受ける
    public override async ValueTask<ClaimsPrincipal> CreateUserAsync(RemoteUserAccount? account, RemoteAuthenticationUserOptions options)
    {
        var user = await base.CreateUserAsync(account!, options);

        if ((user.Identity is ClaimsIdentity identity) &&
            (account is not null) &&
            account.AdditionalProperties.TryGetValue("realm_access", out var value) &&
            (value is JsonElement element))
        {
            foreach (var role in KeycloakRealmRoles.Parse(element))
            {
                identity.AddClaim(new Claim(options.RoleClaim ?? "role", role));
            }
        }

        return user;
    }
}
