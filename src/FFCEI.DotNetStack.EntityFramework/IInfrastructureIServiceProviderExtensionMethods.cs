using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace FFCEI.DotNetStack.EntityFramework;

public static class IInfrastructureIServiceProviderExtensionMethods
{
    public static IServiceProvider? ApplicationServiceProvider(this IInfrastructure<IServiceProvider> infrastructureServiceProvider)
    {
        var dbContextOptions = infrastructureServiceProvider?.Instance.GetRequiredService<IDbContextOptions>();
        var serviceProvider = dbContextOptions?.ApplicationServiceProvider();

        return serviceProvider;
    }

    public static IServiceProvider? ApplicationServiceProvider(this DbContextOptionsBuilder dbContextOptionsBuilder)
    {
        var dbContextOptions = dbContextOptionsBuilder?.Options;
        var serviceProvider = dbContextOptions?.ApplicationServiceProvider();

        return serviceProvider;
    }

    public static IServiceProvider? ApplicationServiceProvider(this IDbContextOptions dbContextOptions)
    {
        var coreOptions = dbContextOptions?.FindExtension<CoreOptionsExtension>();
        var serviceProvider = coreOptions?.ApplicationServiceProvider;

        return serviceProvider;
    }
}
