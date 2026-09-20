namespace Template.BlazorWasm.Frontend.App.Components.Pages;

using Microsoft.FluentUI.AspNetCore.Components;

using Template.BlazorWasm.Frontend.App.Components.Dialogs;
using Template.BlazorWasm.Frontend.App.Models;

public partial class DataPage
{
    private readonly PaginationState pagination = new() { ItemsPerPage = 15 };

    private FluentDataGrid<DataResponse> grid = default!;

    private GridItemsProvider<DataResponse> itemsProvider = default!;

    private string? searchName;

    [Inject]
    public required ApiClient ApiClient { get; set; }

    [Inject]
    public required IDialogService DialogService { get; set; }

    [Inject]
    public required IToastService ToastService { get; set; }

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

    private Task OnCreateClickAsync() =>
        ShowEditDialogAsync("データ作成", new DataEditForm());

    private Task OnEditClickAsync(DataResponse entry) =>
        ShowEditDialogAsync("データ編集", new DataEditForm { Id = entry.Id, Name = entry.Name, Value = entry.Value });

    // The dialog saves and reports the result itself. Refresh unless it was cancelled.
    private async Task ShowEditDialogAsync(string title, DataEditForm form)
    {
        var dialog = await DialogService.ShowDialogAsync<DataEditDialog>(form, new DialogParameters
        {
            Title = title,
            PreventDismissOnOverlayClick = true
        });
        var result = await dialog.Result;
        if (result.Cancelled)
        {
            return;
        }

        await grid.RefreshDataAsync();
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
