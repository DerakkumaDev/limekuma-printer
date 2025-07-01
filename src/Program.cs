using Limekuma.Services;
using Microsoft.AspNetCore.Server.Kestrel.Core;

string socketPath = Path.Combine(Path.GetTempPath(), "limekuma");

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddGrpc(options => options.EnableDetailedErrors = true);
builder.WebHost.ConfigureKestrel(serverOptions =>
    serverOptions.ListenUnixSocket(socketPath, listenOptions =>
        listenOptions.Protocols = HttpProtocols.Http2));

using WebApplication app = builder.Build();

app.MapGrpcService<BestsService>();
app.MapGrpcService<ListService>();

await app.RunAsync();