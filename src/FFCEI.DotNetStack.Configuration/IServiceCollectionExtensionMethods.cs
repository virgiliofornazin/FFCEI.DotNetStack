using FFCEI.DotNetStack.Configuration.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FFCEI.DotNetStack.Configuration;

public static class IServiceCollectionExtensionMethods
{
    public static IReadOnlyCollection<KeyValuePair<string, string>> AddEnvFiles(this IServiceCollection _,
        EnvFileLoadOptions? options = null, ILogger? logger = null)
        => EnvFile.LoadEnvFiles(options, logger);

    public static IReadOnlyCollection<KeyValuePair<string, string>> AddDefaultEnvFileOnly(this IServiceCollection _,
        EnvFileLoadOptions? options = null, ILogger? logger = null)
        => EnvFile.LoadDefaultEnvFile(options, logger);

    public static IReadOnlyCollection<KeyValuePair<string, string>> AddDefaultLocalEnvFileOnly(this IServiceCollection _,
        EnvFileLoadOptions? options = null, ILogger? logger = null)
        => EnvFile.LoadDefaultLocalEnvFile(options, logger);
}
