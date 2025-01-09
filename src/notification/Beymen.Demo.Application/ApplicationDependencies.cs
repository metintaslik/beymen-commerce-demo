using Microsoft.Extensions.DependencyInjection;

namespace Beymen.Demo.Application;

public static class ApplicationDependencies
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        return services;
    }
}