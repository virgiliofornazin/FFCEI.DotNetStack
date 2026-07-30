/* using Microsoft.Data.Sqlite; */
using Microsoft.EntityFrameworkCore;
/* using Microsoft.Extensions.DependencyInjection; */

namespace FFCEI.DotNetStack.DevelopmentTests.Library.EntityFramework;

public static class DbConnectionOptionsBuilderSettings
{
    public static void ConfigureDbContextConnection(this DbContextOptionsBuilder optionsBuilder, IServiceProvider serviceProvider, string? databaseName = null)
    {
        databaseName ??= "dotnetstack";

        // TODO: POOLING?

        /*
        _ = optionsBuilder.UseInMemoryDatabase($"{databaseName}");
        _ = optionsBuilder.UseSqlite(serviceProvider.GetRequiredService<SqliteConnection>());
        _ = optionsBuilder.UseMySQL($"server=i9wkslnx;database={databaseName};user=dotnetstack;password=dotnetstack;charset=utf8mb4");
        _ = optionsBuilder.UseNpgsql($"Host=i9wkslnx;Port=5432;Database={databaseName};Username=dotnetstack;Password=dotnetstack");
        _ = optionsBuilder.UseOracle($"User Id=dotnetstack;Password=dotnetstack;Data Source=i9wkslnx:1521/FREE;");
        */
        _ = optionsBuilder.UseSqlServer($"Server=i9wkslnx;Database={databaseName};User Id=dotnetstack;Password=dotnetstack;Encrypt=True;TrustServerCertificate=True");
    }
}
