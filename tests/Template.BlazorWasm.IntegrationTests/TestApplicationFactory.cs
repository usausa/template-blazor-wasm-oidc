namespace Template.BlazorWasm;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

public sealed class TestApplicationFactory : WebApplicationFactory<Program>
{
    // IdPディスカバリを行わないテスト用のイシュア/署名鍵(実運用はAuthorityのディスカバリで解決される)
    private const string Issuer = "http://localhost:8180/realms/template";

    private const string Audience = "template-blazor-wasm";

    private static readonly SymmetricSecurityKey SigningKey = new("template-blazor-wasm-oidc-integration-test-signing-key-0123456789"u8.ToArray());

    private readonly string databaseFile = $"test-{Guid.NewGuid():N}.db";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("http_ports", string.Empty);
        builder.UseSetting("ConnectionStrings:Default", $"Data Source={databaseFile};Cache=Shared;Pooling=False");
        builder.UseSetting("Prometheus:Uri", string.Empty);
        builder.UseSetting("Profiler:SqlLog:Enable", "false");
        builder.UseSetting("Profiler:SqlTelemetry:Enable", "false");
        builder.UseSetting("Log:HttpLog", "false");
        builder.UseSetting("Auth:Authority", Issuer);
        builder.UseSetting("Auth:Audience", Audience);
        builder.UseSetting("Auth:RequireHttpsMetadata", "false");

        // IdPメタデータの取得を行わず、テスト用署名鍵でトークンを検証する
        builder.ConfigureTestServices(static services =>
        {
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, static options =>
            {
                options.Configuration = new OpenIdConnectConfiguration { Issuer = Issuer };
                options.TokenValidationParameters.IssuerSigningKey = SigningKey;
            });
        });
    }

    // Keycloakのクレーム設計(preferred_username / realm_access.roles)を模したトークンを発行する
    public static string CreateToken(string name, params string[] roles)
    {
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = Audience,
            Expires = DateTime.UtcNow.AddMinutes(10),
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = Guid.NewGuid().ToString("N"),
                ["preferred_username"] = name,
                ["realm_access"] = new Dictionary<string, object> { ["roles"] = roles }
            },
            SigningCredentials = new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256)
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && File.Exists(databaseFile))
        {
            try
            {
                File.Delete(databaseFile);
            }
            catch (IOException)
            {
                // Ignore
            }
        }
    }
}
