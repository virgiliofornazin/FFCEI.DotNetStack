using FFCEI.DotNetStack.Secrets.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace FFCEI.DotNetStack.Secrets;

[Serializable]
public sealed class SecretManagerRepositoryException
    : Exception
{
    internal const string DefaultMessage = "Secret Manager Repository failure";

    public ISecretManagerRepository SecretManagerRepository { get; private set; } = null!;

    [DoesNotReturn]
    public static void Throw(ISecretManagerRepository secretManagerRepository, Exception innerException)
    {
        ArgumentNullException.ThrowIfNull(secretManagerRepository, nameof(secretManagerRepository));

        var exception = new SecretManagerRepositoryException($"Secret Manager Mepository {secretManagerRepository.GetType()} internal exception", innerException)
        {
            SecretManagerRepository = secretManagerRepository
        };

        throw exception;
    }

    [DoesNotReturn]
    public static T Throw<T>(ISecretManagerRepository secretManagerRepository, Exception innerException)
    {
        Throw(secretManagerRepository, innerException);

        return default!;
    }

    public SecretManagerRepositoryException()
        : base(DefaultMessage)
    {
    }

    public SecretManagerRepositoryException(string message)
        : base(message)
    {
    }

    public SecretManagerRepositoryException(Exception innerException)
        : base(DefaultMessage, innerException)
    {
    }

    public SecretManagerRepositoryException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
