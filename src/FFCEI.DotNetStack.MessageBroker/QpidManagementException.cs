namespace FFCEI.DotNetStack.MessageBroker;

[Serializable]
public sealed class QpidManagementException
    : Exception
{
    internal const string DefaultMessage = "Qpid QMF management request failed";

    public QpidManagementException()
        : base(DefaultMessage)
    {
    }

    public QpidManagementException(string message)
        : base(message)
    {
    }

    public QpidManagementException(Exception innerException)
        : base(DefaultMessage, innerException)
    {
    }

    public QpidManagementException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
