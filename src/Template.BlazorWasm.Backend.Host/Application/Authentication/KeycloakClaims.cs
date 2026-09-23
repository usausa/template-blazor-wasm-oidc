namespace Template.BlazorWasm.Backend.Host.Application.Authentication;

using System.Text.Json;

// Keycloakのアクセストークンはレルムロールをrealm_access({"roles":[...]})で運ぶため、
// 認可ポリシーが参照するroleクレームへ展開する。IdPを変更する場合はここを差し替える
public static class KeycloakClaims
{
    public static void MapRealmRoles(ClaimsIdentity identity)
    {
        var realmAccess = identity.FindFirst("realm_access");
        if (realmAccess is null)
        {
            return;
        }

        try
        {
            using var document = JsonDocument.Parse(realmAccess.Value);
            if (!document.RootElement.TryGetProperty("roles", out var roles) ||
                (roles.ValueKind != JsonValueKind.Array))
            {
                return;
            }

            foreach (var role in roles.EnumerateArray())
            {
                if (role.ValueKind == JsonValueKind.String)
                {
                    identity.AddClaim(new Claim("role", role.GetString()!));
                }
            }
        }
        catch (JsonException)
        {
            // 不正な形式のクレームはロールなしとして扱う
        }
    }
}
