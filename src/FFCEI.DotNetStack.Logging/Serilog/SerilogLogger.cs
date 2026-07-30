using Microsoft.Extensions.Compliance.Redaction;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;

namespace FFCEI.DotNetStack.Logging.Serilog;

public static class SerilogLogger
{
    public static LoggerConfiguration CreateSerilogConfiguration(LogEventLevel? minimumLogLevel = null, IRedactorProvider? redactorProvider = null)
    {
        var logLevel = minimumLogLevel ?? (System.Diagnostics.Debugger.IsAttached ? LogEventLevel.Information : LogEventLevel.Debug);

        return new LoggerConfiguration().MinimumLevel.Is(logLevel).AddSerilogEnrichers(redactorProvider);
    }

    public static LoggerConfiguration CreateSerilogConfiguration(LogLevel minimumLogLevel, IRedactorProvider? redactorProvider = null)
        => CreateSerilogConfiguration(minimumLogLevel.ToLogEventLevel(), redactorProvider);

    public static LoggerConfiguration CreatePlatformBootstrapLoggerConfiguration(LogEventLevel? minimumLogLevel = null, bool logToSystemLogs = false, IRedactorProvider? redactorProvider = null)
    {
        var configuration = CreateSerilogConfiguration(minimumLogLevel, redactorProvider);

        _ = configuration.AddConsoleLogging();

        if (logToSystemLogs)
        {
            if (OperatingSystem.IsWindows())
            {
                _ = configuration.AddEventLogLogging();
            }

            if (OperatingSystem.IsLinux() || OperatingSystem.IsFreeBSD())
            {
                _ = configuration.AddSyslogLogging();
            }
        }

        if (System.Diagnostics.Debugger.IsAttached)
        {
            _ = configuration.AddDebugLogging();
        }

        return configuration;
    }

    public static LoggerConfiguration CreatePlatformBootstrapLoggerConfiguration(LogLevel minimumLogLevel, bool logToSystemLogs = false, IRedactorProvider? redactorProvider = null)
        => CreatePlatformBootstrapLoggerConfiguration(minimumLogLevel.ToLogEventLevel(), logToSystemLogs, redactorProvider);

    public static ILoggerFactory CreatePlatformBootstrapLoggerConfigurationFactory(LogEventLevel? minimumLogLevel = null, bool logToSystemLogs = false, IRedactorProvider? redactorProvider = null)
    {
        var configuration = CreatePlatformBootstrapLoggerConfiguration(minimumLogLevel, logToSystemLogs, redactorProvider);

        Log.Logger = configuration.CreateBootstrapLogger();

        return new SerilogLoggerFactory(Log.Logger);
    }

    public static ILoggerFactory CreatePlatformBootstrapLoggerConfigurationFactory(LogLevel minimumLogLevel, bool logToSystemLogs = false, IRedactorProvider? redactorProvider = null)
        => CreatePlatformBootstrapLoggerConfigurationFactory(minimumLogLevel.ToLogEventLevel(), logToSystemLogs, redactorProvider);
}
