namespace FFCEI.DotNetStack.Secrets.Interfaces;

public interface ISecretManagerRepository
    : IVersionedSecretRepository
{
    SecretManagerProviderType SecretManagerProvider { get; }

    Task<SecretResult> SetSecretAsync(string secretName, string value, CancellationToken cancellationToken = default);
    Task DeleteSecretAsync(string secretName, CancellationToken cancellationToken = default);
}
