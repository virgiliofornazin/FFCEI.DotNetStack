using FFCEI.DotNetStack.Serialization.Interfaces;

namespace FFCEI.DotNetStack.MessageBroker.Interfaces;

public interface ISender
    : IMessageCounter, IMessageBrokerProvider, IDisposable, IAsyncDisposable
{
    MessageBrokerAddress? DefaultAddress { get; set; }
    ISerializer? Serializer { get; set; }
    Action<string>? OnMessagePublishReturned { get; set; }

    void Send(object? message, IMessageAttributes? messageAttributes = null);
    void SendTo(MessageBrokerAddress address, object? message, IMessageAttributes? messageAttributes = null);
    Task SendAsync(object? message, IMessageAttributes? messageAttributes = null, CancellationToken cancellationToken = default);
    Task SendToAsync(MessageBrokerAddress address, object? message, IMessageAttributes? messageAttributes = null, CancellationToken cancellationToken = default);
}
