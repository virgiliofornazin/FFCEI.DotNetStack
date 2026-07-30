using FFCEI.DotNetStack.Logging.Configurations.Common;

namespace FFCEI.DotNetStack.Logging.Configurations;

public sealed class SeqLoggerConfiguration : LoggerConfiguration
{
    public Uri? Uri { get; set; }
    public string? ApiKey { get; set; }
}
