using Microsoft.Extensions.Logging;

namespace FFCEI.DotNetStack.Caching;

internal static partial class CachingLog
{
    [LoggerMessage(Level = LogLevel.Information, Message = "FusionCache '{FusionCacheName}' configured (distributed: {UseDistributedCache})")]
    internal static partial void FusionCacheConfigured(ILogger logger, string fusionCacheName, bool useDistributedCache);
}
