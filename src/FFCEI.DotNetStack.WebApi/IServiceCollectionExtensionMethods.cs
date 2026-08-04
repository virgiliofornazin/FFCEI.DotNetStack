using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace FFCEI.DotNetStack.WebApi;

public static class IServiceCollectionExtensionMethods
{
    public static IServiceCollection AddForwardHeader(this IServiceCollection services,
        IEnumerable<IPAddress>? knownProxies = null,
        IEnumerable<Microsoft.AspNetCore.HttpOverrides.IPNetwork>? knownNetworks = null,
        Action<ForwardedHeadersOptions>? builder = null)
        => services.Configure<ForwardedHeadersOptions>(options =>
             {
                 options.ForwardedHeaders = ForwardedHeaders.All;

                 if (knownProxies != null && knownProxies.Any())
                 {
                     foreach (var knownProxy in knownProxies)
                     {
                         options.KnownProxies.Add(knownProxy);
                     }
                 }
                 else
                 {
                     options.KnownProxies.Clear();
                 }

                 if (knownNetworks != null && knownNetworks.Any())
                 {
                     foreach (var knownNetwork in knownNetworks)
                     {
                         options.KnownNetworks.Add(knownNetwork);
                     }
                 }
                 else
                 {
                     options.KnownNetworks.Clear();
                 }

                 builder?.Invoke(options);

                 if (options.KnownProxies.Count == 0 && options.KnownNetworks.Count == 0)
                 {
                     options.ForwardLimit = null;
                 }
             });
}
