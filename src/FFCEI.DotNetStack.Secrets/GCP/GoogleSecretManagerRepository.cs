using FFCEI.DotNetStack.Secrets.Common;
using Google.Cloud.SecretManager.V1;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace FFCEI.DotNetStack.Secrets.GCP;

public sealed class GoogleSecretManagerRepository
    : SecretManagerRepository
{
    private readonly SecretManagerServiceClient _client;
    private readonly string _projectId;

    public GoogleSecretManagerRepository(SecretManagerServiceClient client, string projectId, ILogger<GoogleSecretManagerRepository>? logger = null)
        : base(SecretManagerProviderType.GCP, logger)
    {
        ArgumentNullException.ThrowIfNull(client, nameof(client));
        ArgumentException.ThrowIfNullOrWhiteSpace(projectId, nameof(projectId));

        _client = client;
        _projectId = projectId;
    }

    private string GetGoogleSecretPath(string secretName)
        => $"projects/{_projectId}/secrets/{secretName}";

    protected override void MapProviderException(string secretName, Exception exception)
    {
        if (exception is RpcException rpcException)
        {
            if (rpcException.StatusCode == StatusCode.NotFound)
            {
                SecretNotFoundException.Throw(secretName);
            }

            if (rpcException.StatusCode == StatusCode.PermissionDenied)
            {
                SecretAccessDeniedException.Throw(secretName, rpcException);
            }
        }
    }

    public override async Task<IReadOnlyCollection<SecretValueVersion>> ListVersionsAsync(string secretName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretName, nameof(secretName));

        SecretsLog.ListingSecretVersions(Logger, SecretManagerProvider, secretName);

        try
        {
            var request = GetGoogleSecretPath(secretName);
            var response = _client.ListSecretVersionsAsync(request);

            var result = new List<SecretValueVersion>();

            await foreach (var version in response.WithCancellation(cancellationToken))
            {
                result.Add(new()
                {
                    Version = version.SecretVersionName.SecretVersionId,
                    IsCurrent = version.State == SecretVersion.Types.State.Enabled,
                    CreationTimestamp = version.CreateTime?.ToDateTimeOffset() ?? DateTimeOffset.MinValue,
                    Status = version.State.ToString()
                });
            }

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
            var request = new SecretVersionName(_projectId, secretName, string.IsNullOrWhiteSpace(version) ? "latest" : version);
            var response = await _client.AccessSecretVersionAsync(request, cancellationToken).ConfigureAwait(false);

            SecretsLog.ReadSecret(Logger, SecretManagerProvider, secretName, version ?? "latest");

            return response.Payload.Data.ToStringUtf8();
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
            var request = GetGoogleSecretPath(secretName);

            await _client.DeleteSecretAsync(request, cancellationToken).ConfigureAwait(false);

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
            string? previousVersion = null;

            try
            {
                var latestVersionName = new SecretVersionName(_projectId, secretName, "latest");
                var latestVersionResponse = await _client.AccessSecretVersionAsync(latestVersionName, cancellationToken).ConfigureAwait(false);

                previousVersion = SecretVersionName.Parse(latestVersionResponse.Name).SecretVersionId;
            }
            catch (RpcException rpcException) when (rpcException.StatusCode == StatusCode.NotFound)
            {
            }

            var request = new AddSecretVersionRequest
            {
                Parent = GetGoogleSecretPath(secretName),
                Payload = new SecretPayload { Data = ByteString.CopyFromUtf8(value) }
            };

            SecretVersion response;

            try
            {
                response = await _client.AddSecretVersionAsync(request, cancellationToken).ConfigureAwait(false);
            }
            catch (RpcException rpcException) when (rpcException.StatusCode == StatusCode.NotFound)
            {
                var createRequest = new CreateSecretRequest
                {
                    Parent = $"projects/{_projectId}",
                    SecretId = secretName,
                    Secret = new Secret
                    {
                        Replication = new Replication { Automatic = new Replication.Types.Automatic() }
                    }
                };

                _ = await _client.CreateSecretAsync(createRequest, cancellationToken).ConfigureAwait(false);

                response = await _client.AddSecretVersionAsync(request, cancellationToken).ConfigureAwait(false);
                previousVersion = null;
            }

            var currentVersion = response.SecretVersionName.SecretVersionId;

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
