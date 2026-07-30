using Microsoft.Extensions.Logging;
using System.Reflection;

namespace FFCEI.DotNetStack.EntityFramework.Configurations;

public class DataProtectionConfigurationOptions
{
    public static readonly TimeSpan DefaultKeyLifetime = TimeSpan.FromDays(30);

    public string ApplicationName { get; set; } = Assembly.GetEntryAssembly()?.GetName().Name ?? string.Empty;
    public TimeSpan? KeyLifetime { get; set; } = DefaultKeyLifetime;
    public LogLevel? DataProtectionLogLevel { get; set; }
    public bool DisableAutomaticKeyGeneration { get; set; }
}
