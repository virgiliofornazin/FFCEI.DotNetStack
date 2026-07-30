namespace FFCEI.DotNetStack.MessageBroker;

[Serializable]
public class MessageBrokerConnectorFactoryException
    : Exception
{
    protected MessageBrokerConnectorFactoryException()
        : base()
    {
    }

    public MessageBrokerConnectorFactoryException(string? message)
        : base(message)
    {
    }

    public MessageBrokerConnectorFactoryException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }
}
