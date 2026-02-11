using Hocon.Extensions.Configuration;
using Limekuma.Services;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

builder.Configuration.AddHoconFile("appsettings.conf", true, true)
    .AddHoconFile($"appsettings.{builder.Environment.EnvironmentName}.conf", true, true);
builder.Services.AddGrpc(options => options.EnableDetailedErrors = true);

await using WebApplication app = builder.Build();

app.MapGrpcService<BestsService>();
app.MapGrpcService<ListService>();

await app.RunAsync();