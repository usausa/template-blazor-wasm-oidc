namespace Template.BlazorWasm.Backend.Host.Endpoints;

using Smart.Mapper;

using Template.BlazorWasm.Backend.Host.Application;
using Template.BlazorWasm.Contracts.Data;

public static partial class DataEndpoints
{
    //--------------------------------------------------------------------------------
    // Mapping
    //--------------------------------------------------------------------------------

    public static void MapDataEndpoints(this WebApplication app)
    {
        var group = app.MapApiGroup(ApiRoutes.Data)
            .RequireAuthorization()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/", HandleListAsync)
            .WithName("DataList")
            .Produces<DataListResponse>()
            .ProducesValidationProblem();
        group.MapGet("/{id:long}", HandleGetAsync)
            .WithName("DataGet")
            .Produces<DataGetResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);
        group.MapPost("/", HandleCreateAsync)
            .WithName("DataCreate")
            .Produces<DataCreateResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);
        group.MapPut("/{id:long}", HandleUpdateAsync)
            .WithName("DataUpdate")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
        group.MapDelete("/{id:long}", HandleDeleteAsync)
            .RequireAuthorization(Policies.Administrator)
            .WithName("DataDelete")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    //--------------------------------------------------------------------------------
    // Handler
    //--------------------------------------------------------------------------------

    [Mapper]
    private static partial DataListEntry ToListEntry(DataEntity entity);

    [Mapper]
    private static partial DataGetResponse ToGetResponse(DataEntity entity);

    private static async ValueTask<IResult> HandleListAsync(
        DataService dataService,
        string? name,
        string? sort,
        CancellationToken cancellationToken,
        bool desc = false,
        [Range(0, Int32.MaxValue)] int page = 0,
        [Range(1, 100)] int size = 20)
    {
        var result = await dataService.QueryPageAsync(name, RequestHelper.Parse(sort, DataSort.Id), desc, page, size, cancellationToken);
        return TypedResults.Ok(new DataListResponse(
            result.Total,
            result.Page,
            result.Size,
            result.Items.Select(ToListEntry).ToList()));
    }

    private static async ValueTask<IResult> HandleGetAsync(
        DataService dataService,
        long id)
    {
        var entity = await dataService.QueryAsync(id);
        return entity is not null
            ? TypedResults.Ok(ToGetResponse(entity))
            : TypedResults.NotFound();
    }

    private static async ValueTask<IResult> HandleCreateAsync(
        DataService dataService,
        DataCreateRequest request)
    {
        var entity = new DataEntity { Name = request.Name, Value = request.Value };
        return await dataService.InsertAsync(entity) == DataWriteStatus.Success
            ? TypedResults.Created($"{ApiRoutes.Data}/{entity.Id}", new DataCreateResponse(entity.Id))
            : TypedResults.Problem(statusCode: StatusCodes.Status409Conflict, title: "Duplicate name.");
    }

    private static async ValueTask<IResult> HandleUpdateAsync(
        DataService dataService,
        long id,
        DataUpdateRequest request)
    {
        var result = await dataService.UpdateAsync(id, request.Name, request.Value);
        return result switch
        {
            DataWriteStatus.Success => TypedResults.NoContent(),
            DataWriteStatus.NotFound => TypedResults.NotFound(),
            _ => TypedResults.Problem(statusCode: StatusCodes.Status409Conflict, title: "Duplicate name.")
        };
    }

    private static async ValueTask<IResult> HandleDeleteAsync(
        DataService dataService,
        long id)
    {
        var result = await dataService.DeleteAsync(id);
        return result == DataWriteStatus.Success ? TypedResults.NoContent() : TypedResults.NotFound();
    }
}
