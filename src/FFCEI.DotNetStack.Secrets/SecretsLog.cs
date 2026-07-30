using Microsoft.Extensions.Logging;

namespace FFCEI.DotNetStack.Secrets;

internal static partial class SecretsLog
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "[{Provider}] Reading secret '{SecretName}' (version: {Version})...")]
    internal static partial void ReadingSecret(ILogger logger, SecretManagerProviderType provider, string secretName, string version);

    [LoggerMessage(Level = LogLevel.Debug, Message = "[{Provider}] Read secret '{SecretName}' (version: {Version})")]
    internal static partial void ReadSecret(ILogger logger, SecretManagerProviderType provider, string secretName, string version);

    [LoggerMessage(Level = LogLevel.Information, Message = "[{Provider}] Writing secret '{SecretName}'...")]
    internal static partial void WritingSecret(ILogger logger, SecretManagerProviderType provider, string secretName);

    [LoggerMessage(Level = LogLevel.Information, Message = "[{Provider}] Wrote secret '{SecretName}'")]
    internal static partial void WroteSecret(ILogger logger, SecretManagerProviderType provider, string secretName);

    [LoggerMessage(Level = LogLevel.Information, Message = "[{Provider}] Deleting secret '{SecretName}'...")]
    internal static partial void DeletingSecret(ILogger logger, SecretManagerProviderType provider, string secretName);

    [LoggerMessage(Level = LogLevel.Information, Message = "[{Provider}] Deleted secret '{SecretName}'")]
    internal static partial void DeletedSecret(ILogger logger, SecretManagerProviderType provider, string secretName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "[{Provider}] Listing versions of secret '{SecretName}'...")]
    internal static partial void ListingSecretVersions(ILogger logger, SecretManagerProviderType provider, string secretName);

    [LoggerMessage(Level = LogLevel.Debug, Message = "[{Provider}] Enumerated {VersionCount} version(s) of secret '{SecretName}'")]
    internal static partial void EnumeratedSecretVersions(ILogger logger, SecretManagerProviderType provider, string secretName, int versionCount);

    [LoggerMessage(Level = LogLevel.Error, Message = "[{Provider}] {Operation} failed for secret '{SecretName}'")]
    internal static partial void SecretOperationFailed(ILogger logger, Exception exception, SecretManagerProviderType provider, SecretManagerOperationType operation, string secretName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Secret manager repository configured (provider: {Provider}, caching: {UseCaching})")]
    internal static partial void SecretManagerRepositoryConfigured(ILogger logger, SecretManagerProviderType provider, bool useCaching);
}
