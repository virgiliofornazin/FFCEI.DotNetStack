namespace FFCEI.DotNetStack.Caching.Configurations;

public abstract class FusionCacheOptions
{
    internal virtual void Copy(FusionCacheOptions options)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));

        CacheName = options.CacheName;
        MaxEntries = options.MaxEntries;
        DebugLogging = options.DebugLogging;
    }

    public string? CacheName { get; set; }
    public long? MaxEntries { get; set; }
    public bool? DebugLogging { get; set; }
}
