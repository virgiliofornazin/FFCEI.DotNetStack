using FFCEI.DotNetStack.AccessControl.Interfaces;
using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.WebApi.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace FFCEI.DotNetStack.WebApi.Middlewares;

public sealed class SecurityTokenValidationMiddleware(RequestDelegate nextMiddleware, ILogger<SecurityTokenValidationMiddleware> logger)
    : AspNetCoreHttpMiddleware(nextMiddleware, logger)
{
    private static OnceFlag WarnedNoPostValidator;

    public async Task InvokeAsync(HttpContext httpContext, ISecurityTokenValidator? validator)
    {
        ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

        if (validator == null)
        {
            if (WarnedNoPostValidator.TestAndSet())
            {
                WebApiLog.NoSecurityTokenValidatorAvailable(logger);
            }

            await AbortProcessingWithServiceUnavailableErrorAsync(httpContext).ConfigureAwait(false);

            return;
        }

        var token = httpContext.BearerToken();

        if (token == null)
        {
            if (httpContext.HasAuthorizationHeader())
            {
                await AbortProcessingWithUnauthorizedErrorAsync(httpContext).ConfigureAwait(false);

                return;
            }
        }
        else
        {
            var valid = await validator.ValidateTokenAsync(token).ConfigureAwait(false);

            if (!valid)
            {
                await AbortProcessingWithUnauthorizedErrorAsync(httpContext).ConfigureAwait(false);

                return;
            }
        }

        await ProcessNextMiddlewareAsync(httpContext).ConfigureAwait(false);
    }
}
