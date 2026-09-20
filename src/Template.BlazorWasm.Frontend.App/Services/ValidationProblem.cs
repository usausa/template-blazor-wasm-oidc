namespace Template.BlazorWasm.Frontend.App.Services;

public static class ValidationProblem
{
    private static readonly Dictionary<string, string[]> Empty = [];

    // The errors part of a validation ProblemDetails (400 from the API's AddValidation()).
    // Keys are the request property names in camelCase.
    public static IReadOnlyDictionary<string, string[]> ParseErrors(string? response)
    {
        if (String.IsNullOrEmpty(response))
        {
            return Empty;
        }

        try
        {
            using var document = JsonDocument.Parse(response);
            return document.RootElement.TryGetProperty("errors", out var errors)
                ? errors.Deserialize<Dictionary<string, string[]>>() ?? Empty
                : Empty;
        }
        catch (JsonException)
        {
            return Empty;
        }
    }
}
