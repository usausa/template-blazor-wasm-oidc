namespace Template.BlazorWasm;

using System.Text.RegularExpressions;

// IdPなしで確認できる範囲(認証ガード)のE2E。ログイン以降のフローはOidcLoginTestで扱う
public sealed class AuthGuardTests : E2ETestBase
{
    [Fact]
    public async Task UnauthenticatedIsRedirectedToLoginFlow()
    {
        // Given
        await using var factory = new E2EApplicationFactory();
        factory.UseKestrel(0);
        factory.StartServer();

        // When
        await Page.GotoAsync(factory.ServerAddress + "/");

        // Then
        await Expect(Page).ToHaveURLAsync(new Regex(".*/authentication/login.*"));
    }
}
