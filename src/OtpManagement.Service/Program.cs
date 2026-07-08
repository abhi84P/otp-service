using Microsoft.EntityFrameworkCore;
using OtpManagement.Service.Data;
using OtpManagement.Service.Grpc;
using OtpManagement.Service.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();
builder.Services.AddDbContext<OtpDbContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("OtpDb")));
builder.Services.AddOptions<OtpOptions>()
    .BindConfiguration(OtpOptions.SectionName)
    .Validate(o => !string.IsNullOrWhiteSpace(o.HmacSecret), "Otp:HmacSecret must be configured.")
    .ValidateOnStart();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IOtpService, OtpManagement.Service.Services.OtpService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OtpDbContext>();
    db.Database.Migrate();
    db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
}

app.MapGrpcService<OtpGrpcService>();

app.Run();
