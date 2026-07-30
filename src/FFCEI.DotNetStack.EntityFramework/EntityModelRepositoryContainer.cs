using FFCEI.DotNetStack.EntityFramework.Interfaces;
using FFCEI.DotNetStack.EntityFramework.Interfaces.Generic;
using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace FFCEI.DotNetStack.EntityFramework;

public class EntityModelRepositoryContainer
    : IEntityModelRepositoryContainer
{
    private readonly HashSet<IReadOnlyEntityModelRepository> _trackedRepositories = [];
    private DbContext _context;
    private bool _disposed;

    public EntityModelRepositoryContainer(DbContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        _context = context;
    }

    private DbContext CheckedContext
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            return _context;
        }
    }

    public DatabaseProviderType DatabaseProvider
        => CheckedContext.DatabaseProvider();

    public string DatabaseProviderName
        => CheckedContext.DatabaseProviderName();

    public int SaveChanges()
        => CheckedContext.SaveChanges();

    public int SaveChanges(bool acceptAllChangesOnSuccess)
        => CheckedContext.SaveChanges(acceptAllChangesOnSuccess);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => CheckedContext.SaveChangesAsync(cancellationToken);

    public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        => CheckedContext.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

    public ChangeTracker ChangeTracker
        => CheckedContext.ChangeTracker;

    public int ExecuteSqlRaw(string sql, params IEnumerable<object?> parameters)
        => CheckedContext.Database.ExecuteSqlRaw(sql, [.. parameters]);

    public Task<int> ExecuteSqlRawAsync(string sql, params IEnumerable<object> parameters)
        => CheckedContext.Database.ExecuteSqlRawAsync(sql, [.. parameters]);

    public Task<int> ExecuteSqlRawAsync(string sql, IEnumerable<object> parameters, CancellationToken cancellationToken)
        => CheckedContext.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);

    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            foreach (var repository in _trackedRepositories)
            {
                repository.DisposeSafelyIfNeeded();
            }

            _trackedRepositories.Clear();

            if (_context != null)
            {
                _context.DisposeSafelyIfNeeded();
                _context = null!;
            }

            _disposed = true;
        }
    }

    public DbContext DetachDbContext()
    {
        var context = CheckedContext;

        _context = null!;

        return context;
    }

    private TRepository TrackRepository<TRepository>(TRepository repository) where TRepository : IReadOnlyEntityModelRepository
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _ = _trackedRepositories.Add(repository);

        return repository;
    }

    public IReadOnlyEntityModelRepository<TEntityModel> ReadOnlyRepository<TEntityModel>(ref IReadOnlyEntityModelRepository<TEntityModel>? repository)
        where TEntityModel : class, IEntityModel
        => TrackRepository(repository ??= new ReadOnlyEntityModelRepository<TEntityModel>(CheckedContext));

    public IReadOnlyEntityModelRepository<TEntityModelInterface> ReadOnlyRepository<TEntityModel, TEntityModelInterface>(ref IReadOnlyEntityModelRepository<TEntityModelInterface>? repository)
        where TEntityModel : class, IEntityModel, TEntityModelInterface where TEntityModelInterface : class, IEntityModel
        => TrackRepository(repository ??= new ReadOnlyEntityModelRepository<TEntityModel, TEntityModelInterface>(CheckedContext));

    public IEntityModelRepository<TEntityModel> Repository<TEntityModel>(ref IEntityModelRepository<TEntityModel>? repository)
        where TEntityModel : class, IEntityModel
        => TrackRepository(repository ??= new EntityModelRepository<TEntityModel>(CheckedContext));

    public IEntityModelRepository<TEntityModelInterface> Repository<TEntityModel, TEntityModelInterface>(ref IEntityModelRepository<TEntityModelInterface>? repository)
        where TEntityModel : class, IEntityModel, TEntityModelInterface where TEntityModelInterface : class, IEntityModel
        => TrackRepository(repository ??= new EntityModelRepository<TEntityModel, TEntityModelInterface>(CheckedContext));

    public void EnsureCreated()
        => CheckedContext.Database.EnsureCreated();

    public Task EnsureCreatedAsync(CancellationToken cancellationToken = default)
        => CheckedContext.Database.EnsureCreatedAsync(cancellationToken);

    public void EnsureDeleted()
        => CheckedContext.Database.EnsureDeleted();

    public Task EnsureDeletedAsync(CancellationToken cancellationToken = default)
        => CheckedContext.Database.EnsureDeletedAsync(cancellationToken);

    public void Migrate()
    {
        if (DatabaseProvider != DatabaseProviderType.InMemory)
        {
            CheckedContext.Database.Migrate();
        }
    }

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        if (DatabaseProvider != DatabaseProviderType.InMemory)
        {
            await CheckedContext.Database.MigrateAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
