using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;

//--------------------------------------------------------------------------------
// Configure builder
//--------------------------------------------------------------------------------
var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<Template.BlazorWasm.Frontend.App.Components.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// System
builder.Services.AddSingleton(TimeProvider.System);

// Authentication (OIDC 認可コード+PKCE。IdP設定は wwwroot/appsettings.json)
builder.Services.AddOidcAuthentication(options =>
    {
        builder.Configuration.Bind("Oidc", options.ProviderOptions);
        options.UserOptions.NameClaim = "preferred_username";
        options.UserOptions.RoleClaim = "role";
    })
    .AddAccountClaimsPrincipalFactory<KeycloakClaimsPrincipalFactory>();
builder.Services.AddCascadingAuthenticationState();

// API client (トークンはベースアドレス配下のリクエストにのみ付与)
builder.Services.AddScoped(static p =>
{
    var handler = new AuthorizationMessageHandler(
        p.GetRequiredService<IAccessTokenProvider>(),
        p.GetRequiredService<NavigationManager>());
    return handler.ConfigureHandler(authorizedUrls: [p.GetRequiredService<NavigationManager>().BaseUri]);
});
builder.Services
    .AddHttpClient(ApiClientNames.Default, client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<AuthorizationMessageHandler>();
builder.Services.AddScoped(static p => new ApiClient(p.GetRequiredService<IHttpClientFactory>().CreateClient(ApiClientNames.Default)));

// UI
builder.Services.AddFluentUIComponents();

await builder.Build().RunAsync();
