using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace AuthServer.Tests.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddScopedMock<T>(this IServiceCollection services, Mock<T> mockService)
        where T : class
    {
        RemoveServices(services, typeof(T));
        services.AddScoped(_ => mockService.Object);
        return services;
    }

    public static IServiceCollection AddSingletonMock<T>(this IServiceCollection services, Mock<T> mockService)
        where T : class
    {
        RemoveServices(services, typeof(T));
        services.AddSingleton(_ => mockService.Object);
        return services;
    }

    public static IServiceCollection AddTransientMock<T>(this IServiceCollection services, Mock<T> mockService)
        where T : class
    {
        RemoveServices(services, typeof(T));
        services.AddScoped(_ => mockService.Object);
        return services;
    }

    private static void RemoveServices(IServiceCollection services, Type serviceType)
    {
        var servicesToRemove = services
            .Where(x => x.ServiceType == serviceType)
            .ToList();

        foreach (var service in servicesToRemove)
        {
            services.Remove(service);
        }
    }
}