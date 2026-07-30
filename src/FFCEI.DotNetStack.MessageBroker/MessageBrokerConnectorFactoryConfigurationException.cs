namespace FFCEI.DotNetStack.MessageBroker;

[Serializable]
public sealed class MessageBrokerConnectorFactoryConfigurationException
    : MessageBrokerConnectorFactoryException
{
    internal const string DefaultMessage = "Message Broker Factory Configuration failure";

    public MessageBrokerConnectorFactoryConfigurationException()
        : base(DefaultMessage)
    {
    }

    public MessageBrokerConnectorFactoryConfigurationException(string message)
        : base(message)
    {
    }

    public MessageBrokerConnectorFactoryConfigurationException(Exception innerException)
        : base(DefaultMessage, innerException)
    {
    }

    public MessageBrokerConnectorFactoryConfigurationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
