using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;

namespace FFCEI.DotNetStack.WebApi.Common;

public abstract class AspNetCoreHttpMiddleware(RequestDelegate nextMiddleware, ILogger logger)
{
    public RequestDelegate NextMiddleware => nextMiddleware;
    public ILogger Logger => logger;

    public Task ProcessNextMiddlewareAsync(HttpContext httpContext)
        => NextMiddleware.Invoke(httpContext);

    protected static async Task AbortProcessingWithErrorAsync(HttpContext httpContext, string? plainTextMessage = null, int? statusCode = null)
    {
        ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

        httpContext.Response.ContentType = "text/plain";
        httpContext.Response.StatusCode = statusCode ?? StatusCodes.Status500InternalServerError;

        if (httpContext.Response.StatusCode < 400)
        {
            httpContext.Response.StatusCode = 500;
        }

        var plainText = Encoding.UTF8.GetBytes(plainTextMessage ?? "Internal server error");

        await httpContext.Response.Body.WriteAsync(plainText).ConfigureAwait(false);
    }

    protected static Task AbortProcessingWithServiceUnavailableErrorAsync(HttpContext httpContext, string? plainTextMessage = null)
        => AbortProcessingWithErrorAsync(httpContext, plainTextMessage ?? "Service unavailable", StatusCodes.Status503ServiceUnavailable);

    protected static Task AbortProcessingWithUnauthorizedErrorAsync(HttpContext httpContext, string? plainTextMessage = null)
        => AbortProcessingWithErrorAsync(httpContext, plainTextMessage ?? "Unauthorized", StatusCodes.Status401Unauthorized);

    protected static Task AbortProcessingWithForbiddenErrorAsync(HttpContext httpContext, string? plainTextMessage = null)
        => AbortProcessingWithErrorAsync(httpContext, plainTextMessage ?? "Forbidden", StatusCodes.Status403Forbidden);
}
