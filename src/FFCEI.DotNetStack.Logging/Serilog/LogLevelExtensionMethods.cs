using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace FFCEI.DotNetStack.Logging.Serilog;

public static class LogLevelExtensionMethods
{
    public static LogEventLevel? ToLogEventLevel(this LogLevel? logLevel)
        => logLevel.HasValue ? ToLogEventLevel(logLevel.Value) : null;

    public static LogEventLevel ToLogEventLevel(this LogLevel logLevel)
        => logLevel switch
        {
            LogLevel.Trace => LogEventLevel.Verbose,
            LogLevel.Debug => LogEventLevel.Debug,
            LogLevel.Information => LogEventLevel.Information,
            LogLevel.Warning => LogEventLevel.Warning,
            LogLevel.Error => LogEventLevel.Error,
            LogLevel.Critical => LogEventLevel.Fatal,
            _ => throw new InvalidOperationException($"Unsupported log level {logLevel}")
        };
}
