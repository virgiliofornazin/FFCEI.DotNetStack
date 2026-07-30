using FFCEI.DotNetStack.Secrets.Common;
using Microsoft.Extensions.Logging;
using Oci.Common.Model;
using Oci.SecretsService;
using Oci.SecretsService.Models;
using Oci.SecretsService.Requests;
using Oci.VaultService;
using Oci.VaultService.Models;
using Oci.VaultService.Requests;
using System.Globalization;
using System.Net;
using System.Text;

namespace FFCEI.DotNetStack.Secrets.OCI;

public sealed class OCISecretManagerRepository
    : SecretManagerRepository
{
    private readonly SecretsClient _secretsClient;
    private readonly VaultsClient _vaultsClient;
    private readonly string _compartmentId;
    private readonly string _vaultId;
    private readonly string? _keyId;

    public OCISecretManagerRepository(SecretsClient secretsClient, VaultsClient vaultsClient, string compartmentId, string vaultId, string? keyId = null, ILogger<OCISecretManagerRepository>? logger = null)
        : base(SecretManagerProviderType.OCI, logger)
    {
        ArgumentNullException.ThrowIfNull(secretsClient, nameof(secretsClient));
        ArgumentNullException.ThrowIfNull(vaultsClient, nameof(vaultsClient));
        ArgumentException.ThrowIfNullOrWhiteSpace(compartmentId, nameof(compartmentId));
        ArgumentException.ThrowIfNullOrWhiteSpace(vaultId, nameof(vaultId));

        _secretsClient = secretsClient;
        _vaultsClient = vaultsClient;
        _compartmentId = compartmentId;
        _vaultId = vaultId;
        _keyId = keyId;
    }

    protected override void MapProviderException(string secretName, Exception exception)
    {
        if (exception is OciException ociException)
        {
            if (ociException.StatusCode == HttpStatusCode.NotFound)
            {
                SecretNotFoundException.Throw(secretName);
            }

            if (ociException.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                SecretAccessDeniedException.Throw(secretName, ociException);
            }
        }
    }

    private async Task<string?> FindSecretIdAsync(string secretName, CancellationToken cancellationToken)
    {
        string? page = null;

        do
        {
            var request = new ListSecretsRequest
            {
                CompartmentId = _compartmentId,
                VaultId = _vaultId,
                Name = secretName,
                Page = page
            };

            var response = await _vaultsClient.ListSecrets(request, cancellationToken: cancellationToken).ConfigureAwait(false);

            var secret = response.Items.FirstOrDefault(item => item.SecretName == secretName &&
                item.LifecycleState != SecretSummary.LifecycleStateEnum.Deleted &&
                item.LifecycleState != SecretSummary.LifecycleStateEnum.PendingDeletion);

            if (secret != null)
            {
                return secret.Id;
            }

            page = response.OpcNextPage;
        }
        while (!string.IsNullOrEmpty(page));

        return null;
    }

    public override async Task<IReadOnlyCollection<SecretValueVersion>> ListVersionsAsync(string secretName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretName, nameof(secretName));

        SecretsLog.ListingSecretVersions(Logger, SecretManagerProvider, secretName);

        try
        {
            var secretId = await FindSecretIdAsync(secretName, cancellationToken).ConfigureAwait(false);

            if (secretId == null)
            {
                SecretNotFoundException.Throw(secretName);
            }

            var result = new List<SecretValueVersion>();

            string? page = null;

            do
            {
                var request = new ListSecretVersionsRequest
                {
                    SecretId = secretId,
                    Page = page
                };

                var response = await _vaultsClient.ListSecretVersions(request, cancellationToken: cancellationToken).ConfigureAwait(false);

                result.AddRange(response.Items.Select(version => new SecretValueVersion()
                {
                    Version = version.VersionNumber?.ToString(CultureInfo.InvariantCulture) ?? "0",
                    IsCurrent = version.Stages?.Contains(SecretVersionSummary.StagesEnum.Current) ?? false,
                    CreationTimestamp = version.TimeCreated != null ? new DateTimeOffset(version.TimeCreated.Value) : DateTimeOffset.MinValue,
                    Status = version.Stages == null ? null : string.Join(", ", version.Stages)
                }));

                page = response.OpcNextPage;
            }
            while (!string.IsNullOrEmpty(page));

            if (result.Count == 0)
            {
                SecretNotFoundException.Throw(secretName);
            }

            KeepOnlyNewestVersionAsCurrentIfNeeded(result);

            SecretsLog.EnumeratedSecretVersions(Logger, SecretManagerProvider, secretName, result.Count);

            return result.AsReadOnly();
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
            var request = new GetSecretBundleByNameRequest
            {
                SecretName = secretName,
                VaultId = _vaultId
            };

            if (!string.IsNullOrWhiteSpace(version))
            {
                request.VersionNumber = long.Parse(version, CultureInfo.InvariantCulture);
            }

            var response = await _secretsClient.GetSecretBundleByName(request, cancellationToken: cancellationToken).ConfigureAwait(false);

            SecretsLog.ReadSecret(Logger, SecretManagerProvider, secretName, version ?? "latest");

            return response.SecretBundle.SecretBundleContent is Base64SecretBundleContentDetails base64Content ?
                Encoding.UTF8.GetString(Convert.FromBase64String(base64Content.Content)) : null;
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
            var secretId = await FindSecretIdAsync(secretName, cancellationToken).ConfigureAwait(false);

            if (secretId == null)
            {
                SecretNotFoundException.Throw(secretName);
            }

            var request = new ScheduleSecretDeletionRequest
            {
                SecretId = secretId,
                ScheduleSecretDeletionDetails = new ScheduleSecretDeletionDetails()
            };

            _ = await _vaultsClient.ScheduleSecretDeletion(request, cancellationToken: cancellationToken).ConfigureAwait(false);

            SecretsLog.DeletedSecret(Logger, SecretManagerProvider, secretName);
        }
        catch (SecretNotFoundException)
        {
            throw;
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
            var secretContent = new Base64SecretContentDetails
            {
                Content = Convert.ToBase64String(Encoding.UTF8.GetBytes(value))
            };

            var secretId = await FindSecretIdAsync(secretName, cancellationToken).ConfigureAwait(false);

            if (secretId == null)
            {
                if (string.IsNullOrWhiteSpace(_keyId))
                {
                    throw new SecretManagerRepositoryException($"Secret '{secretName}' does not exist and no KMS key id was provided to create it");
                }

                var createRequest = new CreateSecretRequest
                {
                    CreateSecretDetails = new CreateSecretDetails
                    {
                        CompartmentId = _compartmentId,
                        VaultId = _vaultId,
                        KeyId = _keyId,
                        SecretName = secretName,
                        SecretContent = secretContent
                    }
                };

                var createResponse = await _vaultsClient.CreateSecret(createRequest, cancellationToken: cancellationToken).ConfigureAwait(false);

                SecretsLog.WroteSecret(Logger, SecretManagerProvider, secretName);

                return new SecretResult()
                {
                    SecretName = secretName,
                    CurrentVersion = createResponse.Secret.CurrentVersionNumber?.ToString(CultureInfo.InvariantCulture) ?? "latest",
                    PreviousVersion = null
                };
            }

            var getRequest = new GetSecretRequest()
            {
                SecretId = secretId
            };

            var previousResponse = await _vaultsClient.GetSecret(getRequest, cancellationToken: cancellationToken).ConfigureAwait(false);
            var previousVersion = previousResponse?.Secret.CurrentVersionNumber?.ToString(CultureInfo.InvariantCulture);

            var updateRequest = new UpdateSecretRequest
            {
                SecretId = secretId,
                UpdateSecretDetails = new UpdateSecretDetails
                {
                    SecretContent = secretContent
                }
            };

            var updateResponse = await _vaultsClient.UpdateSecret(updateRequest, cancellationToken: cancellationToken).ConfigureAwait(false);

            SecretsLog.WroteSecret(Logger, SecretManagerProvider, secretName);

            return new SecretResult()
            {
                SecretName = secretName,
                CurrentVersion = updateResponse.Secret.CurrentVersionNumber?.ToString(CultureInfo.InvariantCulture) ?? "latest",
                PreviousVersion = previousVersion
            };
        }
        catch (SecretManagerRepositoryException)
        {
            throw;
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            return ThrowMappedProviderException<SecretResult>(SecretManagerOperationType.Write, secretName, exception);
        }
    }
}
