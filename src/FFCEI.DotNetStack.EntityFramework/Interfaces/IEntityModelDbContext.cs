using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FFCEI.DotNetStack.EntityFramework.Interfaces;

public interface IEntityModelDbContext
    : IReadOnlyEntityModelDbContext
{
    int SaveChanges();
    int SaveChanges(bool acceptAllChangesOnSuccess);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default);
    ChangeTracker ChangeTracker { get; }
    int ExecuteSqlRaw(string sql, params IEnumerable<object?> parameters);
    Task<int> ExecuteSqlRawAsync(string sql, params IEnumerable<object> parameters);
    Task<int> ExecuteSqlRawAsync(string sql, IEnumerable<object> parameters, CancellationToken cancellationToken);
}
