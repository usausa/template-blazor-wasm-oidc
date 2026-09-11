namespace Template.BlazorWasm.Frontend.App.Components.Layout;

public partial class MainLayout
{
    [Inject]
    public required NavigationManager Navigation { get; set; }

    private void OnLogoutClick() =>
        Navigation.NavigateToLogout("authentication/logout");
}
