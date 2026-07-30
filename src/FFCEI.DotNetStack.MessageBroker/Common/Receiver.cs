using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.MessageBroker.Interfaces;
using FFCEI.DotNetStack.Serialization;
using FFCEI.DotNetStack.Serialization.Interfaces;

namespace FFCEI.DotNetStack.MessageBroker.Common;

public abstract class Receiver(MessageBrokerProviderType messageBrokerProvider, string subject)
    : MessageHandlerRegistry, IConfigurableReceiver
{
    private bool _configured;
    private bool _started;
    private bool _disposed;

    public MessageBrokerProviderType MessageBrokerProvider { get; private set; } = messageBrokerProvider;
    public string Subject { get; private set; } = subject;
    public MessageBrokerAddress? Address { get; internal set; }
    internal SerializationAlgorithm? SerializationAlgorithm { get; set; }
    public IDeserializer? Deserializer { get; set; }
    public CancellationTokenSourceFactory? CancellationTokenSourceFactory { get; set; }
    public TimeSpan PollWaitInterval { get; set; } = TimeSpan.FromMilliseconds(500);
    public TimeSpan ErrorWaitInterval { get; set; } = TimeSpan.FromMilliseconds(100);
    public int RetryMessageCount { get; set; } = 3;

    protected Task WaitAfterErrorAsync(CancellationToken cancellationToken)
        => Task.Delay(ErrorWaitInterval, cancellationToken);

    protected abstract Task DoConfigureAsync(MessageBrokerAddress address, CancellationToken cancellationToken = default);
    protected abstract Task DoStartAsync(CancellationToken cancellationToken = default);
    protected abstract Task DoStopAsync();

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (!_configured)
        {
            throw new InvalidOperationException($"Receiver not configured");
        }

        if (_started)
        {
            throw new InvalidOperationException($"Receiver already started");
        }

        await DoStartAsync(cancellationToken).ConfigureAwait(false);

        _started = true;
    }

    public async Task StopAsync()
    {
        if (CancellationTokenSourceFactory != null)
        {
            await ExceptionProtector.ExecuteTaskAsync(CancellationTokenSourceFactory.IsValueCreated ?
                CancellationTokenSourceFactory.TokenSource.CancelAsync() : Task.CompletedTask).ConfigureAwait(false);
        }

        await ExceptionProtector.ExecuteTaskAsync(DoStopAsync()).ConfigureAwait(false);

        CancellationTokenSourceFactory?.DisposeSafelyIfNeeded();
        CancellationTokenSourceFactory = null;

        _started = false;
    }

    public async Task ConfigureAsync(MessageBrokerAddress address, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_configured)
        {
            throw new InvalidOperationException($"Receiver already configured");
        }

        ArgumentNullException.ThrowIfNull(address, nameof(address));

        await DoConfigureAsync(address, cancellationToken).ConfigureAwait(false);

        _configured = true;
    }

    protected bool DeserializeMessageEnvelope(MessageEnvelope messageEnvelope)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(messageEnvelope, nameof(messageEnvelope));

            if (messageEnvelope.DeserializeEnvelope(Deserializer, this))
            {
                return true;
            }

            InvokeUnknownMessageReceived(messageEnvelope);
        }
        catch (Exception exception)
        {
            IncrementErrorCount();

            if (OnMessageHandlingException != null)
            {
                var result = ExceptionProtector.ExecuteFunctionAndReturnDefaultValue(() => OnMessageHandlingException.Invoke(exception, messageEnvelope));

                if (!(result ?? false))
                {
                    throw new DeserializeMessageEnvelopeException(exception);
                }
            }
            else
            {
                throw new DeserializeMessageEnvelopeException(exception);
            }
        }

        return false;
    }

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
            if ((CancellationTokenSourceFactory != null) && (CancellationTokenSourceFactory.IsValueCreated))
            {
                CancellationTokenSourceFactory.TokenSource.Cancel();
            }

            ExceptionProtector.ExecuteAction(() => StopAsync().GetAwaiter().GetResult());
            ExceptionProtector.ExecuteAction(OnDispose);

            CancellationTokenSourceFactory?.DisposeSafelyIfNeeded();

            _disposed = true;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        if (CancellationTokenSourceFactory != null)
        {
            await ExceptionProtector.ExecuteTaskAsync(CancellationTokenSourceFactory.IsValueCreated ?
                CancellationTokenSourceFactory.TokenSource.CancelAsync() : Task.CompletedTask).ConfigureAwait(false);
        }

        await ExceptionProtector.ExecuteTaskAsync(StopAsync()).ConfigureAwait(false);
        await ExceptionProtector.ExecuteValueTaskAsync(OnDisposeAsync()).ConfigureAwait(false);

        CancellationTokenSourceFactory?.DisposeSafelyIfNeeded();

        _disposed = true;

        GC.SuppressFinalize(this);
    }

    protected virtual void OnDispose()
    {
    }

    protected virtual ValueTask OnDisposeAsync()
        => ValueTask.CompletedTask;
}
