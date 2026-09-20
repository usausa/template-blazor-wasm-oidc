namespace Template.BlazorWasm.Frontend.App.Components.Dialogs;

using System.ComponentModel.DataAnnotations;
using System.Reflection;

using Microsoft.AspNetCore.Components.Forms;
using Microsoft.FluentUI.AspNetCore.Components;

// Saves inside the dialog so that errors from the API (validation, duplicate name) are shown on the
// fields and the user can correct the input without reopening the dialog.
public partial class DataEditDialog
{
    private EditContext editContext = default!;

    private ValidationMessageStore messageStore = default!;

    private FieldIdentifier modelField;

    private DataEditForm model = default!;

    private bool saving;

    [Parameter]
    public DataResponse? Content { get; set; }

    [CascadingParameter]
    public FluentDialog Dialog { get; set; } = default!;

    [Inject]
    public required ApiClient ApiClient { get; set; }

    [Inject]
    public required IToastService ToastService { get; set; }

    protected override void OnInitialized()
    {
        model = Content is null ? new DataEditForm() : new DataEditForm { Name = Content.Name, Value = Content.Value };
        editContext = new EditContext(model);
        messageStore = new ValidationMessageStore(editContext);
        modelField = new FieldIdentifier(model, string.Empty);
    }

    private async Task OnSaveClickAsync()
    {
        messageStore.Clear();
        if (!editContext.Validate())
        {
            return;
        }

        saving = true;
        try
        {
            if (Content is null)
            {
                await ApiClient.CreateDataAsync(new DataCreateRequest(model.Name, model.Value));
                ToastService.ShowSuccess("データを作成しました");
            }
            else
            {
                await ApiClient.UpdateDataAsync(Content.Id, new DataUpdateRequest(model.Name, model.Value));
                ToastService.ShowSuccess("データを更新しました");
            }

            await Dialog.CloseAsync();
        }
        catch (ApiException ex) when (ex.StatusCode == 400)
        {
            AddServerErrors(ValidationProblem.ParseErrors(ex.Response));
        }
        catch (ApiException ex) when (ex.StatusCode == 409)
        {
            messageStore.Add(() => model.Name, "同じ名前のデータが存在します。");
            editContext.NotifyValidationStateChanged();
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            // The entry was deleted meanwhile. Close so that the caller refreshes the list.
            ToastService.ShowError("対象が存在しません");
            await Dialog.CloseAsync();
        }
        catch (AccessTokenNotAvailableException ex)
        {
            ex.Redirect();
        }
        finally
        {
            saving = false;
        }
    }

    private Task OnCancelClickAsync() =>
        Dialog.CancelAsync();

    // Field names come back in camelCase; unknown ones are shown at the model level
    private void AddServerErrors(IReadOnlyDictionary<string, string[]> errors)
    {
        foreach (var (field, messages) in errors)
        {
            var property = typeof(DataEditForm).GetProperty(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
            var identifier = property is null ? modelField : new FieldIdentifier(model, property.Name);
            messageStore.Add(identifier, messages);
        }

        editContext.NotifyValidationStateChanged();
    }

    //--------------------------------------------------------------------------------
    // Form
    //--------------------------------------------------------------------------------

    // Same rules as the Backend contracts. The dialog validates these before calling the API.
    internal sealed class DataEditForm
    {
        [Required(ErrorMessage = "名前を入力してください。")]
        [MaxLength(Length.Name, ErrorMessage = "名前は{1}文字以内で入力してください。")]
        public string Name { get; set; } = string.Empty;

        [Range(0, 999_999_999, ErrorMessage = "値は{1}から{2}の範囲で入力してください。")]
        public int Value { get; set; }
    }
}
