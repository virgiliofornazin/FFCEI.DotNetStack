using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.MessageBroker.Interfaces;
using FFCEI.DotNetStack.Serialization;
using FFCEI.DotNetStack.Serialization.Interfaces;

namespace FFCEI.DotNetStack.MessageBroker.Common;

public abstract class Sender(MessageBrokerProviderType messageBrokerProvider, string subject)
    : MessageCounter(), ISender
{
    private bool _disposed;

    public MessageBrokerProviderType MessageBrokerProvider { get; private set; } = messageBrokerProvider;
    public string Subject { get; private set; } = subject;
    public MessageBrokerAddress? DefaultAddress { get; set; }
    internal SerializationAlgorithm? SerializationAlgorithm { get; set; }
    public ISerializer? Serializer { get; set; }
    public Action<string>? OnMessagePublishReturned { get; set; }
    public CancellationTokenSourceFactory? CancellationTokenSourceFactory { get; set; }

    protected virtual void DoSend(MessageBrokerAddress address, MessageEnvelope messageEnvelope)
        => Task.Run(() => DoSendToAsync(address, messageEnvelope)).GetAwaiter().GetResult();

    protected abstract Task DoSendToAsync(MessageBrokerAddress address, MessageEnvelope messageEnvelope, CancellationToken cancellationToken = default);

    public void Send(object? message, IMessageAttributes? messageAttributes = null)
    {
        ArgumentNullException.ThrowIfNull(DefaultAddress, nameof(DefaultAddress));

        SendTo(DefaultAddress, message, messageAttributes);
    }

    public Task SendAsync(object? message, IMessageAttributes? messageAttributes = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(DefaultAddress, nameof(DefaultAddress));

        return SendToAsync(DefaultAddress, message, messageAttributes, cancellationToken);
    }

    public void SendTo(MessageBrokerAddress address, object? message, IMessageAttributes? messageAttributes = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        ArgumentNullException.ThrowIfNull(address, nameof(address));
        ArgumentNullException.ThrowIfNull(message, nameof(message));

        var messageEnvelope = MessageEnvelope.CreateForSend(message, messageAttributes);

        DoSend(address, messageEnvelope);

        IncrementMessageCount();
    }

    public async Task SendToAsync(MessageBrokerAddress address, object? message, IMessageAttributes? messageAttributes = null, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        ArgumentNullException.ThrowIfNull(address, nameof(address));
        ArgumentNullException.ThrowIfNull(message, nameof(message));

        var messageEnvelope = MessageEnvelope.CreateForSend(message, messageAttributes);

        await DoSendToAsync(address, messageEnvelope, cancellationToken).ConfigureAwait(false);

        IncrementMessageCount();
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
