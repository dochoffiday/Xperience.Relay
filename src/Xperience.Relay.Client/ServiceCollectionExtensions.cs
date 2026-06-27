using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Xperience.Relay.Client;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="RelayClient"/> as a typed <see cref="HttpClient"/> via
    /// <see cref="IHttpClientFactory"/>, configured from <paramref name="configure"/>.
    /// </summary>
    public static IServiceCollection AddRelayClient(this IServiceCollection services, Action<RelayClientOptions> configure)
    {
        services.Configure(configure);
        services.AddHttpClient<RelayClient>((provider, httpClient) =>
        {
            var options = provider.GetRequiredService<IOptions<RelayClientOptions>>().Value;
            httpClient.BaseAddress = options.BaseAddress;
            httpClient.DefaultRequestHeaders.Add(options.ApiKeyHeaderName, options.ApiKey);
        });

        return services;
    }
}
