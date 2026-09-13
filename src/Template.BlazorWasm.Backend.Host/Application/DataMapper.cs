namespace Template.BlazorWasm.Backend.Host.Application;

using Smart.Mapper;

using Template.BlazorWasm.Contracts.Data;

internal static partial class DataMapper
{
    [Mapper]
    public static partial DataResponse ToResponse(this DataEntity entity);
}
