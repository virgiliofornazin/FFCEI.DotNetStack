using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace FFCEI.DotNetStack.Logging.Serilog;

public static class LogEventLevelExtensionMethods
{
    public static LogLevel? ToLogLevel(this LogEventLevel? logEventLevel)
        => logEventLevel.HasValue ? ToLogLevel(logEventLevel.Value) : null;

    public static LogLevel? ToLogLevel(this LogEventLevel logEventLevel)
        => logEventLevel switch
        {
            LogEventLevel.Verbose => LogLevel.Trace,
            LogEventLevel.Debug => LogLevel.Debug,
            LogEventLevel.Information => LogLevel.Information,
            LogEventLevel.Warning => LogLevel.Warning,
            LogEventLevel.Error => LogLevel.Error,
            LogEventLevel.Fatal => LogLevel.Critical,
            _ => throw new InvalidOperationException($"Unsupported log event level {logEventLevel}")
        };
}
