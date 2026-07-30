using Microsoft.Extensions.Compliance.Redaction;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Json;
using Serilog.Redaction;
using Serilog.Sinks.OpenTelemetry;
using Serilog.Sinks.Syslog;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace FFCEI.DotNetStack.Logging.Serilog;

public static class LoggerConfigurationExtensionMethods
{
    private const string OutputContentsTemplate = "{Message:lj}{NewLine}{Exception}";
    private const string OutputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss.fffffff}] [{Level:u3}] (Thread {ThreadId} at {AssemblyName}, source context: {SourceContext}) => " + OutputContentsTemplate;

    public static LoggerConfiguration AddSerilogEnrichers(this LoggerConfiguration loggerConfiguration, IRedactorProvider? redactorProvider = null)
    {
        ArgumentNullException.ThrowIfNull(loggerConfiguration, nameof(loggerConfiguration));

        if (redactorProvider != null)
        {
            _ = loggerConfiguration
                .Destructure.WithRedaction(redactorProvider);
        }

        loggerConfiguration = loggerConfiguration
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithAssemblyName()
            .Enrich.WithThreadId()
            .Enrich.WithThreadName()
            .Enrich.WithExceptionDetails()
            .Enrich.WithSpan(spanOptions: new SpanOptions()
            {
                IncludeTraceFlags = true,
                IncludeBaggage = true,
                IncludeTags = true,
                IncludeOperationName = true
            });

        bool runningOnContainer = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

        if (!runningOnContainer)
        {
            loggerConfiguration = loggerConfiguration
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentUserName()
                .Enrich.WithAssemblyVersion()
                .Enrich.WithProcessId()
                .Enrich.WithProcessName();
        }

        return loggerConfiguration;
    }

    public static LoggerConfiguration AddConsoleLogging(this LoggerConfiguration loggerConfiguration, bool jsonLogging = false, LogEventLevel? minimumLogLevel = null)
    {
        ArgumentNullException.ThrowIfNull(loggerConfiguration, nameof(loggerConfiguration));

        var restrictedToMinimumLevel = minimumLogLevel ?? LogEventLevel.Verbose;

        if (jsonLogging)
        {
            _ = loggerConfiguration.WriteTo.Console(
                formatter: new JsonFormatter(renderMessage: true),
                restrictedToMinimumLevel: restrictedToMinimumLevel,
                standardErrorFromLevel: LogEventLevel.Warning);
        }
        else
        {
            _ = loggerConfiguration.WriteTo.Console(
                outputTemplate: OutputTemplate,
                formatProvider: CultureInfo.InvariantCulture,
                restrictedToMinimumLevel: restrictedToMinimumLevel,
                standardErrorFromLevel: LogEventLevel.Warning);
        }

        return loggerConfiguration;
    }

    public static LoggerConfiguration AddDebugLogging(this LoggerConfiguration loggerConfiguration, bool jsonLogging = false, LogEventLevel? minimumLogLevel = null)
    {
        ArgumentNullException.ThrowIfNull(loggerConfiguration, nameof(loggerConfiguration));

        var restrictedToMinimumLevel = minimumLogLevel ?? LogEventLevel.Verbose;

        if (jsonLogging)
        {
            _ = loggerConfiguration.WriteTo.Debug(
                formatter: new JsonFormatter(renderMessage: true),
                restrictedToMinimumLevel: restrictedToMinimumLevel);
        }
        else
        {
            _ = loggerConfiguration.WriteTo.Debug(
                outputTemplate: OutputTemplate,
                formatProvider: CultureInfo.InvariantCulture,
                restrictedToMinimumLevel: restrictedToMinimumLevel);
        }

        return loggerConfiguration;
    }

    public static LoggerConfiguration AddEventLogLogging(this LoggerConfiguration loggerConfiguration, string? source = null, string? logName = null, string? machineName = null,
        bool manageEventSource = false, LogEventLevel? minimumLogLevel = null)
    {
        ArgumentNullException.ThrowIfNull(loggerConfiguration, nameof(loggerConfiguration));

        if (!OperatingSystem.IsWindows())
        {
            throw new InvalidOperationException("Windows EventLog not supported on this platform");
        }

        source ??= (Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly())?.GetName().Name ?? "unknown";
        machineName ??= ".";

        var restrictedToMinimumLevel = minimumLogLevel ?? LogEventLevel.Verbose;

        _ = loggerConfiguration.WriteTo.Async(sinkConfiguration =>
        {
            if (!OperatingSystem.IsWindows())
            {
                throw new InvalidOperationException("Windows EventLog not supported on this platform");
            }

            _ = sinkConfiguration.EventLog(
                outputTemplate: OutputTemplate,
                formatProvider: CultureInfo.InvariantCulture,
                restrictedToMinimumLevel: restrictedToMinimumLevel,
                source: source,
                logName: logName,
                machineName: machineName,
                manageEventSource: manageEventSource);
        });

        return loggerConfiguration;
    }

    public static LoggerConfiguration AddFileLogging(this LoggerConfiguration loggerConfiguration, string path, bool jsonLogging = false, LogEventLevel? minimumLogLevel = null, long? fileSizeLimitBytes = null, bool buffered = true,
        bool shared = true, TimeSpan? flushToDiskInterval = null, RollingInterval rollingInterval = RollingInterval.Infinite, bool rollOnFileSizeLimit = false, int? retainedFileCountLimit = null, Encoding? encoding = null)
    {
        ArgumentNullException.ThrowIfNull(loggerConfiguration, nameof(loggerConfiguration));

        encoding ??= Encoding.UTF8;

        var restrictedToMinimumLevel = minimumLogLevel ?? LogEventLevel.Verbose;

        if (jsonLogging)
        {
            _ = loggerConfiguration.WriteTo.Async(sinkConfiguration => sinkConfiguration.File(
                formatter: new JsonFormatter(renderMessage: true),
                path: path,
                restrictedToMinimumLevel: restrictedToMinimumLevel,
                fileSizeLimitBytes: fileSizeLimitBytes,
                buffered: buffered,
                shared: shared,
                flushToDiskInterval: flushToDiskInterval,
                rollingInterval: rollingInterval,
                rollOnFileSizeLimit: rollOnFileSizeLimit,
                retainedFileCountLimit: retainedFileCountLimit,
                encoding: encoding));
        }
        else
        {
            _ = loggerConfiguration.WriteTo.Async(sinkConfiguration => sinkConfiguration.File(
                path: path,
                restrictedToMinimumLevel: restrictedToMinimumLevel,
                outputTemplate: OutputTemplate,
                formatProvider: CultureInfo.InvariantCulture,
                fileSizeLimitBytes: fileSizeLimitBytes,
                buffered: buffered,
                shared: shared,
                flushToDiskInterval: flushToDiskInterval,
                rollingInterval: rollingInterval,
                rollOnFileSizeLimit: rollOnFileSizeLimit,
                retainedFileCountLimit: retainedFileCountLimit,
                encoding: encoding));
        }

        return loggerConfiguration;
    }

    public static LoggerConfiguration AddOpenTelemetryLogging(this LoggerConfiguration loggerConfiguration, string endpoint, OtlpProtocol protocol = OtlpProtocol.HttpProtobuf, string? authorization = null, LogEventLevel? minimumLogLevel = null)
    {
        ArgumentNullException.ThrowIfNull(loggerConfiguration, nameof(loggerConfiguration));
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint, nameof(endpoint));

        var restrictedToMinimumLevel = minimumLogLevel ?? LogEventLevel.Verbose;

        _ = loggerConfiguration.WriteTo.Async(sinkConfiguration => sinkConfiguration.OpenTelemetry(
            endpoint: endpoint,
            protocol: protocol,
            headers: string.IsNullOrWhiteSpace(authorization) ? null : new Dictionary<string, string>
            {
                ["Authorization"] = authorization
            },
            restrictedToMinimumLevel: restrictedToMinimumLevel));

        return loggerConfiguration;
    }

    public static LoggerConfiguration AddSeqLogging(this LoggerConfiguration loggerConfiguration, Uri serverUrl, string? apiKey = null, LogEventLevel? minimumLogLevel = null)
    {
        ArgumentNullException.ThrowIfNull(loggerConfiguration, nameof(loggerConfiguration));
        ArgumentNullException.ThrowIfNull(serverUrl, nameof(serverUrl));

        var restrictedToMinimumLevel = minimumLogLevel ?? LogEventLevel.Verbose;

        _ = loggerConfiguration.WriteTo.Async(sinkConfiguration => sinkConfiguration.Seq(
            serverUrl: serverUrl.ToString(),
            apiKey: apiKey,
            restrictedToMinimumLevel: restrictedToMinimumLevel,
            formatProvider: CultureInfo.InvariantCulture));

        return loggerConfiguration;
    }

    public static LoggerConfiguration AddSyslogLogging(this LoggerConfiguration loggerConfiguration, string? host = null, ushort? port = null, string? appName = null, LogEventLevel? minimumLogLevel = null)
    {
        ArgumentNullException.ThrowIfNull(loggerConfiguration, nameof(loggerConfiguration));

        var restrictedToMinimumLevel = minimumLogLevel ?? LogEventLevel.Verbose;

        appName ??= (Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly())?.GetName().Name ?? "unknown";

        if (string.IsNullOrWhiteSpace(host) && (port == null))
        {
            _ = loggerConfiguration.WriteTo.Async(sinkConfiguration => sinkConfiguration.LocalSyslog(
                appName: appName,
                facility: Facility.Local0,
                restrictedToMinimumLevel: restrictedToMinimumLevel,
                outputTemplate: OutputContentsTemplate));
        }
        else
        {
            var udpHost = string.IsNullOrWhiteSpace(host) ? "127.0.0.1" : host;
            var udpPort = !port.HasValue || port < 1 ? 514 : port.Value;

            _ = loggerConfiguration.WriteTo.Async(sinkConfiguration => sinkConfiguration.UdpSyslog(
                host: udpHost,
                port: udpPort,
                appName: appName,
                facility: Facility.Local0,
                restrictedToMinimumLevel: restrictedToMinimumLevel,
                outputTemplate: OutputContentsTemplate));
        }

        return loggerConfiguration;
    }
}
