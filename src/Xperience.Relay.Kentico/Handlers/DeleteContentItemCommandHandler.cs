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
    ServiceAccountResolver serviceAccountResolver,
    IOptions<RelayKenticoOptions> options) : IRelayCommandHandler<DeleteContentItemCommand>
{
    private readonly RelayKenticoOptions _options = options.Value;

    public async Task<RelayCommandResult> HandleAsync(DeleteContentItemCommand command, CancellationToken cancellationToken = default)
    {
        var languageName = command.LanguageName ?? _options.DefaultLanguageName;

        var userId = serviceAccountResolver.ResolveUserId();
        var contentItemManager = contentItemManagerFactory.Create(userId);

        await contentItemManager.Delete(new DeleteContentItemParameters(command.ContentItemId, languageName), cancellationToken);

        return RelayCommandResult.Ok($"Deleted content item {command.ContentItemId} ({languageName}).");
    }
}
