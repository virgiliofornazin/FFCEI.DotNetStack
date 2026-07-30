using Microsoft.AspNetCore.Http;

namespace FFCEI.DotNetStack.WebApi;

public static class HttpContextExtensionMethods
{
    private const string RealRemoteIpAddressItem = "RealRemoteIpAddress";

    public static string? RealRemoteIpAddress(this HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext, nameof(httpContext));

        if (httpContext.Items.TryGetValue(RealRemoteIpAddressItem, out var cachedRealRemoteIpAddress))
        {
            return cachedRealRemoteIpAddress?.ToString();
        }

        var realRemoteIpAddress = httpContext.Request.RealRemoteIpAddress();

        if (!string.IsNullOrWhiteSpace(realRemoteIpAddress))
        {
            httpContext.Items[RealRemoteIpAddressItem] = realRemoteIpAddress;
        }

        return realRemoteIpAddress;
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
