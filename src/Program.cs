using Limekuma.Services;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddGrpc(options => options.EnableDetailedErrors = true);

await using WebApplication app = builder.Build();

app.MapGrpcService<BestsService>();
app.MapGrpcService<ListService>();

await app.RunAsync();