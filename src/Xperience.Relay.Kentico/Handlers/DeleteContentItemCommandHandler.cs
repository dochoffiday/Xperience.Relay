using CMS.ContentEngine;
using Microsoft.Extensions.Options;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;
using Xperience.Relay.Kentico.Internal;

namespace Xperience.Relay.Kentico.Handlers;

/// <summary>
/// Deletes a language variant of a reusable content item. If this is the last variant the
/// parent content item is also removed.
/// </summary>
public class DeleteContentItemCommandHandler(
    IContentItemManagerFactory contentItemManagerFactory,
    ContentVersionReader versionReader,
    ServiceAccountResolver serviceAccountResolver,
    IOptions<RelayKenticoOptions> options) : IRelayCommandHandler<DeleteContentItemCommand>
{
    private readonly RelayKenticoOptions _options = options.Value;

    public async Task<RelayCommandResult> HandleAsync(DeleteContentItemCommand command, CancellationToken cancellationToken = default)
    {
        // Kentico deletes nothing, and says nothing, for an item or language variant that isn't there.
        var problem = versionReader.CheckReusable(command.ContentItemId, "delete-web-page");
        if (problem is not null)
        {
            return RelayCommandResult.Fail(problem);
        }

        var languageName = command.LanguageName ?? _options.DefaultLanguageName;

        if (versionReader.Read(command.ContentItemId, languageName) is null)
        {
            return DraftEditFlow.NotFound($"content item {command.ContentItemId} ({languageName})");
        }

        var userId = serviceAccountResolver.ResolveUserId();
        var contentItemManager = contentItemManagerFactory.Create(userId);

        await contentItemManager.Delete(new DeleteContentItemParameters(command.ContentItemId, languageName), cancellationToken);

        return RelayCommandResult.Ok($"Deleted content item {command.ContentItemId} ({languageName}).");
    }
}
