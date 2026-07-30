using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.Logging.Serilog;
using Microsoft.Extensions.Logging;

namespace FFCEI.DotNetStack.Logging;

public sealed class BootstrapLogger
{
    private static ILoggerFactory? _factory;
    private static ILogger? _logger;

    public static ILogger Instance
    {
        get
        {
            if (_logger == null)
            {
                _logger = Create();
            }

            return _logger;
        }
    }

    public static ILogger Create(LogLevel? minimumLogLevel = null, bool logToSystemLogs = false)
    {
        var logEventLevel = minimumLogLevel.ToLogEventLevel();

        _factory.DisposeObjectSafelyIfNeeded();

        _factory = SerilogLogger.CreatePlatformBootstrapLoggerConfigurationFactory(logEventLevel, logToSystemLogs);
        _logger = _factory.CreateLogger("Bootstrap");

        return _logger;
    }
}
