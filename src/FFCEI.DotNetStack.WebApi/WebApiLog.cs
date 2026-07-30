using Microsoft.Extensions.Logging;

namespace FFCEI.DotNetStack.WebApi;

internal static partial class WebApiLog
{
    [LoggerMessage(Level = LogLevel.Error, Message = "No Security Token Validator available; rejecting request with 'Service Unavailable'")]
    internal static partial void NoSecurityTokenValidatorAvailable(ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "No Claims Identity Enricher available; rejecting request with 'Service Unavailable'")]
    internal static partial void NoClaimsIdentityEnricherAvailable(ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "No Claims Identity available; rejecting request with 'Forbidden'")]
    internal static partial void NoClaimsIdentityAvailable(ILogger logger);
}
