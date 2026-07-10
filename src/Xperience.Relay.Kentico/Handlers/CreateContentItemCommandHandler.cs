using CMS.ContentEngine;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;
using Xperience.Relay.Kentico.Internal;

namespace Xperience.Relay.Kentico.Handlers;

/// <summary>
/// Creates a new reusable content item, optionally writing a Base64-encoded binary into an asset
/// field. The file is decoded to a temp path on disk, wrapped in Kentico asset metadata, and the
/// temp file is deleted in a finally block regardless of outcome. After creation the item is
/// published and, if <see cref="CreateContentItemCommand.FolderPath"/> is set, moved to that
/// content hub folder (creating any missing segments along the way).
/// </summary>
public class CreateContentItemCommandHandler(
    IContentItemManagerFactory contentItemManagerFactory,
    IContentFolderManagerFactory contentFolderManagerFactory,
    ServiceAccountResolver serviceAccountResolver,
    IOptions<RelayKenticoOptions> options) : IRelayCommandHandler<CreateContentItemCommand>
{
    private readonly RelayKenticoOptions _options = options.Value;

    public async Task<RelayCommandResult> HandleAsync(CreateContentItemCommand command, CancellationToken cancellationToken = default)
    {
        var userId = serviceAccountResolver.ResolveUserId();
        var languageName = command.LanguageName ?? _options.DefaultLanguageName;
        var workspaceName = command.WorkspaceName ?? _options.DefaultWorkspaceName;

        if (string.IsNullOrWhiteSpace(workspaceName))
        {
            return RelayCommandResult.Fail("WorkspaceName is required. Set it on the command or configure RelayKenticoOptions.DefaultWorkspaceName.");
        }

        var contentItemManager = contentItemManagerFactory.Create(userId);

        var fieldData = new Dictionary<string, object?>();

        if (command.Fields != null)
        {
            foreach (var (key, value) in command.Fields)
            {
                fieldData[key] = DeserializeJsonElement(value);
            }
        }

        if (command.LinkedItemFields != null)
        {
            foreach (var (key, guids) in command.LinkedItemFields)
            {
                fieldData[key] = guids
                    .Select(g => new ContentItemReference { Identifier = g })
                    .ToList();
            }
        }

        if (command.TagFields != null)
        {
            foreach (var (key, guids) in command.TagFields)
            {
                fieldData[key] = new TagReferences
                {
                    Tags = guids.Select(g => new TagReference { Identifier = g }).ToList()
                };
            }
        }

        string? tempFile = null;
        try
        {
            if (command.Asset is { } asset && asset.IsValid())
            {
                var bytes = Convert.FromBase64String(asset.Base64);
                var ext = Path.GetExtension(asset.FileName);
                tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ext);
                await File.WriteAllBytesAsync(tempFile, bytes, cancellationToken);

                var file = CMS.IO.FileInfo.New(tempFile);
                var assetMetadata = new ContentItemAssetMetadata
                {
                    Extension = file.Extension,
                    Identifier = Guid.NewGuid(),
                    LastModified = DateTime.UtcNow,
                    Name = asset.FileName,
                    Size = file.Length
                };

                fieldData[asset.FieldName] = new ContentItemAssetMetadataWithSource(
                    new ContentItemAssetFileSource(file.FullName, false),
                    assetMetadata);
            }

            var createParams = new CreateContentItemParameters(
                command.ContentTypeName,
                null,
                command.DisplayName?.TrimLength(100),
                languageName,
                workspaceName);

            var contentItemId = await contentItemManager.Create(createParams, new ContentItemData(fieldData));
            var metadata = await contentItemManager.GetContentItemMetadata(contentItemId);

            if (!await contentItemManager.TryPublish(contentItemId, languageName))
            {
                return RelayCommandResult.Fail($"Content item '{command.DisplayName}' was created (ID={contentItemId}) but could not be published.");
            }

            if (command.ContentFolderId.HasValue)
            {
                var folderManager = contentFolderManagerFactory.Create(userId);
                await folderManager.MoveItems(command.ContentFolderId.Value, [contentItemId]);
            }

            return RelayCommandResult.Ok(
                message: $"Created and published '{command.DisplayName}' (ID={contentItemId}).",
                data: new CreateContentItemResult
                {
                    ContentItemGuid = metadata.ContentItemGUID,
                    ContentItemId = contentItemId
                });
        }
        finally
        {
            if (tempFile != null && File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    private static object? DeserializeJsonElement(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.String => element.GetString(),
        JsonValueKind.Number when element.TryGetInt32(out var i) => i,
        JsonValueKind.Number when element.TryGetInt64(out var l) => l,
        JsonValueKind.Number => element.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Null => null,
        _ => element.ToString()
    };
}
