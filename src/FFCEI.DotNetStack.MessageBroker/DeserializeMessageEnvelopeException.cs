namespace FFCEI.DotNetStack.MessageBroker;

[Serializable]
public sealed class DeserializeMessageEnvelopeException
    : Exception
{
    internal const string DefaultMessage = "Exception deserializing message envelope";

    public DeserializeMessageEnvelopeException()
        : base(DefaultMessage)
    {
    }

    public DeserializeMessageEnvelopeException(string message)
        : base(message)
    {
    }

    public DeserializeMessageEnvelopeException(Exception innerException)
        : base(DefaultMessage, innerException)
    {
    }

    public DeserializeMessageEnvelopeException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
