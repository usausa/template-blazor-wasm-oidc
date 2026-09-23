namespace Template.BlazorWasm.Backend.Host.Application;

using Template.BlazorWasm.Backend.Host.Application.Context;
using Template.BlazorWasm.Backend.Host.Application.Telemetry;

public static class EndpointExtensions
{
    public static RouteGroupBuilder MapApiGroup(this IEndpointRouteBuilder endpoints, string prefix) =>
        endpoints.MapGroup(prefix)
            .AddEndpointFilter<RequestMetricsEndpointFilter>()
            .AddEndpointFilter<ServiceContextEndpointFilter>();
}
