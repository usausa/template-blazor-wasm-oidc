namespace Template.BlazorWasm.Frontend.App.Components.Dialogs;

using Microsoft.FluentUI.AspNetCore.Components;

public static class DataDialogExtensions
{
    // The dialog saves and reports the result itself. True unless it was cancelled.
    public static async ValueTask<bool> ShowEditDialogAsync(this IDialogService dialog, string title, DataListEntry? entry)
    {
        var result = await dialog.ShowDialogAsync<DataEditDialog>(options =>
        {
            options.Header.Title = title;
            options.Header.CloseAction.Visible = true;
            options.Parameters.Add(nameof(DataEditDialog.Content), entry);
        });
        return !result.Cancelled;
    }
}
