using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Xperience.Relay.Core;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the relay dispatcher and verb registry, scanning <paramref name="commandAssemblies"/>
    /// for <c>[RelayCommand]</c>-decorated types. Call <c>AddScoped&lt;IRelayCommandHandler&lt;T&gt;, ...&gt;</c>
    /// separately for each command's handler (e.g. from Xperience.Relay.Kentico).
    /// </summary>
    public static IServiceCollection AddRelayCore(this IServiceCollection services, params Assembly[] commandAssemblies)
    {
        services.AddSingleton(new RelayVerbRegistry(commandAssemblies));
        services.AddScoped<IRelayDispatcher, RelayDispatcher>();
        return services;
    }

    public static IServiceCollection AddRelayPipelineBehavior<TBehavior>(this IServiceCollection services)
        where TBehavior : class, IRelayPipelineBehavior
    {
        services.AddScoped<IRelayPipelineBehavior, TBehavior>();
        return services;
    }
}
