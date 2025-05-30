WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddControllers();

using WebApplication app = builder.Build();

app.MapControllers();

await app.RunAsync();
