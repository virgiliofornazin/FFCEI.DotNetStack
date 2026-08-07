using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Text;

namespace FFCEI.DotNetStack.WebApi;

public static class HttpRequestExtensionMethods
{
    private const char RealRemoteIpAddressSplitChar = ',';
    private const char BasicUserPasswordSplitChar = ':';
    private const string CloudfareConnectingIpHeaderName = "CF-Connecting-IP";
    private const string ReverseProxyXForwardedForHeaderName = "X-Forwarded-For";
    private const string ApiKeyHeaderName = "X-API-Key";
    private const string ClientSecretHeaderName = "X-Client-Secret";

    public static string? RealRemoteIpAddress(this HttpRequest httpRequest)
    {
        ArgumentNullException.ThrowIfNull(httpRequest, nameof(httpRequest));

        if (httpRequest.Headers.TryGetValue(CloudfareConnectingIpHeaderName, out var cloudfareConnectingIpHeaderValue))
        {
            var realRequestAddress = cloudfareConnectingIpHeaderValue.ToString().Split(RealRemoteIpAddressSplitChar).FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(realRequestAddress))
            {
                return realRequestAddress;
            }
        }

        var forwardedOptions = httpRequest.HttpContext?.RequestServices?.GetService<IOptions<ForwardedHeadersOptions>>()?.Value;
        var forwardedOptionsConfigured = (forwardedOptions != null) && forwardedOptions.ForwardedHeaders.HasFlag(ForwardedHeaders.XForwardedFor);

        if (!forwardedOptionsConfigured &&
            httpRequest.Headers.TryGetValue(ReverseProxyXForwardedForHeaderName, out var reverseProxyXForwardedForHeaderValue))
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

        if (forwardedOptionsConfigured &&
            httpRequest.Headers.TryGetValue(ReverseProxyXForwardedForHeaderName, out var reverseProxyXForwardedForHeaderValueFallback))
        {
            var realRequestAddress = reverseProxyXForwardedForHeaderValueFallback.ToString().Split(RealRemoteIpAddressSplitChar).FirstOrDefault();

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

        return !string.IsNullOrWhiteSpace(httpRequest.AuthorizationHeader());
    }

    public static string? AuthorizationHeader(this HttpRequest httpRequest)
    {
        ArgumentNullException.ThrowIfNull(httpRequest, nameof(httpRequest));

        if (!httpRequest.Headers.TryGetValue(HeaderNames.Authorization, out var authorizationHeaderValue))
        {
            return null;
        }

        var authorizationHeader = authorizationHeaderValue.ToString();

        if (string.IsNullOrWhiteSpace(authorizationHeader) || (authorizationHeader.ToUpperInvariant().Trim() == "NULL"))
        {
            return null;
        }

        return authorizationHeader;
    }

    public static string? BearerToken(this HttpRequest httpRequest)
    {
        ArgumentNullException.ThrowIfNull(httpRequest, nameof(httpRequest));

        var authorizationHeader = httpRequest.AuthorizationHeader();

        if (string.IsNullOrWhiteSpace(authorizationHeader) ||
            authorizationHeader.Length < 8)
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
        var authorizationHeader = httpRequest.AuthorizationHeader();

        if (string.IsNullOrWhiteSpace(authorizationHeader) ||
            authorizationHeader.Length < 7)
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

    public static bool HasApiKeyHeader(this HttpRequest httpRequest)
    {
        ArgumentNullException.ThrowIfNull(httpRequest, nameof(httpRequest));

        return !string.IsNullOrWhiteSpace(httpRequest.ApiKeyHeader());
    }

    public static string? ApiKeyHeader(this HttpRequest httpRequest, string? apiKeyHeaderName = null)
    {
        ArgumentNullException.ThrowIfNull(httpRequest, nameof(httpRequest));

        if (string.IsNullOrWhiteSpace(apiKeyHeaderName))
        {
            apiKeyHeaderName = ApiKeyHeaderName;
        }

        if (!httpRequest.Headers.TryGetValue(apiKeyHeaderName, out var apiKeyHeaderValue))
        {
            return null;
        }

        var apiKeyHeader = apiKeyHeaderValue.ToString();

        if (string.IsNullOrWhiteSpace(apiKeyHeader) || (apiKeyHeader.ToUpperInvariant().Trim() == "NULL"))
        {
            return null;
        }

        return apiKeyHeader;
    }

    public static string? ClientSecretHeader(this HttpRequest httpRequest, string? clientSecretHeaderName = null)
    {
        ArgumentNullException.ThrowIfNull(httpRequest, nameof(httpRequest));

        if (string.IsNullOrWhiteSpace(clientSecretHeaderName))
        {
            clientSecretHeaderName = ClientSecretHeaderName;
        }

        if (!httpRequest.Headers.TryGetValue(clientSecretHeaderName, out var clientSecretHeaderValue))
        {
            return null;
        }

        var clientSecretHeader = clientSecretHeaderValue.ToString();

        if (string.IsNullOrWhiteSpace(clientSecretHeader) || (clientSecretHeader.ToUpperInvariant().Trim() == "NULL"))
        {
            return null;
        }

        return clientSecretHeader;
    }

    public static (string ApiKey, string? ClientSecret)? ApiKeyAuthentication(this HttpRequest httpRequest, string? apiKeyHeaderName = null, string? clientSecretHeaderName = null)
    {
        var apiKeyHeader = httpRequest.ApiKeyHeader(apiKeyHeaderName);
        var clientSecretHeader = httpRequest.ClientSecretHeader(clientSecretHeaderName);

        if (string.IsNullOrWhiteSpace(apiKeyHeader))
        {
            return null;
        }

        return (apiKeyHeader, clientSecretHeader);
    }
}
