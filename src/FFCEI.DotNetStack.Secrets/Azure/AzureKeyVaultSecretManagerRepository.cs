using Azure;
using Azure.Security.KeyVault.Secrets;
using FFCEI.DotNetStack.Secrets.Common;
using FFCEI.DotNetStack.Framework;
using Microsoft.Extensions.Logging;

namespace FFCEI.DotNetStack.Secrets.Azure;

public sealed class AzureKeyVaultSecretManagerRepository
    : SecretManagerRepository
{
    private readonly SecretClient _client;

    public AzureKeyVaultSecretManagerRepository(SecretClient client, ILogger<AzureKeyVaultSecretManagerRepository>? logger = null)
        : base(SecretManagerProviderType.Azure, logger)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));

        _client = client;
    }

    protected override void MapProviderException(string secretName, Exception exception)
    {
        if (exception is RequestFailedException requestFailedException)
        {
            if (requestFailedException.Status == 404)
            {
                SecretNotFoundException.Throw(secretName);
            }

            if (requestFailedException.Status == 403)
            {
                SecretAccessDeniedException.Throw(secretName, exception);
            }
        }
    }

    public override async Task<IReadOnlyCollection<SecretValueVersion>> ListVersionsAsync(string secretName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretName, nameof(secretName));

        SecretsLog.ListingSecretVersions(Logger, SecretManagerProvider, secretName);

        try
        {
            var versions = _client.GetPropertiesOfSecretVersionsAsync(secretName, cancellationToken);

            var result = new List<SecretValueVersion>();

            await foreach (var version in versions.WithCancellation(cancellationToken))
            {
                result.Add(new()
                {
                    Version = version.Version ?? "latest",
                    IsCurrent = version.Enabled == true,
                    CreationTimestamp = version.CreatedOn ?? DateTimeOffset.MinValue,
                    Status = version.Enabled == true ? "Enabled" : "Disabled"
                });
            }

            if (result.Count == 0)
            {
                SecretNotFoundException.Throw(secretName);
            }

            KeepOnlyNewestVersionAsCurrentIfNeeded(result);

            SecretsLog.EnumeratedSecretVersions(Logger, SecretManagerProvider, secretName, result.Count);

            return result;
        }
        catch (SecretNotFoundException)
        {
            throw;
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            return ThrowMappedProviderException<IReadOnlyCollection<SecretValueVersion>>(SecretManagerOperationType.ListVersions, secretName, exception);
        }
    }

    protected override async Task<string?> DoGetSecretByVersionAsync(string secretName, string? version, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretName, nameof(secretName));

        SecretsLog.ReadingSecret(Logger, SecretManagerProvider, secretName, version ?? "latest");

        try
        {
            var task = string.IsNullOrWhiteSpace(version) ?
                _client.GetSecretAsync(secretName, cancellationToken: cancellationToken) :
                _client.GetSecretAsync(secretName, version, cancellationToken);

            var response = await task.ConfigureAwait(false);

            SecretsLog.ReadSecret(Logger, SecretManagerProvider, secretName, version ?? "latest");

            return response.Value.Value;
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            return ThrowMappedProviderException<string?>(SecretManagerOperationType.Read, secretName, exception);
        }
    }

    public override async Task DeleteSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretName, nameof(secretName));

        SecretsLog.DeletingSecret(Logger, SecretManagerProvider, secretName);

        try
        {
            var operation = await _client.StartDeleteSecretAsync(secretName, cancellationToken).ConfigureAwait(false);
            var response = await operation.WaitForCompletionAsync(cancellationToken).ConfigureAwait(false);

            SecretsLog.DeletedSecret(Logger, SecretManagerProvider, secretName);
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            ThrowMappedProviderException(SecretManagerOperationType.Delete, secretName, exception);
        }
    }

    public override async Task<SecretResult> SetSecretAsync(string secretName, string value, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretName, nameof(secretName));
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));

        SecretsLog.WritingSecret(Logger, SecretManagerProvider, secretName);

        try
        {
            var previousVersions = await ExceptionProtector.ExecuteTaskAndReturnOrNullReference(_client.GetSecretAsync(secretName, cancellationToken: cancellationToken)).ConfigureAwait(false);
            var previousVersion = previousVersions?.Value.Properties.Version;

            var response = await _client.SetSecretAsync(secretName, value, cancellationToken).ConfigureAwait(false);
            var currentVersion = response.Value.Properties.Version ?? "latest";

            var result = new SecretResult()
            {
                SecretName = secretName,
                CurrentVersion = currentVersion,
                PreviousVersion = previousVersion
            };

            SecretsLog.WroteSecret(Logger, SecretManagerProvider, secretName);

            return result;
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            return ThrowMappedProviderException<SecretResult>(SecretManagerOperationType.Write, secretName, exception);
        }
    }
}
