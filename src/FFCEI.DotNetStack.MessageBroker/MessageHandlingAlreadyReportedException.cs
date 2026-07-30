namespace FFCEI.DotNetStack.MessageBroker;

[Serializable]
public sealed class MessageHandlingAlreadyReportedException
    : Exception
{
    internal const string DefaultMessage = "Message handling exception already reported via OnMessageHandlingException";

    public MessageHandlingAlreadyReportedException()
        : base(DefaultMessage)
    {
    }

    public MessageHandlingAlreadyReportedException(string message)
        : base(message)
    {
    }

    public MessageHandlingAlreadyReportedException(Exception innerException)
        : base(DefaultMessage, innerException)
    {
    }

    public MessageHandlingAlreadyReportedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
