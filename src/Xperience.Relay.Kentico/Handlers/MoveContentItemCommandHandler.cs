using CMS.ContentEngine;
using Microsoft.Extensions.Options;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;
using Xperience.Relay.Kentico.Internal;

namespace Xperience.Relay.Kentico.Handlers;

/// <summary>
/// Moves one or more reusable content items into a content hub folder.
/// </summary>
public class MoveContentItemCommandHandler(
    IContentFolderManagerFactory contentFolderManagerFactory,
    ServiceAccountResolver serviceAccountResolver) : IRelayCommandHandler<MoveContentItemCommand>
{
    public async Task<RelayCommandResult> HandleAsync(MoveContentItemCommand command, CancellationToken cancellationToken = default)
    {
        if (command.ContentItemIds.Count == 0)
        {
            return RelayCommandResult.Fail("ContentItemIds must not be empty.");
        }

        var userId = serviceAccountResolver.ResolveUserId();
        var folderManager = contentFolderManagerFactory.Create(userId);

        await folderManager.MoveItems(command.ContentFolderId, command.ContentItemIds.ToArray());

        return RelayCommandResult.Ok($"Moved {command.ContentItemIds.Count} content item(s) to folder {command.ContentFolderId}.");
    }
}
