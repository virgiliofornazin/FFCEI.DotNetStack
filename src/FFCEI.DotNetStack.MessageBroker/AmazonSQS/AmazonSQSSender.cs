using Amazon.SQS.Model;
using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.MessageBroker.Common;

namespace FFCEI.DotNetStack.MessageBroker.AmazonSQS;

public sealed class AmazonSQSSender(string subject, WeakReference<AmazonSQSConnector> connector)
    : Sender(MessageBrokerProviderType.AmazonSQS, subject)
{
    public WeakReference<AmazonSQSConnector> Connector { get; private set; } = connector;

    protected override async Task DoSendToAsync(MessageBrokerAddress address, MessageEnvelope messageEnvelope, CancellationToken cancellationToken = default)
    {
        var hasConnector = Connector.TryGetTarget(out var connector) && (connector != null);

        ObjectDisposedException.ThrowIf(!hasConnector, this);

        using var cancellationTokenSourceFactory = new CancellationTokenSourceFactory(cancellationToken);

        var token = cancellationTokenSourceFactory.Token;

        try
        {
            CancellationTokenSourceFactory = cancellationTokenSourceFactory;

            var queueUrl = await connector!.GetQueueUrlAsync(address, token).ConfigureAwait(false);

            messageEnvelope.SerializeEnvelope(Serializer);

            var request = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = Convert.ToBase64String(messageEnvelope.PayloadBytes ?? []),
                MessageAttributes = AmazonSQSConnector.MessageAttributesToSQSAttributes(messageEnvelope)
            };

            _ = await connector.Client.SendMessageAsync(request, token).ConfigureAwait(false);
        }
        finally
        {
            CancellationTokenSourceFactory = null;
        }
    }
}
