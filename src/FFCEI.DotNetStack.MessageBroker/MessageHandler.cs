using FFCEI.DotNetStack.MessageBroker.Common;
using FFCEI.DotNetStack.MessageBroker.Interfaces;
using FFCEI.DotNetStack.MessageBroker.Interfaces.Generic;

namespace FFCEI.DotNetStack.MessageBroker;

public abstract class MessageHandler<TMessage>
    : MessageCounter, IMessageHandler<TMessage>
{
    public Type MessageType => typeof(TMessage);

    public abstract Task HandleMessageAsync(TMessage message, IMessageEnvelopeAttributes envelopeAttributes, CancellationToken cancellationToken);

    public async Task HandleAsync(IMessageEnvelope messageEnvelope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(messageEnvelope, nameof(messageEnvelope));

        if (messageEnvelope.Payload is not TMessage message)
        {
            ArgumentNullException.ThrowIfNull(messageEnvelope.Payload, nameof(messageEnvelope.Payload));

            throw new InvalidCastException($"Cannot cast message payload of type {messageEnvelope.Payload.GetType()}  to type {typeof(TMessage)}");
        }

        await HandleMessageAsync(message, messageEnvelope.Attributes, cancellationToken).ConfigureAwait(false);

        IncrementMessageCount();
    }
}
