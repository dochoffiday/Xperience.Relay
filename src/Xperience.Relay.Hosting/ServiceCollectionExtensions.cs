using Microsoft.Extensions.DependencyInjection;

namespace Xperience.Relay.Hosting;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the relay's API key filter. Call <c>services.Configure&lt;RelayHostingOptions&gt;(...)</c>
    /// separately to set <see cref="RelayHostingOptions.ApiKey"/>, and <c>endpoints.MapRelayEndpoints()</c>
    /// in the host app's routing setup to expose the endpoints.
    /// </summary>
    public static IServiceCollection AddRelayHosting(this IServiceCollection services)
    {
        services.AddScoped<RelayApiKeyEndpointFilter>();
        return services;
    }
}
