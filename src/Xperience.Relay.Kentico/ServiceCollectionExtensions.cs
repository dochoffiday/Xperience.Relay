using Microsoft.Extensions.DependencyInjection;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;
using Xperience.Relay.Kentico.Handlers;
using Xperience.Relay.Kentico.Internal;

namespace Xperience.Relay.Kentico;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers handlers for the Kentico-backed relay commands (move-web-page, move-content-item,
    /// get-page-info, get-page, get-content-info, get-content, create-content-item, update-web-page,
    /// update-content-item, get-content-hub-folder, query-content-items). Call
    /// <c>services.Configure&lt;RelayKenticoOptions&gt;(...)</c> separately to set
    /// <see cref="RelayKenticoOptions.ServiceAccountUserName"/> and related options.
    /// </summary>
    public static IServiceCollection AddRelayKentico(this IServiceCollection services)
    {
        services.AddScoped<ServiceAccountResolver>();

        services.AddScoped<IRelayCommandHandler<MoveWebPageCommand>, MoveWebPageCommandHandler>();
        services.AddScoped<IRelayCommandHandler<MoveContentItemCommand>, MoveContentItemCommandHandler>();
        services.AddScoped<GetPageInfoCommandHandler>();
        services.AddScoped<IRelayCommandHandler<GetPageInfoCommand>>(sp => sp.GetRequiredService<GetPageInfoCommandHandler>());
        services.AddScoped<IRelayCommandHandler<GetPageCommand>, GetPageCommandHandler>();
        services.AddScoped<GetContentInfoCommandHandler>();
        services.AddScoped<IRelayCommandHandler<GetContentInfoCommand>>(sp => sp.GetRequiredService<GetContentInfoCommandHandler>());
        services.AddScoped<IRelayCommandHandler<GetContentCommand>, GetContentCommandHandler>();
        services.AddScoped<IRelayCommandHandler<CreateContentItemCommand>, CreateContentItemCommandHandler>();
        services.AddScoped<IRelayCommandHandler<UpdateWebPageCommand>, UpdateWebPageCommandHandler>();
        services.AddScoped<IRelayCommandHandler<UpdateContentItemCommand>, UpdateContentItemCommandHandler>();
        services.AddScoped<IRelayCommandHandler<GetContentHubFolderCommand>, GetContentHubFolderCommandHandler>();
        services.AddScoped<IRelayCommandHandler<QueryContentItemsCommand>, QueryContentItemsCommandHandler>();

        return services;
    }
}
