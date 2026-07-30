namespace FFCEI.DotNetStack.MessageBroker.Interfaces;

public interface IMessageHandlerCallback
{
    IMessageHandlerCallback ConfigureCallbacks(Action<IMessageHandlerCallback> expression);

    Action<IMessageEnvelope>? OnUnknownMessageReceived { get; set; }
    Action<IMessageEnvelope>? OnUnhandledMessage { get; set; }
    Func<Exception, IMessageEnvelope?, bool>? OnMessageHandlingException { get; set; }
}
