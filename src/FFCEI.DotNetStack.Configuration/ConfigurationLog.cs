using Microsoft.Extensions.Logging;

namespace FFCEI.DotNetStack.Configuration;

internal static partial class ConfigurationLog
{
    [LoggerMessage(Level = LogLevel.Debug, Message = "Searching environment variable(s) files named {FileName} into {Location}")]
    internal static partial void SearchingEnvironmentVariablesFileIntoLocation(ILogger logger, string fileName, string location);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Attempting to load environment variable(s) from {FileName}")]
    internal static partial void AttemptingToLoadEnvironmentVariablesFile(ILogger logger, string fileName);

    [LoggerMessage(Level = LogLevel.Information, Message = "Loaded {Count} environment variable(s) from {FileName}")]
    internal static partial void LoadedEnvironmentVariablesFile(ILogger logger, int count, string fileName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "No environment variable(s) were loaded from {FileName} (empty file?)")]
    internal static partial void EmptyEnvironmentVariablesFile(ILogger logger, string fileName);
}
