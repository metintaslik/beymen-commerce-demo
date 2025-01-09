using Beymen.Demo.Infrastructure;
using Beymen.Demo.Infrastructure.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging(config => config.AddConsole());
builder.Services.AddControllers();

builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

app.MapControllers();

app.UseMiddleware<DomainExceptionHandlingMiddleware>();

await app.RunAsync();