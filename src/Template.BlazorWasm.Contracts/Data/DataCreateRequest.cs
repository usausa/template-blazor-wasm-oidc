namespace Template.BlazorWasm.Contracts.Data;

public sealed record DataCreateRequest(
    [property: Required][property: MaxLength(Length.Name)] string Name,
    [property: Range(0, 999_999_999)] int Value);
