namespace Template.BlazorWasm.Usecase;

using Template.BlazorWasm.Models;
using Template.BlazorWasm.Models.Entity;
using Template.BlazorWasm.Services;

public sealed class DataUsecase
{
    private readonly DataService dataService;

    public DataUsecase(DataService dataService)
    {
        this.dataService = dataService;
    }

    public async ValueTask<PagedResult<DataEntity>> QueryPageAsync(string? name, string? sort, bool desc, int page, int size, CancellationToken cancellationToken = default)
    {
        var total = await dataService.CountAsync(name, cancellationToken);
        var items = await dataService.QueryPageAsync(name, sort, desc, page * size, size, cancellationToken);
        return new PagedResult<DataEntity>(total, page, size, items);
    }
}
