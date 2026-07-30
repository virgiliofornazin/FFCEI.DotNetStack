using FFCEI.DotNetStack.Framework;
using StackExchange.Redis;

namespace FFCEI.DotNetStack.Caching;

public sealed class RedisResilientConnectionFactory
    : IDisposable, IAsyncDisposable
{
    private readonly Func<Lazy<Task<IConnectionMultiplexer>>> _lazyConnectionFactory;
    private Lazy<Task<IConnectionMultiplexer>> _lazyConnection;
    private bool _disposed;

    public RedisResilientConnectionFactory(string connectionString)
    {
        _lazyConnectionFactory = () => CreateLazyConnection(connectionString);
        _lazyConnection = _lazyConnectionFactory();
    }

    public async Task<IConnectionMultiplexer> MultiplexerAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        try
        {
            return await _lazyConnection.Value.ConfigureAwait(false);
        }
        catch
        {
            _lazyConnection = _lazyConnectionFactory();

            throw;
        }
    }

    public async Task<IDatabase> GetDatabaseAsync(int db = -1)
        => (await MultiplexerAsync().ConfigureAwait(false)).GetDatabase(db);

    private static Lazy<Task<IConnectionMultiplexer>> CreateLazyConnection(string connectionString)
        => new(async () =>
        {
            var redisConfigurationOptions = ConfigurationOptions.Parse(connectionString);

            redisConfigurationOptions.AbortOnConnectFail = false;
            redisConfigurationOptions.ReconnectRetryPolicy ??= new LinearRetry(100);

            return await ConnectionMultiplexer.ConnectAsync(redisConfigurationOptions).ConfigureAwait(false);
        },
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static Func<Task<IConnectionMultiplexer>> CreateResilientConnectionFactory(string connectionString)
    {
        var lazyConnection = CreateLazyConnection(connectionString);

        return async () =>
        {
            try
            {
                return await lazyConnection.Value.ConfigureAwait(false);
            }
            catch
            {
                lazyConnection = CreateLazyConnection(connectionString);

                throw;
            }
        };
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        if (_lazyConnection.IsValueCreated)
        {
            var multiplexer = ExceptionProtector.ExecuteFunctionAndReturnOrNullReference(() => _lazyConnection.Value.GetAwaiter().GetResult());

            multiplexer.DisposeSafelyIfNeeded();
        }

        _disposed = true;

        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        if (_lazyConnection.IsValueCreated)
        {
            var multiplexer = await ExceptionProtector.ExecuteTaskFunctionAndReturnOrNullReference(async () => await _lazyConnection.Value.ConfigureAwait(false)).ConfigureAwait(false);

            if (multiplexer != null)
            {
                await multiplexer.DisposeSafelyIfNeededAsync().ConfigureAwait(false);
            }
        }

        _disposed = true;

        GC.SuppressFinalize(this);
    }
}
