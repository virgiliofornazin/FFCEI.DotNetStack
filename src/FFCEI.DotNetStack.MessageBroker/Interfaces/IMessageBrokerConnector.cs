using FFCEI.DotNetStack.Serialization.Interfaces;

namespace FFCEI.DotNetStack.MessageBroker.Interfaces;

public interface IMessageBrokerConnector
    : IDisposable, IAsyncDisposable
{
    public MessageBrokerProviderType MessageBrokerProvider { get; }

    bool Started();

    Task StartAsync(Uri uri, CancellationToken cancellationToken = default);
    Task StartAsync(string connectionString, CancellationToken cancellationToken = default);
    Task StopAsync();
    Task<ISender> GetSenderAsync(string subject, MessageBrokerAddress? defaultAddress = null, ISerializer? serializer = null, CancellationToken cancellationToken = default);
    Task<IReceiver> GetReceiverAsync(string subject, IDeserializer? deserializer = null, CancellationToken cancellationToken = default);
}
