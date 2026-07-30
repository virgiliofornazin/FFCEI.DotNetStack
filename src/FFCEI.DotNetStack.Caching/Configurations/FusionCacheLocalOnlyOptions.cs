namespace FFCEI.DotNetStack.Caching.Configurations;

public class FusionCacheLocalOnlyOptions
    : FusionCacheOptions
{
    internal override void Copy(FusionCacheOptions options)
    {
        base.Copy(options);

        if (options is FusionCacheLocalOnlyOptions localOnlyOptions)
        {
            LocalMemoryCacheEntryLifetime = localOnlyOptions.LocalMemoryCacheEntryLifetime;
        }
    }

    public TimeSpan? LocalMemoryCacheEntryLifetime { get; set; }
}
