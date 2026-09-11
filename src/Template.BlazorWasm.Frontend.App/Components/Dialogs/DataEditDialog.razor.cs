namespace Template.BlazorWasm.Frontend.App.Components.Dialogs;

using Microsoft.FluentUI.AspNetCore.Components;

using Template.BlazorWasm.Frontend.App.Models;

public partial class DataEditDialog
{
    [Parameter]
    public DataEditForm Content { get; set; } = default!;

    [CascadingParameter]
    public FluentDialog Dialog { get; set; } = default!;

    private Task OnSaveClickAsync() =>
        String.IsNullOrWhiteSpace(Content.Name) ? Task.CompletedTask : Dialog.CloseAsync(Content);

    private Task OnCancelClickAsync() =>
        Dialog.CancelAsync();
}
