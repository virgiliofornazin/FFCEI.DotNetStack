using FFCEI.DotNetStack.Serialization.Interfaces;

namespace FFCEI.DotNetStack.MessageBroker.Interfaces;

public interface IReceiver
    : IMessageCounter, IMessageBrokerProvider, IMessageHandlerRegistry, IDisposable, IAsyncDisposable
{
    MessageBrokerAddress? Address { get; }
    IDeserializer? Deserializer { get; set; }

    Task StartAsync(CancellationToken cancellationToken = default);

    Task StopAsync();
}
