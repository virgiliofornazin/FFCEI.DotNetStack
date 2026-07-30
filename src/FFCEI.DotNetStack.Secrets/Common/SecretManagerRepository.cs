using FFCEI.DotNetStack.Secrets.Interfaces;
using FFCEI.DotNetStack.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics.CodeAnalysis;

namespace FFCEI.DotNetStack.Secrets.Common;

public abstract class SecretManagerRepository
    : ISecretManagerRepository
{
    public virtual SecretManagerProviderType SecretManagerProvider { get; protected set; }
    public ILogger Logger { get; internal set; } = NullLogger.Instance;

    protected SecretManagerRepository(ILogger? logger = null)
    {
        Logger = logger ?? NullLogger.Instance;
    }

    protected SecretManagerRepository(SecretManagerProviderType secretManagerProvider, ILogger? logger = null)
    {
        SecretManagerProvider = secretManagerProvider;
        Logger = logger ?? NullLogger.Instance;
    }

    [DoesNotReturn]
    protected void ThrowMappedProviderException(SecretManagerOperationType operation, string secretName, Exception exception)
    {
        SecretsLog.SecretOperationFailed(Logger, exception, SecretManagerProvider, operation, secretName);

        MapProviderException(secretName, exception);

        SecretManagerRepositoryException.Throw(this, exception);
    }

    [DoesNotReturn]
    protected T ThrowMappedProviderException<T>(SecretManagerOperationType operation, string secretName, Exception exception)
    {
        ThrowMappedProviderException(operation, secretName, exception);

        return default!;
    }

    protected abstract void MapProviderException(string secretName, Exception exception);

    protected static void KeepOnlyNewestVersionAsCurrentIfNeeded(IReadOnlyCollection<SecretValueVersion> versions)
    {
        ArgumentNullException.ThrowIfNull(versions, nameof(versions));

        var staleCurrentVersions = versions
            .Where(version => version.IsCurrent)
            .OrderByDescending(version => version.CreationTimestamp)
            .Skip(1);

        foreach (var version in staleCurrentVersions)
        {
            version.IsCurrent = false;
        }
    }

    public virtual Task<string?> GetSecretAsync(string secretName, CancellationToken cancellationToken = default)
        => DoGetSecretByVersionAsync(secretName, null, cancellationToken);

    public virtual async Task<T?> GetJsonSecretAsync<T>(string secretName, CancellationToken cancellationToken = default) where T : class
    {
        var value = await GetSecretAsync(secretName, cancellationToken).ConfigureAwait(false);
        var json = value == null ? default : JsonDeserializer.Default.Deserialize<T>(value);

        return json;
    }

    public virtual async Task<IReadOnlyCollection<KeyValuePair<string, string?>>> GetSecretsAsync(IEnumerable<string> secretNames, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(secretNames, nameof(secretNames));

        var results = new List<KeyValuePair<string, string?>>();

        foreach (var name in secretNames)
        {
            var value = await GetSecretAsync(name, cancellationToken).ConfigureAwait(false);

            results.Add(KeyValuePair.Create(name, value));
        }

        return results.AsReadOnly();
    }

    public abstract Task<IReadOnlyCollection<SecretValueVersion>> ListVersionsAsync(string secretName, CancellationToken cancellationToken = default);

    protected abstract Task<string?> DoGetSecretByVersionAsync(string secretName, string? version, CancellationToken cancellationToken = default);

    public virtual Task<string?> GetSecretByVersionAsync(string secretName, string version, CancellationToken cancellationToken = default)
        => DoGetSecretByVersionAsync(secretName, version, cancellationToken);

    public virtual async Task<T?> GetJsonSecretByVersionAsync<T>(string secretName, string version, CancellationToken cancellationToken = default) where T : class
    {
        var value = await GetSecretByVersionAsync(secretName, version, cancellationToken).ConfigureAwait(false);
        var json = value == null ? default : JsonDeserializer.Default.Deserialize<T>(value);

        return json;
    }

    public abstract Task DeleteSecretAsync(string secretName, CancellationToken cancellationToken = default);

    public abstract Task<SecretResult> SetSecretAsync(string secretName, string value, CancellationToken cancellationToken = default);
}
