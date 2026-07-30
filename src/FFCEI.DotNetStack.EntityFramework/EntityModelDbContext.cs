using FFCEI.DotNetStack.EntityFramework.Interfaces;
using FFCEI.DotNetStack.EntityFramework.Metadata.Builders;
using FFCEI.DotNetStack.EntityFramework.Metadata.Configurations;
using FFCEI.DotNetStack.EntityFramework.Metadata.Conventions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace FFCEI.DotNetStack.EntityFramework;

public class EntityModelDbContext
    : DbContext, IEntityModelDbContext
{
    public EntityModelDbContext(DbContextOptions options, Assembly rootAssembly)
        : base(options)
    {
        ArgumentNullException.ThrowIfNull(rootAssembly, nameof(rootAssembly));

        RootAssembly = rootAssembly;
    }

    public Assembly RootAssembly { get; set; }

    public DatabaseProviderType DatabaseProvider
        => this.DatabaseProvider();

    public string DatabaseProviderName
        => this.DatabaseProviderName();

    public int ExecuteSqlRaw(string sql, params IEnumerable<object?> parameters)
        => Database.ExecuteSqlRaw(sql, [.. parameters]);

    public Task<int> ExecuteSqlRawAsync(string sql, params IEnumerable<object> parameters)
        => Database.ExecuteSqlRawAsync(sql, [.. parameters]);

    public Task<int> ExecuteSqlRawAsync(string sql, IEnumerable<object> parameters, CancellationToken cancellationToken)
        => Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);

    public bool UseLazyLoadProxies { get; set; } = true;
    public bool UseSecondLevelCache { get; set; } = true;

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder, nameof(configurationBuilder));

        EntityModelDbContextConventions.ConfigureConventions(configurationBuilder, DatabaseProvider);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder, nameof(modelBuilder));

        var logger = this.GetService<ILoggerFactory>().CreateLogger(typeof(EntityModelDbContextModelBuilder));

        EntityModelDbContextModelBuilder.BuildModels(modelBuilder, DatabaseProvider, RootAssembly, logger);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        ArgumentNullException.ThrowIfNull(optionsBuilder, nameof(optionsBuilder));

        EntityModelDbContextConfiguration.ConfigureDbContext(this, optionsBuilder);
    }
}
