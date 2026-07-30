using FFCEI.DotNetStack.EntityFramework.Storage.DataProtection;
using FFCEI.DotNetStack.Framework;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace FFCEI.DotNetStack.EntityFramework;

public static class IServiceProviderExtensionMethods
{
    public static IDataProtectionProvider? DataProtectionProvider
    {
        get;
        internal set
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            field = value;
        }
    }

    public static IServiceProvider UseDataProtection(
        this IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));

        DataProtectionProvider ??= serviceProvider.GetService<IDataProtectionProvider>();

        ArgumentNullException.ThrowIfNull(DataProtectionProvider, nameof(DataProtectionProvider));

        return serviceProvider;
    }

    private static readonly MethodInfo? _methodRelationalDatabaseCreatorGetCreateTablesCommands = typeof(RelationalDatabaseCreator)
        .GetMethod("GetCreateTablesCommands", BindingFlags.Instance | BindingFlags.NonPublic);

    public static void EnsureDataProtectionDatabaseCreated(
        this IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));

        if (!IServiceCollectionExtensionMethods.UsingKeyDbContextDefault)
        {
            return;
        }

        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<DataProtectionKeyDbContext>();

        ExceptionProtector.ExecuteAction(() =>
        {
            _ = context.Database.EnsureCreated();
        });

        serviceProvider.EnsureDataProtectionTableCreated();
    }

    public static void EnsureDataProtectionTableCreated(
        this IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));

        if (!IServiceCollectionExtensionMethods.UsingKeyDbContextDefault)
        {
            return;
        }

        using var scope = serviceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<DataProtectionKeyDbContext>();

        if (context.Database.GetService<IDatabaseCreator>() is RelationalDatabaseCreator databaseCreator)
        {
            if (_methodRelationalDatabaseCreatorGetCreateTablesCommands == null)
            {
                throw new InvalidOperationException("Method 'RelationalDatabaseCreator.GetCreateTablesCommands' not found by reflection; incompatible EF Core version");
            }

            if (_methodRelationalDatabaseCreatorGetCreateTablesCommands.Invoke(databaseCreator, [MigrationsSqlGenerationOptions.Default]) is IEnumerable<MigrationCommand> migrationCommands)
            {
                var commands = migrationCommands.Where(command => command.CommandText.Contains(nameof(DataProtectionKey), StringComparison.InvariantCulture)).ToList();

                foreach (var command in commands)
                {
                    ExceptionProtector.ExecuteAction(() =>
                    {
                        _ = context.Database.ExecuteSqlRaw(command.CommandText);
                    });
                }
            }
        }
    }
}
