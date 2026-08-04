using FFCEI.DotNetStack.Caching.Configurations;
using FFCEI.DotNetStack.Serialization.Defaults;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Backplane.StackExchangeRedis;
using ZiggyCreatures.Caching.Fusion.Serialization.NeueccMessagePack;

namespace FFCEI.DotNetStack.Caching;

public static class IServiceCollectionExtensionMethods
{
    public static IServiceCollection AddLocalLocalFusionCache(this IServiceCollection services,
        FusionCacheLocalOnlyOptions options,
        Action<IFusionCacheBuilder>? builder = null,
        ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        var distributedOptions = new FusionCacheDistributedOptions(options);

        var result = services.AddDistributedFusionCache(
            distributedOptions,
            builder,
            logger);

        options.Copy(distributedOptions);

        return result;
    }

    public static IServiceCollection AddDistributedFusionCache(this IServiceCollection services,
        FusionCacheDistributedOptions options,
        Action<IFusionCacheBuilder>? builder = null,
        ILogger? logger = null)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentException.ThrowIfNullOrWhiteSpace(options.CacheName, nameof(options.CacheName));

        logger ??= NullLogger.Instance;

        options.LocalMemoryCacheEntryLifetime ??= TimeSpan.FromSeconds(30);

        if (options.LocalMemoryCacheEntryLifetime <= TimeSpan.Zero)
        {
            throw new InvalidOperationException("localMemoryCacheEntryLifetimeReference has a invalid lifetime");
        }

        options.DistributedCacheEntryLifetime ??=
            (string.IsNullOrWhiteSpace(options.DistributedCacheRedisConnectionString) || string.IsNullOrWhiteSpace(options.BackplaneRedisConnectionString) ?
                TimeSpan.Zero : TimeSpan.FromSeconds(120));

        options.UseDistributedCache = (options.DistributedCacheEntryLifetime.Value.Ticks > 0) &&
            !string.IsNullOrWhiteSpace(options.DistributedCacheRedisConnectionString) &&
            !string.IsNullOrWhiteSpace(options.BackplaneRedisConnectionString);

        if (options.UseDistributedCache)
        {
            if (options.DistributedCacheEntryLifetime.Value < options.LocalMemoryCacheEntryLifetime.Value)
            {
                options.DistributedCacheEntryLifetime = options.LocalMemoryCacheEntryLifetime;
            }
        }

        var oneSecond = TimeSpan.FromSeconds(1);

        var fusionCacheBuilder = services.AddFusionCache(options.CacheName)
            .WithOptions(fusionCacheOptions =>
            {
                fusionCacheOptions.EnableAutoRecovery = true;
                fusionCacheOptions.AutoRecoveryMaxItems = ushort.MaxValue;
                fusionCacheOptions.AutoRecoveryDelay = TimeSpan.FromTicks(options.LocalMemoryCacheEntryLifetime.Value.Ticks / 10);

                if (fusionCacheOptions.AutoRecoveryDelay < oneSecond)
                {
                    fusionCacheOptions.AutoRecoveryDelay = oneSecond;
                }
            })
            .WithDefaultEntryOptions(entryOptions =>
            {
                entryOptions.IsFailSafeEnabled = true;
                entryOptions.FailSafeThrottleDuration = TimeSpan.FromTicks(options.LocalMemoryCacheEntryLifetime.Value.Ticks / 10);

                if (entryOptions.FailSafeThrottleDuration < oneSecond)
                {
                    entryOptions.FailSafeThrottleDuration = oneSecond;
                }

                entryOptions.Duration = options.LocalMemoryCacheEntryLifetime.Value;
                entryOptions.FailSafeMaxDuration = TimeSpan.FromTicks(options.LocalMemoryCacheEntryLifetime.Value.Ticks * 2);

                if (options.UseDistributedCache)
                {
                    entryOptions.DistributedCacheDuration = options.DistributedCacheEntryLifetime.Value;
                    entryOptions.DistributedCacheFailSafeMaxDuration = TimeSpan.FromTicks(options.DistributedCacheEntryLifetime.Value.Ticks * 2);
                }

                if (options.MaxEntries != null)
                {
                    entryOptions.Size = 1;
                }
            })
            .WithSerializer(_ => new FusionCacheNeueccMessagePackSerializer(
                MessagePackDefaults.TrustedUncompressedSerializationOptions));

        if (options.MaxEntries != null)
        {
            fusionCacheBuilder = fusionCacheBuilder
                .WithMemoryCache(_ => new MemoryCache(new MemoryCacheOptions
                {
                    SizeLimit = options.MaxEntries
                }));
        }

        if (options.UseDistributedCache)
        {
            var distributedCacheMultiplexerFactory = RedisResilientConnectionFactory.CreateResilientConnectionFactory(options.DistributedCacheRedisConnectionString!);
            var backplaneMultiplexerFactory = RedisResilientConnectionFactory.CreateResilientConnectionFactory(options.BackplaneRedisConnectionString!);

            fusionCacheBuilder = fusionCacheBuilder
                .WithDistributedCache(_ => new RedisCache(new RedisCacheOptions()
                {
                    ConnectionMultiplexerFactory = distributedCacheMultiplexerFactory
                }))
                .WithBackplane(_ => new RedisBackplane(new RedisBackplaneOptions()
                {
                    ConnectionMultiplexerFactory = backplaneMultiplexerFactory
                }));
        }

        if (options.DebugLogging.HasValue)
        {
            _ = services.AddLogging(loggingBuilder
                => loggingBuilder.AddFilter("ZiggyCreatures.Caching.Fusion",
                    options.DebugLogging.Value ? LogLevel.Debug : LogLevel.Warning));
        }

        builder?.Invoke(fusionCacheBuilder);

        CachingLog.FusionCacheConfigured(logger, options.CacheName, options.UseDistributedCache);

        return services;
    }
}
