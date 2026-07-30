using FFCEI.DotNetStack.EntityFramework.Interfaces.Generic;
using FFCEI.DotNetStack.Models.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections;
using System.Linq.Expressions;

namespace FFCEI.DotNetStack.EntityFramework;

public class ReadOnlyEntityModelRepository<TEntityModel>(DbContext context)
    : ReadOnlyEntityModelRepository<TEntityModel, TEntityModel>(context) where TEntityModel : class, IEntityModel
{
}

public class ReadOnlyEntityModelRepository<TEntityModel, TEntityModelInterface>
    : IReadOnlyEntityModelRepository<TEntityModelInterface>, IAsyncEnumerable<TEntityModelInterface> where TEntityModel : class, IEntityModel, TEntityModelInterface where TEntityModelInterface : class, IEntityModel
{
    private readonly DbContext _context;
    private readonly DbSet<TEntityModel> _set;
    private bool _disposed;

    public ReadOnlyEntityModelRepository(DbContext context)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        _context = context;
        _set = _context.Set<TEntityModel>();
    }

    protected DbSet<TEntityModel> CheckedSet
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            return _set;
        }
    }

    protected DbContext CheckedContext
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            return _context;
        }
    }

    public IEntityType EntityType
        => CheckedSet.EntityType;

    public Type ElementType
        => CheckedSet.AsQueryable<TEntityModel>().ElementType;

    public Expression Expression
        => CheckedSet.AsQueryable<TEntityModel>().Expression;

    public IQueryProvider Provider
        => CheckedSet.AsQueryable<TEntityModel>().Provider;

    public DatabaseProviderType DatabaseProvider
        => CheckedContext.DatabaseProvider();

    public string DatabaseProviderName
        => CheckedContext.DatabaseProviderName();

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
            OnDispose();

            _disposed = true;
        }
    }

    protected virtual void OnDispose()
    {
    }

    public ValueTask<TEntityModelInterface?> FindByKeyAsync(params IEnumerable<object> keyValues)
        => FindByKeyAsync(CancellationToken.None, keyValues);

    public async ValueTask<TEntityModelInterface?> FindByKeyAsync(CancellationToken cancellationToken, params IEnumerable<object> keyValues)
        => await CheckedSet.FindAsync([.. keyValues], cancellationToken).ConfigureAwait(false);

    public ValueTask<IEntityModel?> FindModelByKeyAsync(params IEnumerable<object> keyValues)
        => FindModelByKeyAsync(CancellationToken.None, keyValues);

    public async ValueTask<IEntityModel?> FindModelByKeyAsync(CancellationToken cancellationToken, params IEnumerable<object> keyValues)
        => await CheckedSet.FindAsync([.. keyValues], cancellationToken).ConfigureAwait(false);

    public IAsyncEnumerator<TEntityModelInterface> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => CheckedSet.GetAsyncEnumerator(cancellationToken: cancellationToken);

    public IEnumerator<TEntityModelInterface> GetEnumerator()
        => CheckedSet.AsEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => CheckedSet.AsEnumerable().GetEnumerator();
}
