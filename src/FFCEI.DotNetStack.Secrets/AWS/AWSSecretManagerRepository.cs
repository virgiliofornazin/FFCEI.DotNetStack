using Amazon.Runtime;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using FFCEI.DotNetStack.Secrets.Common;
using FFCEI.DotNetStack.Framework;
using Microsoft.Extensions.Logging;

namespace FFCEI.DotNetStack.Secrets.AWS;

public sealed class AWSSecretManagerRepository
    : SecretManagerRepository
{
    private readonly IAmazonSecretsManager _client;

    public AWSSecretManagerRepository(IAmazonSecretsManager client, ILogger<AWSSecretManagerRepository>? logger = null)
        : base(SecretManagerProviderType.AWS, logger)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));

        _client = client;
    }

    protected override void MapProviderException(string secretName, Exception exception)
    {
        if (exception is ResourceNotFoundException)
        {
            SecretNotFoundException.Throw(secretName);
        }
        else if (exception is AmazonServiceException amazonServiceException)
        {
            if (amazonServiceException.ErrorCode == "AccessDeniedException")
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
            var request = new ListSecretVersionIdsRequest
            {
                SecretId = secretName
            };

            var response = await _client.ListSecretVersionIdsAsync(request, cancellationToken).ConfigureAwait(false);

            var result = (response.Versions ?? []).Select(version => new SecretValueVersion()
            {
                Version = version.VersionId,
                IsCurrent = version.VersionStages?.Contains("AWSCURRENT") ?? false,
                CreationTimestamp = version.CreatedDate != null ? new DateTimeOffset(version.CreatedDate.Value) : DateTimeOffset.MinValue,
                Status = version.VersionStages != null ? string.Join(", ", version.VersionStages) : string.Empty
            })
            .ToList().AsReadOnly();

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
            var request = new GetSecretValueRequest()
            {
                SecretId = secretName
            };

            if (!string.IsNullOrWhiteSpace(version))
            {
                request.VersionId = version;
            }

            var response = await _client.GetSecretValueAsync(request, cancellationToken).ConfigureAwait(false);

            SecretsLog.ReadSecret(Logger, SecretManagerProvider, secretName, version ?? "latest");

            return response.SecretString ?? Convert.ToBase64String(response.SecretBinary.ToArray());
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
            var request = new DeleteSecretRequest
            {
                SecretId = secretName,
                ForceDeleteWithoutRecovery = false
            };

            var response = await _client.DeleteSecretAsync(request, cancellationToken).ConfigureAwait(false);

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
            var describeRequest = new DescribeSecretRequest
            {
                SecretId = secretName
            };

            var previousVersions = await ExceptionProtector.ExecuteTaskAndReturnOrNullReference(_client.DescribeSecretAsync(describeRequest, cancellationToken)).ConfigureAwait(false);

            if (previousVersions == null)
            {
                var createRequest = new CreateSecretRequest
                {
                    Name = secretName,
                    SecretString = value
                };

                var createResponse = await _client.CreateSecretAsync(createRequest, cancellationToken).ConfigureAwait(false);

                SecretsLog.WroteSecret(Logger, SecretManagerProvider, secretName);

                return new SecretResult()
                {
                    SecretName = secretName,
                    CurrentVersion = createResponse.VersionId,
                    PreviousVersion = null
                };
            }

            var previousVersion = previousVersions.VersionIdsToStages?.FirstOrDefault(entry => entry.Value?.Contains("AWSCURRENT") ?? false).Key;

            previousVersion ??= previousVersions.VersionIdsToStages?.Keys.FirstOrDefault();

            var request = new PutSecretValueRequest
            {
                SecretId = secretName,
                SecretString = value,
                VersionStages = ["AWSCURRENT"]
            };

            var response = await _client.PutSecretValueAsync(request, cancellationToken).ConfigureAwait(false);
            var currentVersion = response.VersionId;

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
