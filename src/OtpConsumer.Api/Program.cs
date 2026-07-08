using OtpConsumer.Api.Endpoints;
using OtpService.Grpc;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpcClient<OtpManager.OtpManagerClient>(o =>
{
    o.Address = new Uri(builder.Configuration["OtpService:Address"]
        ?? throw new InvalidOperationException("OtpService:Address must be configured."));
});

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapOtpEndpoints();

app.Run();
