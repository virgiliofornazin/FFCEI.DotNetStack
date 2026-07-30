using FFCEI.DotNetStack.Caching.Configurations;
using ZiggyCreatures.Caching.Fusion;

namespace FFCEI.DotNetStack.Secrets.Configurations;

public sealed class SecretManagerRepositoryOptions
{
    public FusionCacheLocalOnlyOptions? CacheOptions { get; set; }
    public Action<IFusionCacheBuilder>? FusionCacheBuilder { get; set; }
}
