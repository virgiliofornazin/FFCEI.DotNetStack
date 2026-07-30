using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System.Text;

namespace FFCEI.DotNetStack.WebApi;

public static class HttpRequestExtensionMethods
{
    private const char RealRemoteIpAddressSplitChar = ',';
    private const char BasicUserPasswordSplitChar = ':';
    private const string CloudfareConnectingIpHeader = "CF-Connecting-IP";
    private const string ReverseProxyXForwardedForHeader = "X-Forwarded-For";

    public static string? RealRemoteIpAddress(this HttpRequest httpRequest)
    {
        ArgumentNullException.ThrowIfNull(httpRequest, nameof(httpRequest));

        if (httpRequest.Headers.TryGetValue(CloudfareConnectingIpHeader, out var cloudfareConnectingIpHeaderValue))
        {
            var realRequestAddress = cloudfareConnectingIpHeaderValue.ToString().Split(RealRemoteIpAddressSplitChar).FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(realRequestAddress))
            {
                return realRequestAddress;
            }
        }

        if (httpRequest.Headers.TryGetValue(ReverseProxyXForwardedForHeader, out var reverseProxyXForwardedForHeaderValue))
        {
            var realRequestAddress = reverseProxyXForwardedForHeaderValue.ToString().Split(RealRemoteIpAddressSplitChar).FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(realRequestAddress))
            {
                return realRequestAddress;
            }
        }

        var remoteIpAdress = httpRequest.HttpContext?.Connection?.RemoteIpAddress;

        if (remoteIpAdress != null)
        {
            var realRequestAddress = remoteIpAdress.ToString();

            if (!string.IsNullOrWhiteSpace(realRequestAddress))
            {
                return realRequestAddress;
            }
        }

        return null;
    }

    public static bool HasAuthorizationHeader(this HttpRequest httpRequest)
    {
        ArgumentNullException.ThrowIfNull(httpRequest, nameof(httpRequest));

        return httpRequest.Headers.ContainsKey(HeaderNames.Authorization);
    }

    public static string? BearerToken(this HttpRequest httpRequest)
    {
        ArgumentNullException.ThrowIfNull(httpRequest, nameof(httpRequest));

        if (!httpRequest.Headers.TryGetValue(HeaderNames.Authorization, out var authorizationHeaderValue))
        {
            return null;
        }

        var authorizationHeader = authorizationHeaderValue.ToString();

        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            return null;
        }

        if (authorizationHeader.Length < 8)
        {
            return null;
        }

        var bearerPrefix = authorizationHeader[..7].ToUpperInvariant();

        if (bearerPrefix != "BEARER ")
        {
            return null;
        }

        var jwtToken = authorizationHeader[7..];

        return string.IsNullOrWhiteSpace(jwtToken) ? null : jwtToken;
    }

    public static (string User, string Password)? BasicAuthentication(this HttpRequest httpRequest)
    {
        ArgumentNullException.ThrowIfNull(httpRequest, nameof(httpRequest));

        if (!httpRequest.Headers.TryGetValue(HeaderNames.Authorization, out var authorizationHeaderValue))
        {
            return null;
        }

        var authorizationHeader = authorizationHeaderValue.ToString();

        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            return null;
        }

        if (authorizationHeader.Length < 7)
        {
            return null;
        }

        var basicPrefix = authorizationHeader[..6].ToUpperInvariant();

        if (basicPrefix != "BASIC ")
        {
            return null;
        }

        byte[] decodedBytes;

        try
        {
            decodedBytes = Convert.FromBase64String(authorizationHeader[6..]);
        }
        catch (FormatException)
        {
            return null;
        }

        var userPassword = Encoding.UTF8.GetString(decodedBytes).Split(BasicUserPasswordSplitChar, 2);

        var user = userPassword.Length > 0 ? userPassword[0] : string.Empty;
        var password = userPassword.Length > 1 ? userPassword[1] : string.Empty;

        return (user, password);
    }
}
