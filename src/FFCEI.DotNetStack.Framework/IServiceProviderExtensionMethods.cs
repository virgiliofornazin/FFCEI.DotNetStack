using Microsoft.Extensions.DependencyInjection;

namespace FFCEI.DotNetStack.Framework;

public static class IServiceProviderExtensionMethods
{
    public static ScopeAndServiceResult<TService> CreateScopeAndGetService<TService>(this IServiceProvider serviceProvider) where TService : class
    {
        var scope = serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetService<TService>();

        return new ScopeAndServiceResult<TService>(scope, service);
    }

    public static ScopeAndServiceResult<object> CreateScopeAndGetService(this IServiceProvider serviceProvider, Type type)
    {
        var scope = serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetService(type);

        return new ScopeAndServiceResult<object>(scope, service);
    }

    public static ScopeAndServiceResult<TService> CreateScopeAndGetKeyedService<TService>(this IServiceProvider serviceProvider, object? serviceKey) where TService : class
    {
        var scope = serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetKeyedService<TService>(serviceKey);

        return new ScopeAndServiceResult<TService>(scope, service);
    }

    public static ScopeAndServiceResult<object> CreateScopeAndGetKeyedService(this IServiceProvider serviceProvider, Type type, object? serviceKey)
    {
        var scope = serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetKeyedService(type, serviceKey);

        return new ScopeAndServiceResult<object>(scope, service);
    }

    public static ScopeAndServiceResult<TService> CreateScopeAndGetRequiredService<TService>(this IServiceProvider serviceProvider) where TService : class
    {
        var scope = serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();

        return new ScopeAndServiceResult<TService>(scope, service);
    }

    public static ScopeAndServiceResult<object> CreateScopeAndGetRequiredService(this IServiceProvider serviceProvider, Type type)
    {
        var scope = serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService(type);

        return new ScopeAndServiceResult<object>(scope, service);
    }

    public static ScopeAndServiceResult<TService> CreateScopeAndGetRequiredKeyedService<TService>(this IServiceProvider serviceProvider, object? serviceKey) where TService : class
    {
        var scope = serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredKeyedService<TService>(serviceKey);

        return new ScopeAndServiceResult<TService>(scope, service);
    }

    public static ScopeAndServiceResult<object> CreateScopeAndGetRequiredKeyedService(this IServiceProvider serviceProvider, Type type, object? serviceKey)
    {
        var scope = serviceProvider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredKeyedService(type, serviceKey);

        return new ScopeAndServiceResult<object>(scope, service);
    }
}
