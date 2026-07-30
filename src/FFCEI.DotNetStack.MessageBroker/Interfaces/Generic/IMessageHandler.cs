namespace FFCEI.DotNetStack.MessageBroker.Interfaces.Generic;

public interface IMessageHandler<TMessage>
    : IMessageEnvelopeHandler
{
    Task HandleMessageAsync(TMessage message, IMessageEnvelopeAttributes envelopeAttributes, CancellationToken cancellationToken);
}
