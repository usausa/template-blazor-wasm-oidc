namespace Template.BlazorWasm.Frontend.App.Components.Pages;

using Microsoft.FluentUI.AspNetCore.Components;

using Template.BlazorWasm.Frontend.App.Components.Dialogs;

public partial class DataPage
{
    private readonly PaginationState pagination = new() { ItemsPerPage = 15 };

#pragma warning disable CA2213
    private FluentDataGrid<DataListEntry> grid = default!;
#pragma warning restore CA2213

    private GridItemsProvider<DataListEntry> itemsProvider = default!;

    private string? searchName;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    [Inject]
    public required ApiClient ApiClient { get; set; }

    [Inject]
    public required IDialogService DialogService { get; set; }

    [Inject]
    public required INotificationService NotificationService { get; set; }

    //--------------------------------------------------------------------------------
    // Initialize
    //--------------------------------------------------------------------------------

    protected override void OnInitialized()
    {
        itemsProvider = async request =>
        {
            try
            {
                var page = request.StartIndex / pagination.ItemsPerPage;

                // 並べ替えはサーバー側で行うため、グリッドが選んだ列と昇降をAPIへ渡す
                var sorted = request.GetSortByProperties().FirstOrDefault();
                var result = await ApiClient.DataListAsync(
                    searchName,
                    sorted.PropertyName,
                    sorted.Direction == DataGridSortDirection.Descending,
                    page,
                    pagination.ItemsPerPage,
                    request.CancellationToken);
                return GridItemsProviderResult.From(result.Items.ToList(), result.Total);
            }
            catch (AccessTokenNotAvailableException ex)
            {
                // セッション失効時は対話ログインへ誘導する
                ex.Redirect();
                return GridItemsProviderResult.From(new List<DataListEntry>(), 0);
            }
        };
    }

    //--------------------------------------------------------------------------------
    // Search
    //--------------------------------------------------------------------------------

    private async Task OnSearchClickAsync()
    {
        await pagination.SetCurrentPageIndexAsync(0);
        await grid.RefreshDataAsync();
    }

    //--------------------------------------------------------------------------------
    // Create / Edit
    //--------------------------------------------------------------------------------

    private async Task OnCreateClickAsync()
    {
        if (await DialogService.ShowEditDialogAsync("データ作成", null))
        {
            await grid.RefreshDataAsync();
        }
    }

    private async Task OnEditClickAsync(DataListEntry entry)
    {
        if (await DialogService.ShowEditDialogAsync("データ編集", entry))
        {
            await grid.RefreshDataAsync();
        }
    }

    //--------------------------------------------------------------------------------
    // Delete
    //--------------------------------------------------------------------------------

    private async Task OnDeleteClickAsync(DataListEntry entry)
    {
        var result = await DialogService.ShowConfirmationAsync($"{entry.Name} を削除します。よろしいですか?", title: "削除確認", primaryButton: "削除", secondaryButton: "キャンセル");
        if (result.Cancelled)
        {
            return;
        }

        try
        {
            await ApiClient.DataDeleteAsync(entry.Id);

            await NotificationService.ShowSuccessToastAsync("データを削除しました");
            await grid.RefreshDataAsync();
        }
        catch (ApiException ex) when (ex.StatusCode == 403)
        {
            await NotificationService.ShowErrorToastAsync("削除の権限がありません");
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            await NotificationService.ShowErrorToastAsync("対象が存在しません");
            await grid.RefreshDataAsync();
        }
        catch (AccessTokenNotAvailableException ex)
        {
            ex.Redirect();
        }
    }
}
