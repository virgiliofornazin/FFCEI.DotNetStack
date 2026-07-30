using Microsoft.Extensions.DependencyInjection;

namespace FFCEI.DotNetStack.Framework;

public sealed class ScopeAndServiceResult<TService>
    : IDisposable, IAsyncDisposable where TService : class
{
    private bool _disposed;

    public IServiceScope Scope
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            return field;
        }
        internal set;
    }

    public IServiceProvider ServiceProvider => Scope.ServiceProvider;

    public TService? Service
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            return field;
        }
        internal set;
    }

    public TService RequiredService => Service!;

    internal ScopeAndServiceResult(IServiceScope scope, TService? service)
    {
        Scope = scope;
        Service = service;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Scope.DisposeSafelyIfNeeded();
        Service?.DisposeObjectSafelyIfNeeded();

        _disposed = true;

        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        await Scope.DisposeObjectSafelyIfNeededAsync().ConfigureAwait(false);
        await Service.DisposeObjectSafelyIfNeededAsync().ConfigureAwait(false);

        _disposed = true;

        GC.SuppressFinalize(this);
    }
}
