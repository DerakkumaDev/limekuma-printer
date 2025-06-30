using Limekuma.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

using WebApplication app = builder.Build();

app.MapGrpcService<BestsService>();
app.MapGrpcService<ListService>();

await app.RunAsync();