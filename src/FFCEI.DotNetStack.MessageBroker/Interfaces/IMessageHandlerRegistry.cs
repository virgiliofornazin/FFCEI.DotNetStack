namespace FFCEI.DotNetStack.MessageBroker.Interfaces;

public interface IMessageHandlerRegistry
    : IMessageHandlerCallback, IMessageCounter
{
    void AddMessageHandler(IMessageEnvelopeHandler handler);

    void AddMessageHandler<THandler>() where THandler : IMessageEnvelopeHandler;

    bool TryAddMessageHandler(IMessageEnvelopeHandler handler);

    bool TryAddMessageHandler<THandler>() where THandler : IMessageEnvelopeHandler;

    bool CanHandleMessageOFType(Type messageType);
}
