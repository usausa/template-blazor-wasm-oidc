namespace Template.BlazorWasm;

using System.Text.RegularExpressions;

using Microsoft.Playwright;

// ログイン〜CRUD一巡のE2E。Keycloak(http://localhost:8180、README参照)が必要なため、
// 環境変数 E2E_OIDC=1 のときのみ実行する(未設定時はスキップ)
public sealed class DataCrudTests : E2ETestBase
{
    // Keycloakクライアントのredirect URI許可リスト(http://localhost:8080/*)に合わせた固定ポート
    private const int ServerPort = 8080;

    private static void SkipUnlessOidcEnabled() =>
        Assert.SkipUnless(Environment.GetEnvironmentVariable("E2E_OIDC") == "1", "Keycloakが必要なため、E2E_OIDC=1のときのみ実行する");

    private async Task LoginAsync(string address)
    {
        await Page.GotoAsync(address + "/");

        // Keycloakのログイン画面へリダイレクトされるので、初期ユーザーでログインする
        await Expect(Page.Locator("#username")).ToBeVisibleAsync();
        await Page.Locator("#username").FillAsync("admin");
        await Page.Locator("#password").FillAsync("admin");
        await Page.Locator("#kc-login").ClickAsync();

        await Expect(Page).ToHaveTitleAsync(new Regex("ホーム.*"));
    }

    [Fact]
    public async Task LoginShowsHomePage()
    {
        SkipUnlessOidcEnabled();

        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(ServerPort);
        factory.StartServer();

        // When & Then
        await LoginAsync(factory.ServerAddress);
    }

    [Fact]
    public async Task CreateDataShowsInGrid()
    {
        SkipUnlessOidcEnabled();

        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(ServerPort);
        factory.StartServer();

        await LoginAsync(factory.ServerAddress);

        // When
        await Page.GotoAsync(factory.ServerAddress + "/data");
        await Page.Locator("fluent-button", new PageLocatorOptions { HasTextString = "新規作成" }).ClickAsync();
        await Expect(Page.Locator("fluent-dialog fluent-text-field input")).ToBeVisibleAsync();
        await Page.Locator("fluent-dialog fluent-text-field input").FillAsync("E2EItem");
        await Page.Locator("fluent-dialog fluent-number-field input").FillAsync("123");
        await Page.Locator("fluent-dialog fluent-button", new PageLocatorOptions { HasTextString = "保存" }).ClickAsync();

        // Then
        await Expect(Page.GetByText("データを作成しました")).ToBeVisibleAsync();
        await Expect(Page.Locator("table")).ToContainTextAsync("E2EItem");
    }
}
