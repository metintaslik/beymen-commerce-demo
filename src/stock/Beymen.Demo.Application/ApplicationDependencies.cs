using Beymen.Demo.Application.Interfaces;
using Beymen.Demo.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Beymen.Demo.Application;

public static class ApplicationDependencies
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IStockService, StockService>();

        return services;
    }
}