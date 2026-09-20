namespace Template.BlazorWasm.Services;

using Template.BlazorWasm.Frontend.App.Services;

public sealed class ValidationProblemTests
{
    [Fact]
    public void ParseErrorsReturnsFieldMessages()
    {
        // Arrange (as returned by the API's AddValidation())
        const string response = """
            {"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"name":["The Name field is required."],"value":["The field Value must be between 0 and 999999999."]},"traceId":"00-0-0-00"}
            """;

        // Act
        var errors = ValidationProblem.ParseErrors(response);

        // Assert
        Assert.Equal(2, errors.Count);
        Assert.Equal(["The Name field is required."], errors["name"]);
        Assert.Single(errors["value"]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not json")]
    [InlineData("{\"title\":\"Bad Request\"}")]
    public void ParseErrorsReturnsEmptyForOtherResponses(string? response)
    {
        // Act
        var errors = ValidationProblem.ParseErrors(response);

        // Assert
        Assert.Empty(errors);
    }
}
