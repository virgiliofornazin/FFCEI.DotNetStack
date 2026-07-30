using FFCEI.DotNetStack.Logging.Configurations.Common;

namespace FFCEI.DotNetStack.Logging.Configurations;

public sealed class LoggerFactoryConfiguration : LoggerConfiguration
{
    public ConsoleLoggerConfiguration? Console { get; set; }
    public DebuggerLoggerConfiguration? Debugger { get; set; }
    public SystemLoggerConfiguration? System { get; set; }
    public OpenTelemetryLoggerConfiguration? OpenTelemetry { get; set; }
    public SeqLoggerConfiguration? Seq { get; set; }
}
