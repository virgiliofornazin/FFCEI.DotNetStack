namespace FFCEI.DotNetStack.Secrets.Interfaces;

public interface ISecretRepository
{
    Task<string?> GetSecretAsync(string secretName, CancellationToken cancellationToken = default);
    Task<T?> GetJsonSecretAsync<T>(string secretName, CancellationToken cancellationToken = default) where T : class;
    Task<IReadOnlyCollection<KeyValuePair<string, string?>>> GetSecretsAsync(IEnumerable<string> secretNames, CancellationToken cancellationToken = default);
}
