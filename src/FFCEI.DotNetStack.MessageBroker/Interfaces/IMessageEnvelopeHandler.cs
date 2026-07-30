namespace FFCEI.DotNetStack.MessageBroker.Interfaces;

public interface IMessageEnvelopeHandler
    : IMessageCounter
{
    Type MessageType { get; }

    Task HandleAsync(IMessageEnvelope messageEnvelope, CancellationToken cancellationToken);
}
