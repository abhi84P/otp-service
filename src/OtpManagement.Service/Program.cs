var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

var app = builder.Build();

app.MapGet("/", () => "OtpManagement.Service — gRPC only. Use a gRPC client against the OtpManager service.");

app.Run();
