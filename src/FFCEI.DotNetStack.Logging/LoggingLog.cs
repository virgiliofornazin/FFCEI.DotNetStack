using Microsoft.Extensions.Logging;

namespace FFCEI.DotNetStack.Logging;

internal static partial class LoggingLog
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "Adding Serilog console logging")]
    internal static partial void AddSerilogConsoleLogging(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Adding Serilog debugger logging")]
    internal static partial void AddSerilogDebuggerLogging(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Adding Serilog OpenTelemetry logging")]
    internal static partial void AddSerilogOpenTelemetryLogging(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Adding Serilog Seq logging")]
    internal static partial void AddSerilogSeqLogging(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Adding Serilog Syslog logging")]
    internal static partial void AddSerilogSyslogLogging(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Adding Serilog Windows EventLog logging")]
    internal static partial void AddSerilogWindowsEventLogLogging(ILogger logger);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Creating Serilog configuration")]
    internal static partial void CreatingSerilogConfiguration(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Serilog configured")]
    internal static partial void SerilogConfigured(ILogger logger);
}
