using Azure.Core;
using Azure.Identity;
using EFCoreSecondLevelCacheInterceptor;
using FFCEI.DotNetStack.Caching;
using FFCEI.DotNetStack.EntityFramework.Configurations;
using FFCEI.DotNetStack.EntityFramework.Storage.DataProtection;
using Google.Cloud.Kms.V1;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using ZiggyCreatures.Caching.Fusion;

namespace FFCEI.DotNetStack.EntityFramework;

public static class IServiceCollectionExtensionMethods
{
    public static IServiceCollection AddDbContextFactoryAndRepositoryContainer<TContext, TRepositoryContainer>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder>? configureDbContextOptions = null,
        ILogger? logger = null) where TContext : DbContext where TRepositoryContainer : EntityModelRepositoryContainer
    {
        logger ??= NullLogger.Instance;

        _ = services
            .AddDbContextFactory<TContext>(configureDbContextOptions)
            .AddScoped<TRepositoryContainer>();

        EntityFrameworkLog.DbContextAndRepositoryConfigured(logger, typeof(TContext), typeof(TRepositoryContainer));

        return services;
    }

    public static IServiceCollection AddDbContextFactoryAndRepositoryContainer<TContext, TRepositoryContainer>(
        this IServiceCollection services,
        Action<IServiceProvider, DbContextOptionsBuilder> configureDbContextOptions,
        ILogger? logger = null) where TContext : DbContext where TRepositoryContainer : EntityModelRepositoryContainer
    {
        logger ??= NullLogger.Instance;

        _ = services
            .AddDbContextFactory<TContext>(configureDbContextOptions)
            .AddScoped<TRepositoryContainer>();

        EntityFrameworkLog.DbContextAndRepositoryConfigured(logger, typeof(TContext), typeof(TRepositoryContainer));

        return services;
    }

    public const string EFSecondLevelCacheFusionCacheNameDefault = "EFCoreFusionCache";

    public static IServiceCollection AddEFCoreCachingViaEFSecondLevelCache(this IServiceCollection services,
        EFCoreCachingSecondLevelCacheOptions? options = null,
        Action<EFCoreSecondLevelCacheOptions>? efSecondLevelCacheBuilder = null,
        Action<IFusionCacheBuilder>? fusionCacheBuilder = null,
        ILogger? logger = null)
    {
        logger ??= NullLogger.Instance;

        options ??= new();
        options.CacheOptions.CacheName ??= EFSecondLevelCacheFusionCacheNameDefault;
        options.CacheOptions.DebugLogging ??= Debugger.IsAttached;
        options.EntityCacheEntryPrefix ??= "EF_";

        _ = services.AddDistributedFusionCache(
            options.CacheOptions,
            fusionCacheBuilder,
            logger);

        _ = services.AddEFSecondLevelCache(cacheOptions =>
        {
            _ = cacheOptions.UseFusionCacheProvider(options.CacheOptions.CacheName);
            _ = cacheOptions.UseCacheKeyPrefix(options.EntityCacheEntryPrefix);
            _ = cacheOptions.CacheAllQueries(CacheExpirationMode.Absolute, options.CacheOptions.LocalMemoryCacheEntryLifetime);

            _ = options.CacheEmptyResults
                ? cacheOptions.SkipCachingResults(results => results.Value == null)
                : cacheOptions.SkipCachingResults(results => (results.Value == null) || ((results.Value is EFTableRows rows) && !rows.HasRows));

            _ = options.CacheOptions.UseDistributedCache
                ? cacheOptions.UseDbCallsIfCachingProviderIsDown(options.CacheOptions.DistributedCacheEntryLifetime!.Value)
                : cacheOptions.UseDbCallsIfCachingProviderIsDown(options.CacheOptions.LocalMemoryCacheEntryLifetime!.Value);

            if (options.CacheOptions.DebugLogging ?? false)
            {
                _ = cacheOptions.ConfigureLogging(true);
            }

            efSecondLevelCacheBuilder?.Invoke(cacheOptions);
        });

        EntityFrameworkLog.EFSecondLevelCacheConfigured(logger);

        return services;
    }
    internal static IServiceCollection ApplyDataProtectionOptions(
        this IServiceCollection services,
        IDataProtectionBuilder builder,
        DataProtectionConfigurationOptions options,
        string dataProtectionProvider,
        X509Certificate2? x509EncryptionCertificate = null,
        IReadOnlyCollection<X509Certificate2>? x509DecryptionCertificates = null,
        ILogger? logger = null)
    {
        logger ??= NullLogger.Instance;

        _ = builder.SetApplicationName(options.ApplicationName);

        if (x509EncryptionCertificate != null)
        {
            _ = builder.ProtectKeysWithCertificate(x509EncryptionCertificate);
        }

        if ((x509DecryptionCertificates != null) && (x509DecryptionCertificates.Count > 0))
        {
            var decryptionCertificates = ((X509Certificate2[])(x509EncryptionCertificate == null ?
                [.. x509DecryptionCertificates] : [x509EncryptionCertificate, .. x509DecryptionCertificates]))
                   .Distinct()
                   .ToArray();

            _ = builder.UnprotectKeysWithAnyCertificate(decryptionCertificates);
        }

        if (options.DisableAutomaticKeyGeneration)
        {
            _ = builder.DisableAutomaticKeyGeneration();
        }
        else
        {
            var keyLifetime = options.KeyLifetime ?? TimeSpan.FromDays(30);

            _ = builder.SetDefaultKeyLifetime(keyLifetime);
        }

        if (options.DataProtectionLogLevel != null)
        {
            _ = services.AddLogging(loggingBuilder =>
            {
                _ = loggingBuilder.AddFilter("Microsoft.AspNetCore.DataProtection",
                    options.DataProtectionLogLevel.Value);
            });
        }

        EntityFrameworkLog.DataProtectionConfigured(logger, dataProtectionProvider);

        return services;
    }

    public static IServiceCollection AddEFDataProtectionUsingFilesystemKeyStore(
        this IServiceCollection services,
        string directoryPath,
        Action<DataProtectionConfigurationOptions>? configureOptions = null,
        X509Certificate2? x509EncryptionCertificate = null,
        IReadOnlyCollection<X509Certificate2>? x509DecryptionCertificates = null,
        ILogger? logger = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath, nameof(directoryPath));

        var options = new DataProtectionConfigurationOptions();

        configureOptions?.Invoke(options);

        var builder = services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(directoryPath));

        return services.ApplyDataProtectionOptions(builder, options, "Filesystem Key Store", x509EncryptionCertificate, x509DecryptionCertificates, logger);
    }

    public static IServiceCollection AddEFDataProtectionUsingDatabaseTableKeyStore<TContext>(
        this IServiceCollection services,
        Action<DataProtectionConfigurationOptions>? configureOptions = null,
        X509Certificate2? x509EncryptionCertificate = null,
        IReadOnlyCollection<X509Certificate2>? x509DecryptionCertificates = null,
        ILogger? logger = null)
        where TContext : DbContext, IDataProtectionKeyContext
    {
        var options = new DataProtectionConfigurationOptions();

        configureOptions?.Invoke(options);

        var builder = services.AddDataProtection()
            .PersistKeysToDbContext<TContext>();

        return services.ApplyDataProtectionOptions(builder, options, $"Database Table Key Store ({typeof(TContext).Name})", x509EncryptionCertificate, x509DecryptionCertificates, logger);
    }

    internal static bool UsingKeyDbContextDefault;

    public static IServiceCollection AddEFDataProtectionUsingStandardKeyContextDatabaseTableKeyStore(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder>? configureDbContextOptions = null,
        Action<DataProtectionConfigurationOptions>? configureOptions = null,
        X509Certificate2? x509EncryptionCertificate = null,
        IReadOnlyCollection<X509Certificate2>? x509DecryptionCertificates = null,
        ILogger? logger = null)
    {
        _ = services.AddDbContextFactory<DataProtectionKeyDbContext>(configureDbContextOptions);

        UsingKeyDbContextDefault = true;

        return services.AddEFDataProtectionUsingDatabaseTableKeyStore<DataProtectionKeyDbContext>(configureOptions, x509EncryptionCertificate, x509DecryptionCertificates, logger);
    }

    public static IServiceCollection AddEFDataProtectionUsingStandardKeyContextDatabaseTableKeyStore(
        this IServiceCollection services,
        Action<IServiceProvider, DbContextOptionsBuilder> configureDbContextOptions,
        Action<DataProtectionConfigurationOptions>? configureOptions = null,
        X509Certificate2? x509EncryptionCertificate = null,
        IReadOnlyCollection<X509Certificate2>? x509DecryptionCertificates = null,
        ILogger? logger = null)
    {
        _ = services.AddDbContextFactory<DataProtectionKeyDbContext>(configureDbContextOptions);

        UsingKeyDbContextDefault = true;

        return services.AddEFDataProtectionUsingDatabaseTableKeyStore<DataProtectionKeyDbContext>(configureOptions, x509EncryptionCertificate, x509DecryptionCertificates, logger);
    }

    private const string RedisDataProtectionConnectionFactoryKey = "FFCEI.DotNetStack.EntityFramework.DataProtection.RedisResilientConnectionFactory";

    public static IServiceCollection AddEFDataProtectionUsingRedisKeyStore(
        this IServiceCollection services,
        string redisConnectionString,
        string redisKeyName,
        Action<DataProtectionConfigurationOptions>? configureOptions = null,
        X509Certificate2? x509EncryptionCertificate = null,
        IReadOnlyCollection<X509Certificate2>? x509DecryptionCertificates = null,
        ILogger? logger = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(redisConnectionString, nameof(redisConnectionString));
        ArgumentException.ThrowIfNullOrWhiteSpace(redisKeyName, nameof(redisKeyName));

        var options = new DataProtectionConfigurationOptions();

        configureOptions?.Invoke(options);

#pragma warning disable CA2000 // Dispose objects before losing scope
        var redisResilientConnectionFactory = new RedisResilientConnectionFactory(redisConnectionString);
#pragma warning restore CA2000 // Dispose objects before losing scope

        _ = services.AddKeyedSingleton<RedisResilientConnectionFactory>(RedisDataProtectionConnectionFactoryKey, redisResilientConnectionFactory);

        var builder = services.AddDataProtection()
            .PersistKeysToStackExchangeRedis(() => redisResilientConnectionFactory.GetDatabaseAsync().GetAwaiter().GetResult(), redisKeyName);

        return ApplyDataProtectionOptions(services, builder, options, "Redis Key Store", x509EncryptionCertificate, x509DecryptionCertificates, logger);
    }

    public static IServiceCollection AddEFDataProtectionUsingAzureKeyVaultAndBlobStorageKeyStore(
        this IServiceCollection services,
        Uri azureKeyVaultUri,
        Uri azureBlobStorageUri,
        Action<DataProtectionConfigurationOptions>? configureOptions = null,
        TokenCredential? customCredential = null,
        ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(azureKeyVaultUri, nameof(azureKeyVaultUri));
        ArgumentNullException.ThrowIfNull(azureBlobStorageUri, nameof(azureBlobStorageUri));

        var options = new DataProtectionConfigurationOptions();

        configureOptions?.Invoke(options);

        var credential = customCredential ?? new DefaultAzureCredential();

        var builder = services.AddDataProtection()
            .PersistKeysToAzureBlobStorage(azureBlobStorageUri, credential)
            .ProtectKeysWithAzureKeyVault(azureKeyVaultUri, credential);

        return ApplyDataProtectionOptions(services, builder, options, "Azure Key Vault + Blob Storage Key Store", logger: logger);
    }

    public static IServiceCollection AddEFDataProtectionUsingAWSSystemsManagerKeyStore(
        this IServiceCollection services,
        string awsSystemsManagerParameterPath,
        Action<DataProtectionConfigurationOptions>? configureOptions = null,
        string? kmsKeyId = null,
        ILogger? logger = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(awsSystemsManagerParameterPath, nameof(awsSystemsManagerParameterPath));

        var options = new DataProtectionConfigurationOptions();

        configureOptions?.Invoke(options);

        var builder = services.AddDataProtection()
            .PersistKeysToAWSSystemsManager(awsSystemsManagerParameterPath, persistOptions =>
            {
                if (kmsKeyId != null)
                {
                    persistOptions.KMSKeyId = kmsKeyId;
                }
            });

        return ApplyDataProtectionOptions(services, builder, options, "AWS Systems Manager Key Store", logger: logger);
    }

    public static IServiceCollection AddEFDataProtectionUsingGoogleKmsKeyStore(
        this IServiceCollection services,
        string projectId,
        string locationId,
        string keyRingId,
        string keyId,
        Action<DataProtectionConfigurationOptions>? configureOptions = null,
        ILogger? logger = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectId, nameof(projectId));
        ArgumentException.ThrowIfNullOrWhiteSpace(locationId, nameof(locationId));
        ArgumentException.ThrowIfNullOrWhiteSpace(keyRingId, nameof(keyRingId));
        ArgumentException.ThrowIfNullOrWhiteSpace(keyId, nameof(keyId));

        var options = new DataProtectionConfigurationOptions();

        configureOptions?.Invoke(options);

        var builder = services.AddDataProtection()
            .ProtectKeysWithGoogleKms(projectId, locationId, keyRingId, keyId);

        return ApplyDataProtectionOptions(services, builder, options, "Google Kms Key Store", logger: logger);
    }

    public static IServiceCollection AddEFDataProtectionUsingGoogleKmsKeyStore(
        this IServiceCollection services,
        CryptoKeyName keyName,
        Action<DataProtectionConfigurationOptions>? configureOptions = null,
        ILogger? logger = null)
    {
        var options = new DataProtectionConfigurationOptions();

        configureOptions?.Invoke(options);

        var builder = services.AddDataProtection()
            .ProtectKeysWithGoogleKms(keyName);

        return ApplyDataProtectionOptions(services, builder, options, "Google Kms Key Store", logger: logger);
    }

    public static IServiceCollection AddEFDataProtectionUsingGoogleKmsKeyStore(
        this IServiceCollection services,
        string keyName,
        Action<DataProtectionConfigurationOptions>? configureOptions = null,
        ILogger? logger = null)
    {
        var options = new DataProtectionConfigurationOptions();

        configureOptions?.Invoke(options);

        var builder = services.AddDataProtection()
            .ProtectKeysWithGoogleKms(keyName);

        return ApplyDataProtectionOptions(services, builder, options, "Google Kms Key Store", logger: logger);
    }

    public static IServiceCollection AddEFDataProtectionUsingGoogleKmsKeyStore(
        this IServiceCollection services,
        CryptoKeyName keyName,
        KeyManagementServiceClient kmsClient,
        Action<DataProtectionConfigurationOptions>? configureOptions = null,
        ILogger? logger = null)
    {
        var options = new DataProtectionConfigurationOptions();

        configureOptions?.Invoke(options);

        var builder = services.AddDataProtection()
            .ProtectKeysWithGoogleKms(keyName, kmsClient);

        return ApplyDataProtectionOptions(services, builder, options, "Google Kms Key Store", logger: logger);
    }
}
