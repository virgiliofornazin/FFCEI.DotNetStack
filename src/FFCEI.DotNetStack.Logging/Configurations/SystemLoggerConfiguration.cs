using FFCEI.DotNetStack.Logging.Configurations.Common;

namespace FFCEI.DotNetStack.Logging.Configurations;

public sealed class SystemLoggerConfiguration : LoggerConfiguration
{
    public string? WindowsEventLogSource { get; set; }
    public string? WindowsEventLogName { get; set; }
    public string? WindowsEventLogMachineName { get; set; }
    public bool WindowsEventLogManageEventSource { get; set; }
    public string? SyslogHost { get; set; }
    public ushort? SyslogPort { get; set; }
    public string? SyslogAppName { get; set; }
}
