using Microsoft.AspNetCore.Http;

namespace FFCEI.DotNetStack.WebApi;

public static class HttpContextExtensionMethods
{
    public static string? RealRemoteIpAddress(this HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

        return httpContext.Request.RealRemoteIpAddress();
    }

    public static bool HasAuthorizationHeader(this HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

        return httpContext.Request.HasAuthorizationHeader();
    }

    public static string? BearerToken(this HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

        return httpContext.Request.BearerToken();
    }

    public static (string User, string Password)? BasicAuthentication(this HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

        return httpContext.Request.BasicAuthentication();
    }
}
