using Microsoft.Extensions.Logging;

namespace FFCEI.DotNetStack.Compliance;

internal static partial class ComplianceLog
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Compliance Redactors configured")]
    internal static partial void ComplianceConfigured(ILogger logger);
}
