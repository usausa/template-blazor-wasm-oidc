namespace Template.BlazorWasm.Frontend.App.Models;

using System.ComponentModel.DataAnnotations;

// Same rules as the Backend contracts. The dialog validates these before calling the API.
public sealed class DataEditForm
{
    // null while creating
    public long? Id { get; init; }

    [Required(ErrorMessage = "名前を入力してください。")]
    [MaxLength(Length.Name, ErrorMessage = "名前は{1}文字以内で入力してください。")]
    public string Name { get; set; } = string.Empty;

    [Range(0, 999_999_999, ErrorMessage = "値は{1}から{2}の範囲で入力してください。")]
    public int Value { get; set; }
}
