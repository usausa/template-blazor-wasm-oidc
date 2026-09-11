namespace Template.BlazorWasm.Components.Layout;

using Template.BlazorWasm.Frontend.App.Components.Layout;

public sealed class NavMenuTests : FluentUITestBase
{
    [Fact]
    public void RenderShowsNavigationLinks()
    {
        // Arrange & Act
        var cut = Render<NavMenu>();

        // Assert
        Assert.Contains("ホーム", cut.Markup, StringComparison.Ordinal);
        Assert.Contains("データ", cut.Markup, StringComparison.Ordinal);
    }
}
