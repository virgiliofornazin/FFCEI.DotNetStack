namespace FFCEI.DotNetStack.MessageBroker.Interfaces;

public interface IMessageBrokerConnectorFactory
    : IMessageHandlerCallback, IDisposable, IAsyncDisposable
{
    Task<IMessageBrokerConnector> GetConnectorByConnectionIdAsync(string? connectionId, CancellationToken cancellationToken = default);
    Task<IMessageBrokerConnector> GetConnectorAsync(string subject, CancellationToken cancellationToken = default);
    Task<IMessageBrokerConnector> GetDefaultConnectorAsync(CancellationToken cancellationToken = default);
    Task<ISender> GetSenderAsync(string subject, MessageBrokerAddress? defaultAddress = null, CancellationToken cancellationToken = default);
    Task<ISender> GetDefaultSenderAsync(MessageBrokerAddress? defaultAddress = null, CancellationToken cancellationToken = default);
    Task<IReceiver> GetReceiverAsync(string subject, CancellationToken cancellationToken = default);
    Task<IReceiver> GetDefaultReceiverAsync(CancellationToken cancellationToken = default);
}
