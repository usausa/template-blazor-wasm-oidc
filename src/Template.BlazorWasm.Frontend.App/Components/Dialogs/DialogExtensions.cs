namespace Template.BlazorWasm.Frontend.App.Components.Dialogs;

using Microsoft.FluentUI.AspNetCore.Components;

public static class DialogExtensions
{
    // The dialog saves and reports the result itself. True unless it was cancelled.
    public static async ValueTask<bool> ShowEditDialogAsync(this IDialogService dialog, string title, DataResponse? entry)
    {
        var parameters = new DialogParameters
        {
            Title = title,
            PreventDismissOnOverlayClick = true
        };
        var reference = entry is null
            ? await dialog.ShowDialogAsync<DataEditDialog>(parameters)
            : await dialog.ShowDialogAsync<DataEditDialog>(entry, parameters);
        var result = await reference.Result;
        return !result.Cancelled;
    }
}
