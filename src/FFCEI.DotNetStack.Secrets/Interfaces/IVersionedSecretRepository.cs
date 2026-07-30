namespace FFCEI.DotNetStack.Secrets.Interfaces;

public interface IVersionedSecretRepository
    : ISecretRepository
{
    Task<IReadOnlyCollection<SecretValueVersion>> ListVersionsAsync(string secretName, CancellationToken cancellationToken = default);
    Task<string?> GetSecretByVersionAsync(string secretName, string version, CancellationToken cancellationToken = default);
    Task<T?> GetJsonSecretByVersionAsync<T>(string secretName, string version, CancellationToken cancellationToken = default) where T : class;
}
