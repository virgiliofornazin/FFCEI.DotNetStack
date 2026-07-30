using FFCEI.DotNetStack.Configuration.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FFCEI.DotNetStack.Configuration;

public static class IServiceProviderExtensionMethods
{
    public static IReadOnlyCollection<KeyValuePair<string, string>> UseEnvFiles(this IServiceProvider serviceProvider,
        EnvFileLoadOptions? options = null)
    {
        var logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(EnvFile));
        var result = EnvFile.LoadEnvFiles(options, logger);

        return result;
    }

    public static IReadOnlyCollection<KeyValuePair<string, string>> UseDefaultEnvFileOnly(this IServiceProvider serviceProvider,
        EnvFileLoadOptions? options = null)
    {
        var logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(EnvFile));
        var result = EnvFile.LoadDefaultEnvFile(options, logger);

        return result;
    }

    public static IReadOnlyCollection<KeyValuePair<string, string>> UseDefaultLocalEnvFileOnly(this IServiceProvider serviceProvider,
        EnvFileLoadOptions? options = null)
    {
        var logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(EnvFile));
        var result = EnvFile.LoadDefaultLocalEnvFile(options, logger);

        return result;
    }
}
