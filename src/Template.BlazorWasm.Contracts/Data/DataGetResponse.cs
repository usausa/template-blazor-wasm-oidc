namespace Template.BlazorWasm.Contracts.Data;

public sealed record DataGetResponse(long Id, string Name, int Value, DateTime CreatedAt);
