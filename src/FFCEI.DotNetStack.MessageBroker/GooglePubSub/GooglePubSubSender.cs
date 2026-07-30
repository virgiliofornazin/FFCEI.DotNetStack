using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.MessageBroker.Common;

namespace FFCEI.DotNetStack.MessageBroker.GooglePubSub;

public sealed class GooglePubSubSender(string subject, WeakReference<GooglePubSubConnector> connector)
    : Sender(MessageBrokerProviderType.GooglePubSub, subject)
{
    public WeakReference<GooglePubSubConnector> Connector { get; private set; } = connector;

    protected override async Task DoSendToAsync(MessageBrokerAddress address, MessageEnvelope messageEnvelope, CancellationToken cancellationToken = default)
    {
        var hasConnector = Connector.TryGetTarget(out var connector) && (connector != null);

        ObjectDisposedException.ThrowIf(!hasConnector, this);

        var topicName = connector!.GetTopicName(address);

        using var cancellationTokenSourceFactory = new CancellationTokenSourceFactory(cancellationToken);

        var token = cancellationTokenSourceFactory.Token;

        try
        {
            CancellationTokenSourceFactory = cancellationTokenSourceFactory;

            messageEnvelope.SerializeEnvelope(Serializer);

            var message = GooglePubSubConnector.MessageAttributesToPubsubMessage(messageEnvelope);

            _ = await connector.PublisherApi.PublishAsync(topicName, [message], cancellationToken: token).ConfigureAwait(false);
        }
        finally
        {
            CancellationTokenSourceFactory = null;
        }
    }
}
