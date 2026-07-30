using Amazon;
using Amazon.Runtime;
using Amazon.SecretsManager;
using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using FFCEI.DotNetStack.Caching;
using FFCEI.DotNetStack.Secrets.AWS;
using FFCEI.DotNetStack.Secrets.Azure;
using FFCEI.DotNetStack.Secrets.Common;
using FFCEI.DotNetStack.Secrets.Configurations;
using FFCEI.DotNetStack.Secrets.Docker;
using FFCEI.DotNetStack.Secrets.GCP;
using FFCEI.DotNetStack.Secrets.Interfaces;
using FFCEI.DotNetStack.Secrets.OCI;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.SecretManager.V1;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Oci.SecretsService;
using Oci.VaultService;
using System.Diagnostics;

namespace FFCEI.DotNetStack.Secrets;

public static class IServiceCollectionExtensionMethods
{
    public const string SecretManagerFusionCacheDefault = "SecretManagerFusionCache";

    public static IServiceCollection AddCustomSecretManagerRepository(this IServiceCollection services,
        ISecretManagerRepository secretManagerRepository,
        SecretManagerRepositoryOptions? options = null,
        ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(secretManagerRepository, nameof(secretManagerRepository));

        logger ??= NullLogger.Instance;

        options ??= new();

        var useCaching = false;

        if (options.CacheOptions != null)
        {
            options.CacheOptions.CacheName ??= SecretManagerFusionCacheDefault;
            options.CacheOptions.DebugLogging ??= Debugger.IsAttached;

            _ = services.AddLocalLocalFusionCache(
                options.CacheOptions,
                options.FusionCacheBuilder,
                logger);

            _ = services.AddSingleton<ISecretManagerRepository>(serviceProvider =>
            {
                if (secretManagerRepository is SecretManagerRepository underlyingRepository && underlyingRepository.Logger == NullLogger.Instance)
                {
                    underlyingRepository.Logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(underlyingRepository.GetType());
                }

                var cachingSecretManager = new CachingSecretManagerRepository(secretManagerRepository, serviceProvider, options.CacheOptions.CacheName);

                return cachingSecretManager;
            });

            useCaching = true;
        }
        else
        {
            _ = services.AddSingleton<ISecretManagerRepository>(serviceProvider =>
            {
                if (secretManagerRepository is SecretManagerRepository underlyingRepository && underlyingRepository.Logger == NullLogger.Instance)
                {
                    underlyingRepository.Logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(underlyingRepository.GetType());
                }

                return secretManagerRepository;
            });
        }

        _ = services.AddSingleton<ISecretRepository>(serviceProvider =>
            serviceProvider.GetRequiredService<ISecretManagerRepository>());

        _ = services.AddSingleton<IVersionedSecretRepository>(serviceProvider =>
            serviceProvider.GetRequiredService<ISecretManagerRepository>());

        SecretsLog.SecretManagerRepositoryConfigured(logger, secretManagerRepository.SecretManagerProvider, useCaching);

        return services;
    }

    public static IServiceCollection AddAWSSecretManagerRepository(this IServiceCollection services,
        IAmazonSecretsManager client,
        SecretManagerRepositoryOptions? options = null,
        ILogger? logger = null)
        => services.AddCustomSecretManagerRepository(new AWSSecretManagerRepository(client), options, logger);

    public static IServiceCollection AddAWSSecretManagerRepository(this IServiceCollection services,
        AmazonSecretsManagerConfig config,
        SecretManagerRepositoryOptions? options = null,
        ILogger? logger = null)
        => services.AddAWSSecretManagerRepository(new AmazonSecretsManagerClient(config), options, logger);

    public static IServiceCollection AddAWSSecretManagerRepository(this IServiceCollection services,
        RegionEndpoint region,
        SecretManagerRepositoryOptions? options = null,
        ILogger? logger = null)
        => services.AddAWSSecretManagerRepository(new AmazonSecretsManagerClient(region), options, logger);

    public static IServiceCollection AddAWSSecretManagerRepository(this IServiceCollection services,
        string accessKey,
        string clientSecret,
        AmazonSecretsManagerConfig config,
        SecretManagerRepositoryOptions? options = null,
        ILogger? logger = null)
        => services.AddAWSSecretManagerRepository(new AmazonSecretsManagerClient(accessKey, clientSecret, config), options, logger);

    public static IServiceCollection AddAWSSecretManagerRepository(this IServiceCollection services,
        string accessKey,
        string clientSecret,
        RegionEndpoint region,
        SecretManagerRepositoryOptions? options = null,
        ILogger? logger = null)
        => services.AddAWSSecretManagerRepository(new AmazonSecretsManagerClient(accessKey, clientSecret, region), options, logger);

    public static IServiceCollection AddAWSSecretManagerRepository(this IServiceCollection services,
        AWSCredentials awsCredentials,
        RegionEndpoint region,
        SecretManagerRepositoryOptions? options = null,
        ILogger? logger = null)
        => services.AddAWSSecretManagerRepository(new AmazonSecretsManagerClient(awsCredentials, region), options, logger);

    public static IServiceCollection AddAWSSecretManagerRepository(this IServiceCollection services,
        AWSCredentials awsCredentials,
        AmazonSecretsManagerConfig config,
        SecretManagerRepositoryOptions? options = null,
        ILogger? logger = null)
        => services.AddAWSSecretManagerRepository(new AmazonSecretsManagerClient(awsCredentials, config), options, logger);

    public static IServiceCollection AddAzureKeyVaultSecretManagerRepository(this IServiceCollection services,
        SecretClient client,
        SecretManagerRepositoryOptions? options = null,
        ILogger? logger = null)
        => services.AddCustomSecretManagerRepository(new AzureKeyVaultSecretManagerRepository(client), options, logger);

    public static IServiceCollection AddAzureKeyVaultSecretManagerRepository(this IServiceCollection services,
        Uri? uri = null,
        TokenCredential? credentials = null,
        SecretManagerRepositoryOptions? options = null,
        ILogger? logger = null)
        => services.AddAzureKeyVaultSecretManagerRepository(new SecretClient(uri ?? new Uri("https://azure.net"), credentials ?? new DefaultAzureCredential()), options, logger);

    public static IServiceCollection AddAzureKeyVaultSecretManagerRepository(this IServiceCollection services,
        string tenantId,
        string clientId,
        string clientSecret,
        Uri? uri = null,
        SecretManagerRepositoryOptions? options = null,
        ILogger? logger = null)
        => services.AddAzureKeyVaultSecretManagerRepository(new SecretClient(uri ?? new Uri("https://azure.net"), new ClientSecretCredential(tenantId, clientId, clientSecret)), options, logger);

    public static IServiceCollection AddDockerSecretManagerRepository(this IServiceCollection services,
        ILogger? logger = null)
        => services.AddCustomSecretManagerRepository(new DockerSecretManagerRepository(), options: new(), logger);

    public static IServiceCollection AddGCPSecretManagerRepository(this IServiceCollection services,
        SecretManagerServiceClient client,
        string projectId,
        SecretManagerRepositoryOptions? options = null,
        ILogger? logger = null)
        => services.AddCustomSecretManagerRepository(new GoogleSecretManagerRepository(client, projectId), options, logger);

    public static IServiceCollection AddGCPSecretManagerRepository(this IServiceCollection services,
        string projectId,
        SecretManagerRepositoryOptions? options = null,
        ILogger? logger = null)
        => services.AddGCPSecretManagerRepository(SecretManagerServiceClient.Create(), projectId, options, logger);

    public static IServiceCollection AddGCPSecretManagerRepository(this IServiceCollection services,
        string jsonCredentialsPath,
        string projectId,
        SecretManagerRepositoryOptions? options = null,
        ILogger? logger = null)
        => services.AddGCPSecretManagerRepository(new SecretManagerServiceClientBuilder()
        {
            Credential = CredentialFactory.FromFile<ServiceAccountCredential>(jsonCredentialsPath).ToGoogleCredential()
        }.Build(), projectId, options, logger);

    public static IServiceCollection AddOCISecretManagerRepository(this IServiceCollection services,
        SecretsClient secretsClient,
        VaultsClient vaultsClient,
        string compartmentId,
        string vaultId,
        string? keyId = null,
        SecretManagerRepositoryOptions? options = null,
        ILogger? logger = null)
        => services.AddCustomSecretManagerRepository(new OCISecretManagerRepository(secretsClient, vaultsClient, compartmentId, vaultId, keyId), options, logger);
}
