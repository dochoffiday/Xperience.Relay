using CMS.ContentEngine;
using Microsoft.Extensions.Options;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;
using Xperience.Relay.Kentico.Internal;

namespace Xperience.Relay.Kentico.Handlers;

public class UnpublishContentItemCommandHandler(
    IContentItemManagerFactory contentItemManagerFactory,
    ContentVersionReader versionReader,
    ServiceAccountResolver serviceAccountResolver,
    IOptions<RelayKenticoOptions> options) : IRelayCommandHandler<UnpublishContentItemCommand>
{
    private readonly RelayKenticoOptions _options = options.Value;

    public async Task<RelayCommandResult> HandleAsync(UnpublishContentItemCommand command, CancellationToken cancellationToken = default)
    {
        var problem = versionReader.CheckReusable(command.ContentItemId, "unpublish-web-page");
        if (problem is not null)
        {
            return RelayCommandResult.Fail(problem);
        }

        var languageName = command.LanguageName ?? _options.DefaultLanguageName;
        var userId = serviceAccountResolver.ResolveUserId();
        var contentItemManager = contentItemManagerFactory.Create(userId);

        return await PublishingFlow.UnpublishAsync(
            $"content item {command.ContentItemId} ({languageName})",
            versionReader.Read(command.ContentItemId, languageName),
            () => contentItemManager.TryUnpublish(command.ContentItemId, languageName, cancellationToken));
    }
}
