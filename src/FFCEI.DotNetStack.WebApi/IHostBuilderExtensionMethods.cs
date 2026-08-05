using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;

namespace FFCEI.DotNetStack.WebApi;

public static class IHostBuilderExtensionMethods
{
    public static IHostBuilder AddForwardedHeaders(this IHostBuilder hostBuilder,
        IEnumerable<IPAddress>? knownProxies = null,
        IEnumerable<System.Net.IPNetwork>? knownNetworks = null,
        Action<ForwardedHeadersOptions>? builder = null)
    {
        ArgumentNullException.ThrowIfNull(hostBuilder, nameof(hostBuilder));

        _ = hostBuilder.ConfigureServices((hostContext, services) =>
        {
            _ = services.Configure<ForwardedHeadersOptions>(options =>
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
                         options.KnownIPNetworks.Add(knownNetwork);
                     }
                 }
                 else
                 {
                     options.KnownIPNetworks.Clear();
                 }

                 builder?.Invoke(options);

                 if (options.KnownProxies.Count == 0 && options.KnownIPNetworks.Count == 0)
                 {
                     options.ForwardLimit = null;
                 }
             });
        });

        _ = hostBuilder.ConfigureWebHost(webBuilder =>
        {
            _ = webBuilder.Configure((hostContext, app) =>
            {
                _ = app.UseForwardedHeaders();
            });
        });

        return hostBuilder;
    }
}
