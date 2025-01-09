using Beymen.Demo.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(config => config.AddConsole());

builder.Services.AddControllers();

builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

app.MapControllers();

await app.RunAsync();