namespace Template.BlazorWasm.Frontend.App.Services;

// NSwag生成クライアント(ApiClient.g.cs)のシリアライザ設定をサーバのJSON規約に合わせる
public partial class ApiClient
{
    static partial void UpdateJsonSerializerSettings(JsonSerializerOptions settings)
    {
        settings.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        settings.PropertyNameCaseInsensitive = true;
    }
}
