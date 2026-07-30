using Confluent.Kafka;
using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.MessageBroker.Common;

namespace FFCEI.DotNetStack.MessageBroker.Kafka;

public sealed class KafkaSender(IProducer<Null, byte[]> producer, string subject, WeakReference<KafkaConnector> connector)
    : Sender(MessageBrokerProviderType.Kafka, subject)
{
    public WeakReference<KafkaConnector> Connector { get; private set; } = connector;
    public IProducer<Null, byte[]> Producer { get; private set; } = producer;

    protected override async Task DoSendToAsync(MessageBrokerAddress address, MessageEnvelope messageEnvelope, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(Producer, nameof(Producer));

        var topic = KafkaConnector.ExtractKakfaTopicFromAddress(address);
        var headers = KafkaConnector.MessageAttributesToKafkaHeaders(messageEnvelope);

        using var cancellationTokenSourceFactory = new CancellationTokenSourceFactory(cancellationToken);

        var token = cancellationTokenSourceFactory.Token;

        try
        {
            CancellationTokenSourceFactory = cancellationTokenSourceFactory;

            messageEnvelope.SerializeEnvelope(Serializer);

            var kafkaMessage = new Message<Null, byte[]>
            {
                Value = messageEnvelope.PayloadBytes!,
                Headers = headers
            };

            _ = await Producer.ProduceAsync(topic, kafkaMessage, token).ConfigureAwait(false);
        }
        finally
        {
            CancellationTokenSourceFactory = null;
        }
    }

    protected override void OnDispose()
         => ExceptionProtector.ExecuteAction(() =>
         {
             var hasConnector = Connector.TryGetTarget(out var connector) && connector != null;

             if (hasConnector)
             {
                 connector!.CloseProducerAsync(Producer).GetAwaiter().GetResult();
             }
         });

    protected override ValueTask OnDisposeAsync()
         => ExceptionProtector.ExecuteValueTaskFunctionAsync(async () =>
            {
                var hasConnector = Connector.TryGetTarget(out var connector) && connector != null;

                if (hasConnector)
                {
                    await connector!.CloseProducerAsync(Producer).ConfigureAwait(false);
                }
            });
}
