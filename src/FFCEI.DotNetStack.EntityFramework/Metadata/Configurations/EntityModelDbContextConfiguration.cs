using EFCoreSecondLevelCacheInterceptor;
using FFCEI.DotNetStack.EntityFramework.Infrastructure;
using FFCEI.DotNetStack.EntityFramework.Interceptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

namespace FFCEI.DotNetStack.EntityFramework.Metadata.Configurations;

public static class EntityModelDbContextConfiguration
{
    public static void ConfigureDbContext(DbContext context, DbContextOptionsBuilder optionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentNullException.ThrowIfNull(optionsBuilder, nameof(optionsBuilder));

        _ = optionsBuilder.ReplaceService<IModelCacheKeyFactory, EntityModelDbContextModelCacheKeyFactory>();
        _ = optionsBuilder.AddInterceptors(new LogicallyDeletableEntitiesForeignKeyDeleteBehaviourSaveChangesInterceptor());
        _ = optionsBuilder.AddInterceptors(new RecoverLogicallyDeletableEntitiesSaveChangesInterceptor());
        _ = optionsBuilder.AddInterceptors(new GenerateAutomaticPropertyValuesSaveChangesInterceptor());

        var entityModelDbContext = context as EntityModelDbContext;
        var serviceProvider = optionsBuilder.Options.FindExtension<CoreOptionsExtension>()?.ApplicationServiceProvider;
        var environment = serviceProvider?.GetService<IHostEnvironment>();
        var addSecondLevelCache = (entityModelDbContext?.UseSecondLevelCache ?? false) && (optionsBuilder.DatabaseProvider() != DatabaseProviderType.Oracle);
        var useLadyLoadProxies = entityModelDbContext?.UseLazyLoadProxies ?? false;
        var isProduction = (environment?.IsProduction() ?? false);
        var isDebugging = System.Diagnostics.Debugger.IsAttached || !isProduction;
        var sensitiveDataLogging = System.Diagnostics.Debugger.IsAttached && !isProduction;
        var logDetailedErrors = System.Diagnostics.Debugger.IsAttached && !isProduction;

        if (addSecondLevelCache)
        {
            var secondLevelCacheInterceptor = serviceProvider?.GetService<SecondLevelCacheInterceptor>();

            if (secondLevelCacheInterceptor != null)
            {
                _ = optionsBuilder.AddInterceptors(secondLevelCacheInterceptor);
            }
        }

        if (useLadyLoadProxies)
        {
            _ = optionsBuilder.UseLazyLoadingProxies();
        }

        if (sensitiveDataLogging)
        {
            _ = optionsBuilder.EnableSensitiveDataLogging();
        }

        if (logDetailedErrors)
        {
            _ = optionsBuilder.EnableDetailedErrors();
        }

        if (!isDebugging)
        {
            _ = optionsBuilder.UseLoggerFactory(NullLoggerFactory.Instance);
        }
    }
}
