namespace FFCEI.DotNetStack.Framework;

public sealed class CancellationTokenSourceFactory
    : IDisposable
{
    private readonly Func<CancellationTokenSource> _cancellationTokenSourceFactory;
    private readonly Lazy<CancellationTokenSource> _cancellationTokenSource;
    private bool _disposed;

    public CancellationTokenSourceFactory()
    {
        _cancellationTokenSourceFactory = () => new();

        _cancellationTokenSource = new(_cancellationTokenSourceFactory);
    }

    public CancellationTokenSourceFactory(CancellationToken token)
    {
        if (token.CanBeCanceled)
        {
            _cancellationTokenSourceFactory = () => CancellationTokenSource.CreateLinkedTokenSource(token);
        }
        else
        {
            _cancellationTokenSourceFactory = () => new();
        }

        _cancellationTokenSource = new(_cancellationTokenSourceFactory);
    }

    public CancellationTokenSourceFactory(params ReadOnlySpan<CancellationToken> tokens)
    {
        if (tokens.Any(token => token.CanBeCanceled))
        {
            var tokensArray = tokens.ToArray();

            _cancellationTokenSourceFactory = () => CancellationTokenSource.CreateLinkedTokenSource(tokensArray);
        }
        else
        {
            _cancellationTokenSourceFactory = () => new();
        }

        _cancellationTokenSource = new(_cancellationTokenSourceFactory);
    }

    public CancellationTokenSourceFactory(Func<CancellationTokenSource> factory)
    {
        ArgumentNullException.ThrowIfNull(factory, nameof(factory));

        _cancellationTokenSourceFactory = factory;
        _cancellationTokenSource = new(_cancellationTokenSourceFactory);
    }

    public bool IsValueCreated
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            return _cancellationTokenSource.IsValueCreated;
        }
    }

    public CancellationTokenSource TokenSource
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            return _cancellationTokenSource.Value;
        }
    }

    public CancellationToken Token
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            return TokenSource.Token;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_cancellationTokenSource.IsValueCreated)
        {
            _cancellationTokenSource.Value.DisposeSafelyIfNeeded();
        }

        _disposed = true;

        GC.SuppressFinalize(this);
    }
}
