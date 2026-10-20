using CMS.ContentEngine;
using Microsoft.Extensions.Options;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;
using Xperience.Relay.Kentico.Internal;

namespace Xperience.Relay.Kentico.Handlers;

public class PublishContentItemCommandHandler(
    IContentItemManagerFactory contentItemManagerFactory,
    ContentVersionReader versionReader,
    ServiceAccountResolver serviceAccountResolver,
    IOptions<RelayKenticoOptions> options) : IRelayCommandHandler<PublishContentItemCommand>
{
    private readonly RelayKenticoOptions _options = options.Value;

    public async Task<RelayCommandResult> HandleAsync(PublishContentItemCommand command, CancellationToken cancellationToken = default)
    {
        var problem = versionReader.CheckReusable(command.ContentItemId, "publish-web-page");
        if (problem is not null)
        {
            return RelayCommandResult.Fail(problem);
        }

        var languageName = command.LanguageName ?? _options.DefaultLanguageName;
        var userId = serviceAccountResolver.ResolveUserId();
        var contentItemManager = contentItemManagerFactory.Create(userId);

        return await PublishingFlow.PublishAsync(
            $"content item {command.ContentItemId} ({languageName})",
            versionReader.Read(command.ContentItemId, languageName),
            () => contentItemManager.TryCreateDraft(command.ContentItemId, languageName, cancellationToken),
            () => contentItemManager.TryPublish(command.ContentItemId, languageName, cancellationToken));
    }
}
