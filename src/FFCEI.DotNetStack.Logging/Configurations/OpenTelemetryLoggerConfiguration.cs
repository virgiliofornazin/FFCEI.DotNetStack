using FFCEI.DotNetStack.Logging.Configurations.Common;
using Serilog.Sinks.OpenTelemetry;

namespace FFCEI.DotNetStack.Logging.Configurations;

public sealed class OpenTelemetryLoggerConfiguration : LoggerConfiguration
{
    public Uri? Uri { get; set; }
    public OtlpProtocol? Protocol { get; set; }
    public string? Authorization { get; set; }
}
