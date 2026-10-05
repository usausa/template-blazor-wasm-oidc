namespace Template.BlazorWasm.Contracts.Data;

public sealed record DataListResponse(int Total, int Page, int Size, IReadOnlyList<DataListEntry> Items);
