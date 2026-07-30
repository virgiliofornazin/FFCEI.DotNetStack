using Amazon.SimpleNotificationService.Model;
using Amazon.SQS.Model;
using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.MessageBroker.AmazonSQS;
using FFCEI.DotNetStack.MessageBroker.Common;

namespace FFCEI.DotNetStack.MessageBroker.AmazonSQSwithSNS;

public sealed class AmazonSQSwithSNSSender(string subject, WeakReference<AmazonSQSwithSNSConnector> connector)
    : Sender(MessageBrokerProviderType.AmazonSQSwithSNS, subject)
{
    public WeakReference<AmazonSQSwithSNSConnector> Connector { get; private set; } = connector;

    protected override async Task DoSendToAsync(MessageBrokerAddress address, MessageEnvelope messageEnvelope, CancellationToken cancellationToken = default)
    {
        var hasConnector = Connector.TryGetTarget(out var connector) && (connector != null);

        ObjectDisposedException.ThrowIf(!hasConnector, this);

        using var cancellationTokenSourceFactory = new CancellationTokenSourceFactory(cancellationToken);

        var token = cancellationTokenSourceFactory.Token;

        try
        {
            CancellationTokenSourceFactory = cancellationTokenSourceFactory;

            if (AmazonSQSwithSNSConnector.IsTopicAddress(address))
            {
                await SendToTopicAsync(connector!, address, messageEnvelope, token).ConfigureAwait(false);
            }
            else
            {
                await SendToQueueAsync(connector!, address, messageEnvelope, token).ConfigureAwait(false);
            }
        }
        finally
        {
            CancellationTokenSourceFactory = null;
        }
    }

    private async Task SendToTopicAsync(AmazonSQSwithSNSConnector connector, MessageBrokerAddress address, MessageEnvelope messageEnvelope, CancellationToken cancellationToken)
    {
        var topicArn = await connector.GetTopicArnAsync(AmazonSQSwithSNSConnector.ExtractTopicName(address), cancellationToken).ConfigureAwait(false);

        messageEnvelope.SerializeEnvelope(Serializer);

        var request = new PublishRequest
        {
            TopicArn = topicArn,
            Message = Convert.ToBase64String(messageEnvelope.PayloadBytes ?? []),
            MessageAttributes = AmazonSQSwithSNSConnector.MessageAttributesToSNSAttributes(messageEnvelope)
        };

        _ = await connector.SnsClient.PublishAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private async Task SendToQueueAsync(AmazonSQSwithSNSConnector connector, MessageBrokerAddress address, MessageEnvelope messageEnvelope, CancellationToken cancellationToken)
    {
        var queueUrl = await connector.GetQueueUrlAsync(AmazonSQSwithSNSConnector.ExtractQueueName(address), cancellationToken).ConfigureAwait(false);

        messageEnvelope.SerializeEnvelope(Serializer);

        var request = new SendMessageRequest
        {
            QueueUrl = queueUrl,
            MessageBody = Convert.ToBase64String(messageEnvelope.PayloadBytes ?? []),
            MessageAttributes = AmazonSQSConnector.MessageAttributesToSQSAttributes(messageEnvelope)
        };

        _ = await connector.SqsClient.SendMessageAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
