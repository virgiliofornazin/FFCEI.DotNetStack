namespace FFCEI.DotNetStack.Caching.Configurations;

public class FusionCacheDistributedOptions
    : FusionCacheLocalOnlyOptions
{
    public FusionCacheDistributedOptions()
    {
    }

    internal FusionCacheDistributedOptions(FusionCacheLocalOnlyOptions options)
    {
        Copy(options);
    }

    public bool UseDistributedCache { get; set; }
    public TimeSpan? DistributedCacheEntryLifetime { get; set; }
    public string? DistributedCacheRedisConnectionString { get; set; }
    public string? BackplaneRedisConnectionString { get; set; }
}
