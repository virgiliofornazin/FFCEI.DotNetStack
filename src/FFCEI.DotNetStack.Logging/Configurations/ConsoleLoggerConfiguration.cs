using FFCEI.DotNetStack.Logging.Configurations.Common;

namespace FFCEI.DotNetStack.Logging.Configurations;

public sealed class ConsoleLoggerConfiguration : LoggerConfiguration
{
    public bool JsonLogging { get; set; }
}
