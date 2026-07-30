using FFCEI.DotNetStack.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using MySql.EntityFrameworkCore.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Oracle.EntityFrameworkCore.Infrastructure.Internal;
using System.Reflection;

namespace FFCEI.DotNetStack.EntityFramework;

public static class DbContextExtensionMethods
{
    private static readonly HashSet<string> _knownProviderExtensionNames =
        [
            nameof(InMemoryOptionsExtension),
            nameof(SqliteOptionsExtension),
            nameof(MySQLOptionsExtension),
            nameof(NpgsqlOptionsExtension),
            nameof(OracleOptionsExtension),
            nameof(SqlServerOptionsExtension)
        ];

    public static string? DatabaseProviderExtensionName(this DbContextOptions options)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        var dbContextOptionsExtension = options.Extensions.FirstOrDefault(e => _knownProviderExtensionNames.Contains(e.GetType().Name));

        return dbContextOptionsExtension?.GetType().Name;
    }

    public static string? DatabaseProviderExtensionName(this DbContextOptionsBuilder optionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder, nameof(optionsBuilder));

        return optionsBuilder.Options.DatabaseProviderExtensionName();
    }

    public static string DatabaseProviderName(this DbContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        var providerName = context.Database.ProviderName;

        return providerName ?? string.Empty;
    }

    public static DatabaseProviderType DatabaseProviderFromExtensionName(this string providerName)
    {
        ArgumentNullException.ThrowIfNull(providerName, nameof(providerName));

        return
            providerName.Contains("InMemory", StringComparison.InvariantCultureIgnoreCase) ? DatabaseProviderType.InMemory :
            providerName.Contains("Sqlite", StringComparison.InvariantCultureIgnoreCase) ? DatabaseProviderType.Sqlite :
            providerName.Contains("MySql", StringComparison.InvariantCultureIgnoreCase) ? DatabaseProviderType.MySql :
            providerName.Contains("Npgsql", StringComparison.InvariantCultureIgnoreCase) ? DatabaseProviderType.Postgres :
            providerName.Contains("Oracle", StringComparison.InvariantCultureIgnoreCase) ? DatabaseProviderType.Oracle :
            providerName.Contains("SqlServer", StringComparison.InvariantCultureIgnoreCase) ? DatabaseProviderType.SqlServer :
            DatabaseProviderType.Unknown;
    }

    public static DatabaseProviderType DatabaseProviderFromString(this string providerName)
    {
        ArgumentNullException.ThrowIfNull(providerName, nameof(providerName));

        return
            providerName.StartsWith("Microsoft.EntityFrameworkCore.InMemory", StringComparison.InvariantCulture) ? DatabaseProviderType.InMemory :
            providerName.StartsWith("Microsoft.EntityFrameworkCore.Sqlite", StringComparison.InvariantCulture) ? DatabaseProviderType.Sqlite :
            providerName.StartsWith("MySql.EntityFrameworkCore", StringComparison.InvariantCulture) ? DatabaseProviderType.MySql :
            providerName.StartsWith("Npgsql.EntityFrameworkCore.PostgreSQL", StringComparison.InvariantCulture) ? DatabaseProviderType.Postgres :
            providerName.StartsWith("Oracle.EntityFrameworkCore", StringComparison.InvariantCulture) ? DatabaseProviderType.Oracle :
            providerName.StartsWith("Microsoft.EntityFrameworkCore.SqlServer", StringComparison.InvariantCulture) ? DatabaseProviderType.SqlServer :
            DatabaseProviderType.Unknown;
    }

    public static DatabaseProviderType DatabaseProvider(this DbContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        return DatabaseProviderFromString(context.DatabaseProviderName());
    }

    public static DatabaseProviderType DatabaseProvider(this DbContextOptions options)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        return DatabaseProviderFromExtensionName(options.DatabaseProviderExtensionName() ?? string.Empty);
    }

    public static DatabaseProviderType DatabaseProvider(this DbContextOptionsBuilder optionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder, nameof(optionsBuilder));

        return DatabaseProviderFromExtensionName(optionsBuilder.DatabaseProviderExtensionName() ?? string.Empty);
    }

    public static Type? ClrTypeOfEntityType(this DbContext context, object entityOrType)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentNullException.ThrowIfNull(entityOrType, nameof(entityOrType));

        var entityType = (entityOrType as Type ?? entityOrType.GetType()).GetLazyLoadingProxyUnderlyingType();

        return context.Model.FindEntityType(entityType)?.ClrType;
    }

    private static readonly MethodInfo? _methodDbContextSet = typeof(DbContext)
        .GetMethods(BindingFlags.Public | BindingFlags.Instance)
        .FirstOrDefault(m => m.Name == nameof(DbContext.Set) && m.IsGenericMethod && m.GetParameters().Length == 0);

    private static readonly MethodInfo? _methodEntityFrameworkQueryableExtensionsIgnoreQueryFilters1 = typeof(EntityFrameworkQueryableExtensions)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .FirstOrDefault(m => m.Name == nameof(EntityFrameworkQueryableExtensions.IgnoreQueryFilters) && m.IsGenericMethod && m.GetParameters().Length == 1);

    private static readonly MethodInfo? _methodEntityFrameworkQueryableExtensionsIgnoreQueryFilters2 = typeof(EntityFrameworkQueryableExtensions)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .FirstOrDefault(m => m.Name == nameof(EntityFrameworkQueryableExtensions.IgnoreQueryFilters) && m.IsGenericMethod && m.GetParameters().Length == 2);

    private static readonly MethodInfo? _methodEntityFrameworkQueryableExtensionsIgnoreAutoIncludes = typeof(EntityFrameworkQueryableExtensions)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .FirstOrDefault(m => m.Name == nameof(EntityFrameworkQueryableExtensions.IgnoreAutoIncludes) && m.IsGenericMethod);

    private static readonly MethodInfo? _methodEntityFrameworkQueryableExtensionsAsTracking = typeof(EntityFrameworkQueryableExtensions)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .FirstOrDefault(m => m.Name == nameof(EntityFrameworkQueryableExtensions.AsTracking) && m.IsGenericMethod && m.GetParameters().Length == 1);

    private static readonly MethodInfo? _methodEntityFrameworkQueryableExtensionsAsNoTrackingWithIdentityResolution = typeof(EntityFrameworkQueryableExtensions)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .FirstOrDefault(m => m.Name == nameof(EntityFrameworkQueryableExtensions.AsNoTrackingWithIdentityResolution) && m.IsGenericMethod && m.GetParameters().Length == 1);

    public static IQueryable? DbSetGeneric(this DbContext context, Type entityType, IReadOnlyCollection<string>? ignoreQueryFilters = null, bool? ignoreAutoIncludes = null, bool? tracking = null)
    {
        ArgumentNullException.ThrowIfNull(entityType, nameof(entityType));

        if (_methodDbContextSet == null)
        {
            throw new InvalidOperationException("Method 'DbContext.Set<T>()' not found by reflection; incompatible EF Core version");
        }

        var queryable = _methodDbContextSet
            .MakeGenericMethod(entityType)
            .Invoke(context, null) as IQueryable ??
            throw new InvalidOperationException($"Could not create DbSet for entity type '{entityType.Name}'");

        if (ignoreQueryFilters != null)
        {
            if (ignoreQueryFilters.Count == 0)
            {
                if (_methodEntityFrameworkQueryableExtensionsIgnoreQueryFilters1 == null)
                {
                    throw new InvalidOperationException("Method 'IgnoreQueryFilters' not found by reflection; incompatible EF Core version");
                }

                queryable = _methodEntityFrameworkQueryableExtensionsIgnoreQueryFilters1
                   .MakeGenericMethod(entityType)
                   .Invoke(null, [queryable]) as IQueryable ??
                   throw new InvalidOperationException($"IgnoreQueryFilters returned no queryable for entity type '{entityType.Name}'");
            }
            else
            {
                if (_methodEntityFrameworkQueryableExtensionsIgnoreQueryFilters2 == null)
                {
                    throw new InvalidOperationException("Method 'IgnoreQueryFilters(names)' not found by reflection; incompatible EF Core version");
                }

                queryable = _methodEntityFrameworkQueryableExtensionsIgnoreQueryFilters2
                    .MakeGenericMethod(entityType)
                    .Invoke(null, [queryable, ignoreQueryFilters]) as IQueryable ??
                    throw new InvalidOperationException($"IgnoreQueryFilters returned no queryable for entity type '{entityType.Name}'");
            }
        }

        if (ignoreAutoIncludes.HasValue)
        {
            if (ignoreAutoIncludes ?? false)
            {
                if (_methodEntityFrameworkQueryableExtensionsIgnoreAutoIncludes == null)
                {
                    throw new InvalidOperationException("Method 'IgnoreAutoIncludes' not found by reflection; incompatible EF Core version");
                }

                queryable = _methodEntityFrameworkQueryableExtensionsIgnoreAutoIncludes
                    .MakeGenericMethod(entityType)
                    .Invoke(null, [queryable]) as IQueryable ??
                    throw new InvalidOperationException($"IgnoreAutoIncludes returned no queryable for entity type '{entityType.Name}'");
            }
        }

        if (tracking.HasValue)
        {
            var trackingMethod = (tracking.Value ? _methodEntityFrameworkQueryableExtensionsAsTracking : _methodEntityFrameworkQueryableExtensionsAsNoTrackingWithIdentityResolution) ??
                throw new InvalidOperationException("Method 'AsTracking'/'AsNoTrackingWithIdentityResolution' not found by reflection; incompatible EF Core version");

            queryable = trackingMethod
                .MakeGenericMethod(entityType)
                .Invoke(null, [queryable]) as IQueryable ??
                throw new InvalidOperationException($"Tracking configuration returned no queryable for entity type '{entityType.Name}'");
        }

        return queryable;
    }
}
