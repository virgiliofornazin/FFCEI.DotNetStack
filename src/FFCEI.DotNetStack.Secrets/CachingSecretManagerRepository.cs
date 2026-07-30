using FFCEI.DotNetStack.Secrets.Common;
using FFCEI.DotNetStack.Secrets.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using ZiggyCreatures.Caching.Fusion;

namespace FFCEI.DotNetStack.Secrets;

public sealed class CachingSecretManagerRepository
    : SecretManagerRepository
{
    private const string CurrentVersionCacheKeySuffix = "@current";

    private readonly ISecretManagerRepository _underlyingSecretManagerRepository;
    private readonly IServiceProvider? _serviceProvider;
    private readonly string? _fusionCacheName;
    private IFusionCache? _fusionCache;

    private IFusionCache FusionCache => _fusionCache ??= _serviceProvider!
        .GetRequiredService<IFusionCacheProvider>()
        .GetCache(_fusionCacheName!);

    private static string SecretValueCacheKey(string secretName, string? version)
        => $"value:{secretName}:{(string.IsNullOrWhiteSpace(version) ? CurrentVersionCacheKeySuffix : version)}";

    private static string SecretVersionsCacheKey(string secretName)
        => $"versions:{secretName}";

    private static string SecretCachedVersionsCacheKey(string secretName)
        => $"cachedversions:{secretName}";

    public CachingSecretManagerRepository(ISecretManagerRepository underlyingSecretManagerRepository, IServiceProvider serviceProvider, string fusionCacheName)
        : base(underlyingSecretManagerRepository?.SecretManagerProvider ?? default)
    {
        ArgumentNullException.ThrowIfNull(underlyingSecretManagerRepository, nameof(underlyingSecretManagerRepository));
        ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));
        ArgumentException.ThrowIfNullOrWhiteSpace(fusionCacheName, nameof(fusionCacheName));

        _underlyingSecretManagerRepository = underlyingSecretManagerRepository;
        _serviceProvider = serviceProvider;
        _fusionCacheName = fusionCacheName;
    }

    public CachingSecretManagerRepository(ISecretManagerRepository underlyingSecretManagerRepository, IFusionCache fusionCache)
        : base(underlyingSecretManagerRepository?.SecretManagerProvider ?? default)
    {
        ArgumentNullException.ThrowIfNull(underlyingSecretManagerRepository, nameof(underlyingSecretManagerRepository));
        ArgumentNullException.ThrowIfNull(fusionCache, nameof(fusionCache));

        _underlyingSecretManagerRepository = underlyingSecretManagerRepository;
        _fusionCache = fusionCache;
    }

    protected override void MapProviderException(string secretName, Exception exception)
    {

    }

    private async Task TrackCachedVersionAsync(string secretName, string version, CancellationToken cancellationToken)
    {
        var trackingKey = SecretCachedVersionsCacheKey(secretName);
        var cachedTrackedVersions = await FusionCache.TryGetAsync<HashSet<string>>(trackingKey, token: cancellationToken).ConfigureAwait(false);
        var trackedVersions = cachedTrackedVersions.HasValue ? cachedTrackedVersions.Value ?? [] : [];

        if (trackedVersions.Add(version))
        {
            await FusionCache.SetAsync(trackingKey, trackedVersions, token: cancellationToken).ConfigureAwait(false);
        }
    }

    public override async Task<IReadOnlyCollection<SecretValueVersion>> ListVersionsAsync(string secretName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretName, nameof(secretName));

        try
        {
            var cacheKey = SecretVersionsCacheKey(secretName);
            var cachedVersions = await FusionCache.TryGetAsync<List<SecretValueVersion>>(cacheKey, token: cancellationToken).ConfigureAwait(false);

            List<SecretValueVersion> versions;

            if (cachedVersions.HasValue)
            {
                versions = cachedVersions.Value ?? [];
            }
            else
            {
                versions = [.. await _underlyingSecretManagerRepository.ListVersionsAsync(secretName, cancellationToken).ConfigureAwait(false)];

                await FusionCache.SetAsync(cacheKey, versions, token: cancellationToken).ConfigureAwait(false);
            }

            var result = versions.AsReadOnly();

            if (result.Count == 0)
            {
                SecretNotFoundException.Throw(secretName);
            }

            KeepOnlyNewestVersionAsCurrentIfNeeded(result);

            return result;
        }
        catch (SecretNotFoundException)
        {
            throw;
        }
        catch (SecretAccessDeniedException)
        {
            throw;
        }
        catch (SecretManagerRepositoryException)
        {
            throw;
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            return SecretManagerRepositoryException.Throw<IReadOnlyCollection<SecretValueVersion>>(this, exception);
        }
    }

    protected override async Task<string?> DoGetSecretByVersionAsync(string secretName, string? version, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretName, nameof(secretName));

        try
        {
            var cacheKey = SecretValueCacheKey(secretName, version ?? "latest");
            var cachedValue = await FusionCache.TryGetAsync<string?>(cacheKey, token: cancellationToken).ConfigureAwait(false);

            if (cachedValue.HasValue)
            {
                return cachedValue.Value;
            }

            var value = await _underlyingSecretManagerRepository.GetSecretByVersionAsync(secretName, version!, cancellationToken).ConfigureAwait(false);

            await FusionCache.SetAsync(cacheKey, value, token: cancellationToken).ConfigureAwait(false);

            await TrackCachedVersionAsync(secretName, version ?? "latest", cancellationToken).ConfigureAwait(false);

            return value;
        }
        catch (SecretNotFoundException)
        {
            throw;
        }
        catch (SecretAccessDeniedException)
        {
            throw;
        }
        catch (SecretManagerRepositoryException)
        {
            throw;
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            return SecretManagerRepositoryException.Throw<string?>(this, exception);
        }
    }

    public override async Task DeleteSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretName, nameof(secretName));

        try
        {
            await _underlyingSecretManagerRepository.DeleteSecretAsync(secretName, cancellationToken).ConfigureAwait(false);

            var trackingKey = SecretCachedVersionsCacheKey(secretName);
            var cachedTrackedVersions = await FusionCache.TryGetAsync<HashSet<string>>(trackingKey, token: cancellationToken).ConfigureAwait(false);

            if (cachedTrackedVersions.HasValue && (cachedTrackedVersions.Value != null))
            {
                foreach (var version in cachedTrackedVersions.Value)
                {
                    await FusionCache.RemoveAsync(SecretValueCacheKey(secretName, version), token: cancellationToken).ConfigureAwait(false);
                }
            }

            await FusionCache.RemoveAsync(trackingKey, token: cancellationToken).ConfigureAwait(false);
            await FusionCache.RemoveAsync(SecretValueCacheKey(secretName, null), token: cancellationToken).ConfigureAwait(false);
            await FusionCache.RemoveAsync(SecretVersionsCacheKey(secretName), token: cancellationToken).ConfigureAwait(false);
        }
        catch (SecretNotFoundException)
        {
            throw;
        }
        catch (SecretAccessDeniedException)
        {
            throw;
        }
        catch (SecretManagerRepositoryException)
        {
            throw;
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            SecretManagerRepositoryException.Throw(this, exception);
        }
    }

    public override async Task<SecretResult> SetSecretAsync(string secretName, string value, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secretName, nameof(secretName));
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));

        try
        {
            var result = await _underlyingSecretManagerRepository.SetSecretAsync(secretName, value, cancellationToken).ConfigureAwait(false);

            await FusionCache.SetAsync<string?>(SecretValueCacheKey(secretName, null), value, token: cancellationToken).ConfigureAwait(false);

            if (!string.IsNullOrWhiteSpace(result.CurrentVersion))
            {
                await FusionCache.SetAsync<string?>(SecretValueCacheKey(secretName, result.CurrentVersion), value, token: cancellationToken).ConfigureAwait(false);
                await TrackCachedVersionAsync(secretName, result.CurrentVersion, cancellationToken).ConfigureAwait(false);
            }

            await FusionCache.RemoveAsync(SecretVersionsCacheKey(secretName), token: cancellationToken).ConfigureAwait(false);

            return result;
        }
        catch (SecretNotFoundException)
        {
            throw;
        }
        catch (SecretAccessDeniedException)
        {
            throw;
        }
        catch (SecretManagerRepositoryException)
        {
            throw;
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            return SecretManagerRepositoryException.Throw<SecretResult>(this, exception);
        }
    }
}
