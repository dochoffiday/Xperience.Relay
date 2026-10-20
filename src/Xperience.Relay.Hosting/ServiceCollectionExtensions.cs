using Microsoft.Extensions.DependencyInjection;

namespace Xperience.Relay.Hosting;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the relay's API key filter. It needs an <see cref="Xperience.Relay.Core.IRelayApiKeyValidator"/>
    /// in the service collection (<c>AddRelayKentico()</c> provides one), and <c>endpoints.MapRelayEndpoints()</c>
    /// in the host app's routing setup exposes the endpoints.
    /// </summary>
    public static IServiceCollection AddRelayHosting(this IServiceCollection services)
    {
        services.AddScoped<RelayApiKeyEndpointFilter>();
        return services;
    }
}
