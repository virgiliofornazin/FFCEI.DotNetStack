using FFCEI.DotNetStack.Configuration;
using FFCEI.DotNetStack.Configuration.Configurations;
using FFCEI.DotNetStack.Secrets.Common;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace FFCEI.DotNetStack.Secrets.Docker;

public sealed class DockerSecretManagerRepository
    : SecretManagerRepository
{
    private const string SecretsLocation = "/run/secrets";

    private readonly ConcurrentDictionary<string, KeyValuePair<string, DateTimeOffset>> _secrets;

    public DockerSecretManagerRepository(ILogger<DockerSecretManagerRepository>? logger = null)
        : base(SecretManagerProviderType.Docker, logger)
    {
        _secrets = new(LoadSecretsFromLocation(SecretsLocation));
    }

    protected override void MapProviderException(string secretName, Exception exception)
    {
    }

    private static Dictionary<string, KeyValuePair<string, DateTimeOffset>> LoadSecretsFromLocation(string location)
    {
        var options = new EnvFileLoadOptions()
        {
            SetEnvironmentVariables = false,
            IgnoreAlreadyDefinedVariables = false,
            AllowOverridePreviousVariables = true
        };

        var variables = new Dictionary<string, string>();
        var timestamps = new Dictionary<string, DateTimeOffset>();

        if (!Directory.Exists(location))
        {
            return [];
        }

        foreach (var filePath in Directory.GetFiles(location))
        {
            var fileTimestamp = File.GetLastWriteTime(filePath);

            if (filePath.EndsWith(".env", StringComparison.OrdinalIgnoreCase))
            {
                var values = EnvFile.LoadSpecificEnvFile(filePath, options);

                EnvFile.MergeVariablesFromEnvFile(options, variables, values,
                    defined => { timestamps[defined] = fileTimestamp; });
            }
            else
            {
                var secret = Path.GetFileName(filePath);
                var value = File.ReadAllText(filePath).TrimEnd('\r', '\n');

                EnvFile.MergeVariablesFromEnvFile(options, variables, [KeyValuePair.Create(secret, value)],
                    defined => { timestamps[defined] = fileTimestamp; });
            }
        }

        var result = new Dictionary<string, KeyValuePair<string, DateTimeOffset>>();

        foreach (var variable in variables)
        {
            if (!timestamps.TryGetValue(variable.Key, out var timestamp))
            {
                timestamp = DateTimeOffset.MinValue;
            }

            result[variable.Key] = KeyValuePair.Create(variable.Value, timestamp);
        }

        return result;
    }

    public override Task<IReadOnlyCollection<SecretValueVersion>> ListVersionsAsync(string secretName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretName, nameof(secretName));

        SecretsLog.ListingSecretVersions(Logger, SecretManagerProvider, secretName);

        if (!_secrets.TryGetValue(secretName, out var secret))
        {
            SecretNotFoundException.Throw(secretName);
        }

        var result = new SecretValueVersion()
        {
            Version = "latest",
            IsCurrent = true,
            CreationTimestamp = secret.Value,
            Status = "Active"
        };

        SecretsLog.EnumeratedSecretVersions(Logger, SecretManagerProvider, secretName, 1);

        return Task.FromResult<IReadOnlyCollection<SecretValueVersion>>([result]);
    }

    protected override Task<string?> DoGetSecretByVersionAsync(string secretName, string? version, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretName, nameof(secretName));

        if (!string.IsNullOrWhiteSpace(version) && !version.Equals("latest", StringComparison.OrdinalIgnoreCase))
        {
            SecretNotFoundException.Throw(secretName);
        }

        SecretsLog.ReadingSecret(Logger, SecretManagerProvider, secretName, version ?? "latest");

        if (!_secrets.TryGetValue(secretName, out var value))
        {
            SecretNotFoundException.Throw(secretName);
        }

        SecretsLog.ReadSecret(Logger, SecretManagerProvider, secretName, version ?? "latest");

        return Task.FromResult<string?>(value.Key);
    }

    public override Task DeleteSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretName, nameof(secretName));

        SecretsLog.DeletingSecret(Logger, SecretManagerProvider, secretName);

        SecretManagerRepositoryException.Throw(this, new NotImplementedException());

        return Task.CompletedTask;
    }

    public override Task<SecretResult> SetSecretAsync(string secretName, string value, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretName, nameof(secretName));
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));

        SecretsLog.WritingSecret(Logger, SecretManagerProvider, secretName);

        SecretManagerRepositoryException.Throw(this, new NotImplementedException());

        return Task.FromResult<SecretResult>(null!);
    }
}
