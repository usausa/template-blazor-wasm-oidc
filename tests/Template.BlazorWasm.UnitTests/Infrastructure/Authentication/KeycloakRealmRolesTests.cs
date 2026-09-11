namespace Template.BlazorWasm.Infrastructure.Authentication;

using System.Text.Json;

using Template.BlazorWasm.Frontend.App.Infrastructure.Authentication;

public sealed class KeycloakRealmRolesTests
{
    private static JsonElement Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    [Fact]
    public void ParseRealmAccessReturnsRoles()
    {
        // Arrange
        var element = Parse("""{"roles":["Administrator","User"]}""");

        // Act
        var roles = KeycloakRealmRoles.Parse(element);

        // Assert
        Assert.Equal(["Administrator", "User"], roles);
    }

    [Fact]
    public void ParseEmptyRolesReturnsEmpty()
    {
        // Arrange
        var element = Parse("""{"roles":[]}""");

        // Act
        var roles = KeycloakRealmRoles.Parse(element);

        // Assert
        Assert.Empty(roles);
    }

    [Theory]
    [InlineData("""{}""")]
    [InlineData("""{"roles":"Administrator"}""")]
    [InlineData("""[1,2]""")]
    [InlineData("\"text\"")]
    public void ParseInvalidShapeReturnsEmpty(string json)
    {
        // Arrange
        var element = Parse(json);

        // Act
        var roles = KeycloakRealmRoles.Parse(element);

        // Assert
        Assert.Empty(roles);
    }

    [Fact]
    public void ParseSkipsNonStringRoles()
    {
        // Arrange
        var element = Parse("""{"roles":["Administrator",123,null]}""");

        // Act
        var roles = KeycloakRealmRoles.Parse(element);

        // Assert
        Assert.Equal(["Administrator"], roles);
    }
}
