using Microsoft.Extensions.Logging;

namespace FFCEI.DotNetStack.Logging.Configurations.Common;

public abstract class LoggerConfiguration
{
    public LogLevel? MinimumLogLevel { get; set; }
}
