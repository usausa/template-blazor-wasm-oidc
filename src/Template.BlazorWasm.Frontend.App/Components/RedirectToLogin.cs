namespace Template.BlazorWasm.Frontend.App.Components;

// 未認証で[Authorize]ページに来た場合、現在URLを引き継いでログインフローへ送る
public sealed class RedirectToLogin : ComponentBase
{
    [Inject]
    public required NavigationManager Navigation { get; set; }

    protected override void OnInitialized() =>
        Navigation.NavigateToLogin("authentication/login");
}
