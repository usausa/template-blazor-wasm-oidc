namespace Template.BlazorWasm.Backend.Host.Settings;

public sealed class AuthSetting
{
    // IdPのイシュアURL(例: http://localhost:8180/realms/template)。署名鍵はディスカバリで解決される
    [Required]
    public string Authority { get; set; } = default!;

    [Required]
    public string Audience { get; set; } = default!;

    // 開発用IdPをhttpで動かす場合のみfalseにする
    public bool RequireHttpsMetadata { get; set; } = true;
}
