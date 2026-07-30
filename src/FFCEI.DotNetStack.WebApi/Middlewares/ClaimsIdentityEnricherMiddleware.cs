using FFCEI.DotNetStack.AccessControl.Interfaces;
using FFCEI.DotNetStack.Framework;
using FFCEI.DotNetStack.WebApi.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace FFCEI.DotNetStack.WebApi.Middlewares;

public sealed class ClaimsIdentityEnricherMiddleware(RequestDelegate nextMiddleware, ILogger<ClaimsIdentityEnricherMiddleware> logger)
    : AspNetCoreHttpMiddleware(nextMiddleware, logger)
{
    private static OnceFlag WarnedNoEnricher;

    public async Task InvokeAsync(HttpContext httpContext, IClaimsIdentityEnricher? enricher)
    {
        ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

        if (enricher == null)
        {
            if (WarnedNoEnricher.TestAndSet())
            {
                WebApiLog.NoClaimsIdentityEnricherAvailable(logger);
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
            if (httpContext.User.Identity is not ClaimsIdentity identity)
            {
                WebApiLog.NoClaimsIdentityAvailable(logger);

                await AbortProcessingWithForbiddenErrorAsync(httpContext).ConfigureAwait(false);

                return;
            }
            else
            {
                await enricher.EnrichAsync(identity).ConfigureAwait(false);
            }
        }

        await ProcessNextMiddlewareAsync(httpContext).ConfigureAwait(false);
    }
}
