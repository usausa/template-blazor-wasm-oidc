namespace Template.BlazorWasm;

using System.Net.Http.Headers;

using Template.BlazorWasm.Contracts.Data;

public sealed class AuthTests : IClassFixture<TestApplicationFactory>
{
    // CA1861: Assertの比較対象は毎回同じ配列のため、呼び出しごとに生成しない
    private static readonly string[] SortedByName = ["SortItemA", "SortItemB", "SortItemC"];

    private static readonly int[] SortedByValueDescending = [30, 20, 10];

    private static readonly string[] InsertionOrder = ["SortItemB", "SortItemA", "SortItemC"];

    private readonly TestApplicationFactory factory;

    public AuthTests(TestApplicationFactory factory)
    {
        this.factory = factory;
    }

    private HttpClient CreateClientWithToken(string name, params string[] roles)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TestApplicationFactory.CreateToken(name, roles));
        return client;
    }

    [Fact]
    public async Task InvalidTokenReturnsUnauthorized()
    {
        // Arrange
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");

        // Act
        var response = await client.GetAsync(new Uri("/api/data", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DataApiWorksWithToken()
    {
        // Arrange
        var client = CreateClientWithToken("admin", "Administrator");

        // Act
        var create = await client.PostAsJsonAsync(new Uri("/api/data", UriKind.Relative), new DataCreateRequest("IntegrationItem", 100), TestContext.Current.CancellationToken);
        var list = await client.GetFromJsonAsync<DataListResponse>(new Uri("/api/data?name=IntegrationItem", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        Assert.NotNull(list);
        Assert.Single(list.Items);
        Assert.Equal("IntegrationItem", list.Items[0].Name);
    }

    [Fact]
    public async Task DeleteWithoutAdministratorRoleReturnsForbidden()
    {
        // Arrange
        var client = CreateClientWithToken("user");

        // Act
        var response = await client.DeleteAsync(new Uri("/api/data/1", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteWithAdministratorRoleWorks()
    {
        // Arrange
        var client = CreateClientWithToken("admin", "Administrator");
        var create = await client.PostAsJsonAsync(new Uri("/api/data", UriKind.Relative), new DataCreateRequest("DeleteItem", 200), TestContext.Current.CancellationToken);
        var created = await create.Content.ReadFromJsonAsync<DataCreateResponse>(TestContext.Current.CancellationToken);

        // Act
        var response = await client.DeleteAsync(new Uri($"/api/data/{created!.Id}", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
    [Fact]
    public async Task DataApiSortsByRequestedColumn()
    {
        // Arrange
        var client = CreateClientWithToken("admin", "Administrator");

        // 登録順とName順・Value順がいずれも異なるように積む
        await client.PostAsJsonAsync(new Uri("/api/data", UriKind.Relative), new DataCreateRequest("SortItemB", 20), TestContext.Current.CancellationToken);
        await client.PostAsJsonAsync(new Uri("/api/data", UriKind.Relative), new DataCreateRequest("SortItemA", 30), TestContext.Current.CancellationToken);
        await client.PostAsJsonAsync(new Uri("/api/data", UriKind.Relative), new DataCreateRequest("SortItemC", 10), TestContext.Current.CancellationToken);

        // Act
        var byName = await client.GetFromJsonAsync<DataListResponse>(new Uri("/api/data?name=SortItem&sort=Name", UriKind.Relative), TestContext.Current.CancellationToken);
        var byValueDesc = await client.GetFromJsonAsync<DataListResponse>(new Uri("/api/data?name=SortItem&sort=Value&desc=true", UriKind.Relative), TestContext.Current.CancellationToken);
        var unknownKey = await client.GetFromJsonAsync<DataListResponse>(new Uri("/api/data?name=SortItem&sort=Unknown", UriKind.Relative), TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(byName);
        Assert.Equal(SortedByName, byName.Items.Select(static x => x.Name));
        Assert.NotNull(byValueDesc);
        Assert.Equal(SortedByValueDescending, byValueDesc.Items.Select(static x => x.Value));

        // 未知のキーはSQLのelse(Id順=登録順)へ落ちる
        Assert.NotNull(unknownKey);
        Assert.Equal(InsertionOrder, unknownKey.Items.Select(static x => x.Name));
    }
}
