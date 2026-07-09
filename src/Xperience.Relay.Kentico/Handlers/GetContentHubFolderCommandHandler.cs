using CMS.ContentEngine;
using CMS.DataEngine;
using Microsoft.Extensions.Options;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;
using Xperience.Relay.Kentico.Internal;

namespace Xperience.Relay.Kentico.Handlers;

/// <summary>
/// Looks up a content hub folder by ID, code name, or slash-separated path. When using
/// <c>FolderPath</c> the command is idempotent — it creates any missing path segments along the
/// way and returns the leaf folder's ID. The ID and code-name modes are read-only lookups.
/// </summary>
public class GetContentHubFolderCommandHandler(
    IContentFolderManagerFactory contentFolderManagerFactory,
    IInfoProvider<ContentFolderInfo> contentFolderInfoProvider,
    ServiceAccountResolver serviceAccountResolver,
    IOptions<RelayKenticoOptions> options) : IRelayCommandHandler<GetContentHubFolderCommand>
{
    private readonly RelayKenticoOptions _options = options.Value;

    public async Task<RelayCommandResult> HandleAsync(GetContentHubFolderCommand command, CancellationToken cancellationToken = default)
    {
        // Exactly one lookup mode must be provided.
        int providedCount = (command.ContentFolderId.HasValue ? 1 : 0)
                          + (!string.IsNullOrWhiteSpace(command.CodeName) ? 1 : 0)
                          + (!string.IsNullOrWhiteSpace(command.FolderPath) ? 1 : 0);

        if (providedCount == 0)
        {
            return RelayCommandResult.Fail("One of ContentFolderId, CodeName, or FolderPath must be provided.");
        }

        if (providedCount > 1)
        {
            return RelayCommandResult.Fail("Only one of ContentFolderId, CodeName, or FolderPath may be provided.");
        }

        if (command.ContentFolderId.HasValue)
        {
            var folder = contentFolderInfoProvider.Get()
                .WhereEquals(nameof(ContentFolderInfo.ContentFolderID), command.ContentFolderId.Value)
                .FirstOrDefault();

            return folder is null
                ? RelayCommandResult.Fail($"Content hub folder with ID {command.ContentFolderId.Value} was not found.")
                : RelayCommandResult.Ok(
                    message: $"Found folder '{folder.ContentFolderDisplayName}' (ID={folder.ContentFolderID}).",
                    data: new GetContentHubFolderResult { ContentFolderId = folder.ContentFolderID });
        }

        if (!string.IsNullOrWhiteSpace(command.CodeName))
        {
            var folder = contentFolderInfoProvider.Get()
                .WhereEquals(nameof(ContentFolderInfo.ContentFolderName), command.CodeName)
                .FirstOrDefault();

            return folder is null
                ? RelayCommandResult.Fail($"Content hub folder with code name '{command.CodeName}' was not found.")
                : RelayCommandResult.Ok(
                    message: $"Found folder '{folder.ContentFolderDisplayName}' (ID={folder.ContentFolderID}).",
                    data: new GetContentHubFolderResult { ContentFolderId = folder.ContentFolderID });
        }

        // FolderPath mode — walk and create if needed.
        var workspaceName = command.WorkspaceName ?? _options.DefaultWorkspaceName;

        if (string.IsNullOrWhiteSpace(workspaceName))
        {
            return RelayCommandResult.Fail("WorkspaceName is required when using FolderPath. Set it on the command or configure RelayKenticoOptions.DefaultWorkspaceName.");
        }

        var userId = serviceAccountResolver.ResolveUserId();
        var folderManager = contentFolderManagerFactory.Create(userId);

        var current = await folderManager.GetRoot(workspaceName);
        var workingPath = "";

        foreach (var segment in command.FolderPath!.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
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
            data: new GetContentHubFolderResult { ContentFolderId = current.ContentFolderID });
    }

    private static string GetCodeName(string folderPath)
    {
        folderPath = folderPath
            .TrimStart('/')
            .Replace('/', '_');

        return Strings.ToCodeName(folderPath);
    }
}
