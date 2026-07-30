namespace FFCEI.DotNetStack.MessageBroker;

public enum MessageBrokerProviderType
{
    Unknown = 0,
    Channels = 1,
    RabbitMQ = 2,
    Kafka = 3,
    Qpid = 4,
    AzureServiceBus = 5,
    GooglePubSub = 6,
    AmazonSQS = 7,
    AmazonSQSwithSNS = 8,
}
