using System.Diagnostics.CodeAnalysis;

namespace FFCEI.DotNetStack.Secrets;

[Serializable]
public sealed class SecretAccessDeniedException
    : Exception
{
    internal const string DefaultMessage = "Secret access was denied";

    public string SecretName { get; private set; } = "";

    [DoesNotReturn]
    public static void Throw(string secretName, Exception innerException)
    {
        var exception = new SecretAccessDeniedException($"Secret {secretName} access was defined", innerException)
        {
            SecretName = secretName
        };

        throw exception;
    }

    public SecretAccessDeniedException()
        : base(DefaultMessage)
    {
    }

    public SecretAccessDeniedException(string message)
        : base(message)
    {
    }

    public SecretAccessDeniedException(Exception innerException)
        : base(DefaultMessage, innerException)
    {
    }

    public SecretAccessDeniedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
