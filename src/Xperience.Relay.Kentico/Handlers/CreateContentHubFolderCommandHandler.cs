using CMS.ContentEngine;
using Microsoft.Extensions.Options;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;
using Xperience.Relay.Kentico.Internal;

namespace Xperience.Relay.Kentico.Handlers;

/// <summary>
/// Ensures a content hub folder path exists, creating any missing path segments. Idempotent —
/// safe to call even if all segments already exist. Returns the leaf folder's
/// <see cref="CreateContentHubFolderResult.ContentFolderId"/> so the caller can pass it directly
/// to a "create-content-item" command without a separate lookup.
/// </summary>
public class CreateContentHubFolderCommandHandler(
    IContentFolderManagerFactory contentFolderManagerFactory,
    ServiceAccountResolver serviceAccountResolver,
    IOptions<RelayKenticoOptions> options) : IRelayCommandHandler<CreateContentHubFolderCommand>
{
    private readonly RelayKenticoOptions _options = options.Value;

    public async Task<RelayCommandResult> HandleAsync(CreateContentHubFolderCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.FolderPath))
        {
            return RelayCommandResult.Fail("FolderPath must not be empty.");
        }

        var workspaceName = command.WorkspaceName ?? _options.DefaultWorkspaceName;
        var userId = serviceAccountResolver.ResolveUserId();
        var folderManager = contentFolderManagerFactory.Create(userId);

        var current = await folderManager.GetRoot(workspaceName);

        foreach (var segment in command.FolderPath.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var child = await folderManager.Get(segment);
            if (child == null)
            {
                await folderManager.Create(current.ContentFolderID, new CreateContentFolderParameters(displayName: segment, name: segment));
                child = await folderManager.Get(segment);
            }
            current = child;
        }

        return RelayCommandResult.Ok(
            message: $"Folder '{command.FolderPath}' is ready (ID={current.ContentFolderID}).",
            data: new CreateContentHubFolderResult { ContentFolderId = current.ContentFolderID });
    }
}
