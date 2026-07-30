using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.MessageBroker.Interfaces;
using FFCEI.DotNetStack.Serialization;
using FFCEI.DotNetStack.Serialization.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FFCEI.DotNetStack.MessageBroker.Common;

public abstract class MessageBrokerConnector(MessageBrokerProviderType messageBrokerProvider, ILogger? logger = null)
        : IMessageBrokerConnector
{
    private bool _disposed;

    protected bool Disposed => _disposed;
    internal SerializationAlgorithm? SerializationAlgorithm { get; set; }
    public ILogger Logger { get; internal set; } = logger ?? NullLogger.Instance;
    public MessageBrokerProviderType MessageBrokerProvider { get; private set; } = messageBrokerProvider;

    public abstract bool Started();

    public abstract Task<IReceiver> GetReceiverAsync(string subject, IDeserializer? deserializer = null, CancellationToken cancellationToken = default);

    public abstract Task<ISender> GetSenderAsync(string subject, MessageBrokerAddress? defaultAddress = null, ISerializer? serializer = null, CancellationToken cancellationToken = default);

    public abstract Task StartAsync(Uri uri, CancellationToken cancellationToken = default);

    public virtual Task StartAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString, nameof(connectionString));

        return StartAsync(new Uri(connectionString), cancellationToken);
    }

    public abstract Task StopAsync();

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
            ExceptionProtector.ExecuteAction(() => { var _ = StopAsync(); });
            ExceptionProtector.ExecuteAction(OnDispose);

            _disposed = true;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        await ExceptionProtector.ExecuteTaskAsync(StopAsync()).ConfigureAwait(false);
        await ExceptionProtector.ExecuteValueTaskAsync(OnDisposeAsync()).ConfigureAwait(false);

        _disposed = true;

        GC.SuppressFinalize(this);
    }

    protected virtual void OnDispose()
    {
    }

    protected virtual ValueTask OnDisposeAsync()
        => ValueTask.CompletedTask;
}
