using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.MessageBroker.Common;
using RabbitMQ.Client;

namespace FFCEI.DotNetStack.MessageBroker.RabbitMQ;

public sealed class RabbitMQSender(IChannel channel, string subject, WeakReference<RabbitMQConnector> connector)
    : Sender(MessageBrokerProviderType.RabbitMQ, subject)
{
    private bool _basicReturnSubscribed;
#pragma warning disable CA2213 // Disposable fields should be disposed
    private readonly SemaphoreSlim _basicReturnSubscribedLock = new(1, 1);
#pragma warning restore CA2213 // Disposable fields should be disposed

    public WeakReference<RabbitMQConnector> Connector { get; private set; } = connector;
    public IChannel Channel { get; private set; } = channel;

    protected override async Task DoSendToAsync(MessageBrokerAddress address, MessageEnvelope messageEnvelope, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(Channel, nameof(Channel));

        if (!_basicReturnSubscribed)
        {
            await _basicReturnSubscribedLock.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                if (!_basicReturnSubscribed)
                {
                    Channel.BasicReturnAsync += (_, eventArguments) =>
                    {
                        OnMessagePublishReturned?.Invoke(eventArguments.ReplyText);

                        return Task.CompletedTask;
                    };

                    _basicReturnSubscribed = true;
                }
            }
            finally
            {
                _ = _basicReturnSubscribedLock.Release();
            }
        }

        var basicProperties = RabbitMQConnector.MessageAttributesToBasicProperties(messageEnvelope);

        AmqpCommons.ExtractExchangeAndRoutingKey(address, out var exchange, out var routingKey);

        var mandatory = address.Type == MessageBrokerAddressType.Queue;

        using var cancellationTokenSourceFactory = new CancellationTokenSourceFactory(cancellationToken);

        var token = cancellationTokenSourceFactory.Token;

        try
        {
            CancellationTokenSourceFactory = cancellationTokenSourceFactory;

            messageEnvelope.SerializeEnvelope(Serializer);

            await Channel.BasicPublishAsync(exchange, routingKey, mandatory, basicProperties, messageEnvelope.PayloadBytes, token).ConfigureAwait(false);
        }
        finally
        {
            CancellationTokenSourceFactory = null;
        }
    }

    protected override void OnDispose()
    {
        var hasConnector = Connector.TryGetTarget(out var connector);

        if (hasConnector && (connector != null))
        {
            connector.CloseChannelAsync(Channel).GetAwaiter().GetResult();
        }

        _basicReturnSubscribedLock.Dispose();
    }

    protected override async ValueTask OnDisposeAsync()
    {
        var hasConnector = Connector.TryGetTarget(out var connector);

        if (hasConnector && (connector != null))
        {
            await connector.CloseChannelAsync(Channel).ConfigureAwait(false);
        }

        _basicReturnSubscribedLock.Dispose();
    }
}
