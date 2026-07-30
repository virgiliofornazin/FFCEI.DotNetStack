using FFCEI.DotNetStack.Caching.Configurations;

namespace FFCEI.DotNetStack.EntityFramework.Configurations;

public sealed class EFCoreCachingSecondLevelCacheOptions
{
    public FusionCacheDistributedOptions CacheOptions { get; set; } = new();
    public bool CacheEmptyResults { get; set; }
    public string? EntityCacheEntryPrefix { get; set; }
}
