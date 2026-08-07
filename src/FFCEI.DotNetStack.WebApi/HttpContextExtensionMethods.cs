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

    public static string? AuthorizationHeader(this HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

        return httpContext.Request.AuthorizationHeader();
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

    public static bool HasApiKeyHeader(this HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

        return httpContext.Request.HasApiKeyHeader();
    }

    public static string? ApiKeyHeader(this HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

        return httpContext.Request.ApiKeyHeader();
    }

    public static string? ClientSecretHeader(this HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

        return httpContext.Request.ClientSecretHeader();
    }

    public static (string ApiKey, string? ClientSecret)? ApiKeyAuthentication(this HttpContext httpContext, string? apiKeyHeaderName = null, string? clientSecretHeaderName = null)
    {
        ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

        return httpContext.Request.ApiKeyAuthentication(apiKeyHeaderName, clientSecretHeaderName);
    }
}
