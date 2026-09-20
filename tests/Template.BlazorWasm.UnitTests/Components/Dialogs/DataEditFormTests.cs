namespace Template.BlazorWasm.Components.Dialogs;

using System.ComponentModel.DataAnnotations;

using Template.BlazorWasm.Domain;
using Template.BlazorWasm.Frontend.App.Components.Dialogs;

public sealed class DataEditFormTests
{
    [Fact]
    public void ValidateValidFormReturnsValid()
    {
        // Arrange
        var form = new DataEditDialog.DataEditForm { Name = "Data-1", Value = 100 };

        // Act
        var results = Validate(form);

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void ValidateEmptyNameReturnsInvalid()
    {
        // Arrange
        var form = new DataEditDialog.DataEditForm { Name = string.Empty, Value = 100 };

        // Act
        var results = Validate(form);

        // Assert
        Assert.Contains(results, static x => x.MemberNames.Contains(nameof(DataEditDialog.DataEditForm.Name)));
    }

    [Fact]
    public void ValidateTooLongNameReturnsInvalid()
    {
        // Arrange
        var form = new DataEditDialog.DataEditForm { Name = new string('a', Length.Name + 1), Value = 100 };

        // Act
        var results = Validate(form);

        // Assert
        Assert.Contains(results, static x => x.MemberNames.Contains(nameof(DataEditDialog.DataEditForm.Name)));
    }

    [Fact]
    public void ValidateOutOfRangeValueReturnsInvalid()
    {
        // Arrange
        var form = new DataEditDialog.DataEditForm { Name = "Data-1", Value = -1 };

        // Act
        var results = Validate(form);

        // Assert
        Assert.Contains(results, static x => x.MemberNames.Contains(nameof(DataEditDialog.DataEditForm.Value)));
    }

    private static List<ValidationResult> Validate(DataEditDialog.DataEditForm form)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(form, new ValidationContext(form), results, validateAllProperties: true);
        return results;
    }
}
