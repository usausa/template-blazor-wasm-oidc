namespace Template.BlazorWasm;

using System.Text.RegularExpressions;

using Microsoft.Playwright;
using Microsoft.Playwright.Xunit.v3;

// IdPなしで確認できる範囲(認証ガード)のE2E。ログイン以降のフローはOidcLoginTestで扱う
public sealed class AuthGuardTests : PageTest
{
    [Fact]
    public async Task UnauthenticatedIsRedirectedToLoginFlow()
    {
        // Arrange
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        // Act (WASMの初回起動が遅いためタイムアウトを長めにとる)
        await Page.GotoAsync(factory.ServerAddress + "/");

        // Assert
        await Expect(Page).ToHaveURLAsync(new Regex(".*/authentication/login.*"), new PageAssertionsToHaveURLOptions { Timeout = 60_000 });
    }
}
