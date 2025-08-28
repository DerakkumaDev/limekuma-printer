using Limekuma.Services;

WebApplicationBuilder builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddGrpc(options => options.EnableDetailedErrors = true);

builder.WebHost.ConfigureKestrel(serverOptions =>
    serverOptions.ListenAnyIP(5000, listenOptions =>
        listenOptions.Protocols =
            Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2));

await using WebApplication app = builder.Build();

app.MapGrpcService<BestsService>();
app.MapGrpcService<ListService>();

await app.RunAsync();