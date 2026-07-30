using System.Diagnostics.CodeAnalysis;

namespace FFCEI.DotNetStack.Secrets;

[Serializable]
public sealed class SecretNotFoundException
    : Exception
{
    internal const string DefaultMessage = "Secret not found";

    public string SecretName { get; private set; } = "";

    [DoesNotReturn]
    public static void Throw(string secretName)
    {
        var exception = new SecretNotFoundException($"Secret {secretName} not found")
        {
            SecretName = secretName
        };

        throw exception;
    }

    public SecretNotFoundException()
        : base(DefaultMessage)
    {
    }

    public SecretNotFoundException(string message)
        : base(message)
    {
    }

    public SecretNotFoundException(Exception innerException)
        : base(DefaultMessage, innerException)
    {
    }

    public SecretNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
