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
    /// update-content-item, update-slug, get-content-hub-folder, query-web-page-items, query-reusable-items, query-sql,
    /// delete-content-item, delete-web-page, create-web-page, publish-web-page,
    /// unpublish-web-page, publish-content-item, unpublish-content-item). Call
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
        services.AddScoped<IRelayCommandHandler<QueryWebPageItemsCommand>, QueryWebPageItemsCommandHandler>();
        services.AddScoped<IRelayCommandHandler<QueryReusableItemsCommand>, QueryReusableItemsCommandHandler>();
        services.AddScoped<IRelayCommandHandler<QuerySqlCommand>, QuerySqlCommandHandler>();
        services.AddScoped<IRelayCommandHandler<DeleteContentItemCommand>, DeleteContentItemCommandHandler>();
        services.AddScoped<IRelayCommandHandler<DeleteWebPageCommand>, DeleteWebPageCommandHandler>();
        services.AddScoped<IRelayCommandHandler<CreateWebPageCommand>, CreateWebPageCommandHandler>();
        services.AddScoped<IRelayCommandHandler<PublishWebPageCommand>, PublishWebPageCommandHandler>();
        services.AddScoped<IRelayCommandHandler<UnpublishWebPageCommand>, UnpublishWebPageCommandHandler>();
        services.AddScoped<IRelayCommandHandler<PublishContentItemCommand>, PublishContentItemCommandHandler>();
        services.AddScoped<IRelayCommandHandler<UnpublishContentItemCommand>, UnpublishContentItemCommandHandler>();
        services.AddScoped<IRelayCommandHandler<UpdateSlugCommand>, UpdateSlugCommandHandler>();
        services.AddScoped<IRelayCommandHandler<ReoptimizeAssetCommand>, ReoptimizeAssetCommandHandler>();
        services.AddScoped<IRelayCommandHandler<RenameAssetCommand>, RenameAssetCommandHandler>();
        services.AddScoped<IRelayCommandHandler<GetTaxonomiesCommand>, GetTaxonomiesCommandHandler>();
        services.AddScoped<IRelayCommandHandler<CreateTaxonomyCommand>, CreateTaxonomyCommandHandler>();
        services.AddScoped<IRelayCommandHandler<UpdateTaxonomyCommand>, UpdateTaxonomyCommandHandler>();
        services.AddScoped<IRelayCommandHandler<DeleteTaxonomyCommand>, DeleteTaxonomyCommandHandler>();
        services.AddScoped<IRelayCommandHandler<GetTagsCommand>, GetTagsCommandHandler>();
        services.AddScoped<IRelayCommandHandler<CreateTagCommand>, CreateTagCommandHandler>();
        services.AddScoped<IRelayCommandHandler<UpdateTagCommand>, UpdateTagCommandHandler>();
        services.AddScoped<IRelayCommandHandler<MoveTagCommand>, MoveTagCommandHandler>();
        services.AddScoped<IRelayCommandHandler<DeleteTagCommand>, DeleteTagCommandHandler>();
        services.AddScoped<IRelayCommandHandler<SearchContentCommand>, SearchContentCommandHandler>();

        return services;
    }
}
