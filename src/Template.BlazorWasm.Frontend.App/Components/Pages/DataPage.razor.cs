namespace Template.BlazorWasm.Frontend.App.Components.Pages;

using Microsoft.FluentUI.AspNetCore.Components;

using Template.BlazorWasm.Frontend.App.Components.Dialogs;

public partial class DataPage
{
    private readonly PaginationState pagination = new() { ItemsPerPage = 15 };

#pragma warning disable CA2213
    private FluentDataGrid<DataResponse> grid = default!;
#pragma warning restore CA2213

    private GridItemsProvider<DataResponse> itemsProvider = default!;

    private string? searchName;

    //--------------------------------------------------------------------------------
    // Property
    //--------------------------------------------------------------------------------

    [Inject]
    public required ApiClient ApiClient { get; set; }

    [Inject]
    public required IDialogService DialogService { get; set; }

    [Inject]
    public required IToastService ToastService { get; set; }

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
                var result = await ApiClient.ListDataAsync(
                    searchName,
                    sorted.PropertyName,
                    sorted.Direction == SortDirection.Descending,
                    page,
                    pagination.ItemsPerPage,
                    request.CancellationToken);
                return GridItemsProviderResult.From(result.Items.ToList(), result.Total);
            }
            catch (AccessTokenNotAvailableException ex)
            {
                // セッション失効時は対話ログインへ誘導する
                ex.Redirect();
                return GridItemsProviderResult.From(new List<DataResponse>(), 0);
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

    private async Task OnEditClickAsync(DataResponse entry)
    {
        if (await DialogService.ShowEditDialogAsync("データ編集", entry))
        {
            await grid.RefreshDataAsync();
        }
    }

    //--------------------------------------------------------------------------------
    // Delete
    //--------------------------------------------------------------------------------

    private async Task OnDeleteClickAsync(DataResponse entry)
    {
        var dialog = await DialogService.ShowConfirmationAsync($"{entry.Name} を削除します。よろしいですか?", "削除", "キャンセル", "削除確認");
        var result = await dialog.Result;
        if (result.Cancelled)
        {
            return;
        }

        try
        {
            await ApiClient.DeleteDataAsync(entry.Id);

            ToastService.ShowSuccess("データを削除しました");
            await grid.RefreshDataAsync();
        }
        catch (ApiException ex) when (ex.StatusCode == 403)
        {
            ToastService.ShowError("削除の権限がありません");
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            ToastService.ShowError("対象が存在しません");
            await grid.RefreshDataAsync();
        }
        catch (AccessTokenNotAvailableException ex)
        {
            ex.Redirect();
        }
    }
}
