// ReSharper disable StringLiteralTypo
var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Template_BlazorWasm_Backend_Host>("backend")
    .WithHttpHealthCheck("/health");

builder.Build().Run();
