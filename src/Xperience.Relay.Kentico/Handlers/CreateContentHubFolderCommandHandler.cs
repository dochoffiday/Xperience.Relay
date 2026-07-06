using CMS.ContentEngine;
using CMS.DataEngine;
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
    IInfoProvider<ContentFolderInfo> contentFolderInfoProvider,
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

        if (string.IsNullOrWhiteSpace(workspaceName))
        {
            return RelayCommandResult.Fail("WorkspaceName is required. Set it on the command or configure RelayKenticoOptions.DefaultWorkspaceName.");
        }

        var userId = serviceAccountResolver.ResolveUserId();
        var folderManager = contentFolderManagerFactory.Create(userId);

        var current = await folderManager.GetRoot(workspaceName);
        var workingPath = "";

        foreach (var segment in command.FolderPath.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            workingPath += "/" + segment;

            // Scope the lookup to children of the current folder by display name.
            // folderManager.Get(name) does a global code-name lookup and would silently
            // match a same-named folder elsewhere in the tree.
            var child = contentFolderInfoProvider.Get()
                .WhereEquals(nameof(ContentFolderInfo.ContentFolderParentFolderID), current.ContentFolderID)
                .WhereEquals(nameof(ContentFolderInfo.ContentFolderDisplayName), segment)
                .FirstOrDefault();

            if (child == null)
            {
                var codeName = GetCodeName(workingPath);

                await folderManager.Create(current.ContentFolderID, new CreateContentFolderParameters(displayName: segment, name: codeName));

                child = contentFolderInfoProvider.Get()
                    .WhereEquals(nameof(ContentFolderInfo.ContentFolderParentFolderID), current.ContentFolderID)
                    .WhereEquals(nameof(ContentFolderInfo.ContentFolderDisplayName), segment)
                    .FirstOrDefault();

                if (child == null)
                {
                    return RelayCommandResult.Fail($"Folder segment '{segment}' was created but could not be retrieved.");
                }
            }

            current = child;
        }

        return RelayCommandResult.Ok(
            message: $"Folder '{command.FolderPath}' is ready (ID={current.ContentFolderID}).",
            data: new CreateContentHubFolderResult { ContentFolderId = current.ContentFolderID });
    }

    private static string GetCodeName(string folderPath)
    {
        // Use the full path to generate a code name to reduce the chance of collisions.
        // Replace slashes with underscores and remove any invalid characters.

        folderPath = folderPath
            .TrimStart('/')
            .Replace('/', '_');

        return Strings.ToCodeName(folderPath);
    }
}
