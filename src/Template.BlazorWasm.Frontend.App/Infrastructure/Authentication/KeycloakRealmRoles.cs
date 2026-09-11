namespace Template.BlazorWasm.Frontend.App.Infrastructure.Authentication;

// Keycloakはレルムロールをrealm_access({"roles":[...]})クレームで運ぶため、ロール名の抽出を共通化する
public static class KeycloakRealmRoles
{
    public static IReadOnlyList<string> Parse(JsonElement realmAccess)
    {
        if ((realmAccess.ValueKind != JsonValueKind.Object) ||
            !realmAccess.TryGetProperty("roles", out var roles) ||
            (roles.ValueKind != JsonValueKind.Array))
        {
            return [];
        }

        return roles.EnumerateArray()
            .Where(static x => x.ValueKind == JsonValueKind.String)
            .Select(static x => x.GetString()!)
            .ToList();
    }
}
