namespace Template.BlazorWasm.Backend.Host.Endpoints;

using Template.BlazorWasm.Contracts.Data;

public static class DataEndpoints
{
    //--------------------------------------------------------------------------------
    // Mapping
    //--------------------------------------------------------------------------------

    public static void MapDataEndpoints(this WebApplication app)
    {
        var group = app.MapGroup(ApiRoutes.Data)
            .RequireAuthorization();

        group.MapGet("/", HandleListAsync)
            .WithName("ListData")
            .Produces<DataListResponse>();
        group.MapGet("/{id:long}", HandleGetAsync)
            .WithName("GetData")
            .Produces<DataResponse>()
            .Produces(StatusCodes.Status404NotFound);
        group.MapPost("/", HandleCreateAsync)
            .WithName("CreateData")
            .Produces<DataCreateResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict);
        group.MapPut("/{id:long}", HandleUpdateAsync)
            .WithName("UpdateData")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
        group.MapDelete("/{id:long}", HandleDeleteAsync)
            .RequireAuthorization(Policies.Administrator)
            .WithName("DeleteData")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    //--------------------------------------------------------------------------------
    // Handler
    //--------------------------------------------------------------------------------

    private static async ValueTask<IResult> HandleListAsync(
        DataUsecase dataUsecase,
        string? name,
        string? sort,
        CancellationToken cancellationToken,
        bool desc = false,
        [Range(0, Int32.MaxValue)] int page = 0,
        [Range(1, 100)] int size = 20)
    {
        var result = await dataUsecase.QueryPageAsync(name, sort, desc, page, size, cancellationToken);
        return TypedResults.Ok(new DataListResponse(
            result.Total,
            result.Page,
            result.Size,
            result.Items.Select(MapToResponse).ToList()));
    }

    private static async ValueTask<IResult> HandleGetAsync(
        DataService dataService,
        long id)
    {
        var entity = await dataService.QueryAsync(id);
        return entity is not null
            ? TypedResults.Ok(MapToResponse(entity))
            : TypedResults.NotFound();
    }

    private static async ValueTask<IResult> HandleCreateAsync(
        DataService dataService,
        DataCreateRequest request)
    {
        var id = await dataService.InsertAsync(request.Name, request.Value);
        return id.HasValue
            ? TypedResults.Created($"{ApiRoutes.Data}/{id.Value}", new DataCreateResponse(id.Value))
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
        var deleted = await dataService.DeleteAsync(id);
        return deleted ? TypedResults.NoContent() : TypedResults.NotFound();
    }

    //--------------------------------------------------------------------------------
    // Mapper
    //--------------------------------------------------------------------------------

    private static DataResponse MapToResponse(DataEntity entity) =>
        new(entity.Id, entity.Name, entity.Value, entity.CreatedAt);
}
