using CMS.ContentEngine;
using Microsoft.Extensions.Options;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;
using Xperience.Relay.Kentico.Internal;

namespace Xperience.Relay.Kentico.Handlers;

/// <summary>
/// Updates fields on an existing reusable content item. Preserves the item's current
/// published/draft state — if it was published the draft is immediately re-published after
/// the update; if it was a draft the update stays as a draft.
/// </summary>
public class UpdateContentItemCommandHandler(
    IContentItemManagerFactory contentItemManagerFactory,
    ContentVersionReader versionReader,
    ContentFieldChecker fieldChecker,
    FailedWriteCacheCleaner cacheCleaner,
    ServiceAccountResolver serviceAccountResolver,
    IOptions<RelayKenticoOptions> options) : IRelayCommandHandler<UpdateContentItemCommand>
{
    private readonly RelayKenticoOptions _options = options.Value;

    public async Task<RelayCommandResult> HandleAsync(UpdateContentItemCommand command, CancellationToken cancellationToken = default)
    {
        if ((command.Fields == null || command.Fields.Count == 0)
            && (command.LinkedItemFields == null || command.LinkedItemFields.Count == 0)
            && (command.TagFields == null || command.TagFields.Count == 0)
            && (command.Assets == null || command.Assets.Count == 0))
        {
            return RelayCommandResult.Fail("No fields provided — nothing to update.");
        }

        var languageName = command.LanguageName ?? _options.DefaultLanguageName;

        var problem = versionReader.CheckReusable(command.ContentItemId, "update-web-page");
        if (problem is not null)
        {
            return RelayCommandResult.Fail(problem);
        }

        // The item's state in this language decides how to edit it and whether to publish the edit afterwards.
        var subject = $"content item {command.ContentItemId} ({languageName})";
        var state = versionReader.Read(command.ContentItemId, languageName);

        if (state is null)
        {
            return DraftEditFlow.NotFound(subject);
        }

        problem = fieldChecker.CheckItem(
            command.ContentItemId,
            new ContentFields(command.Fields, command.LinkedItemFields, command.TagFields, command.Assets));

        if (problem is not null)
        {
            return RelayCommandResult.Fail(problem);
        }

        var fieldData = new Dictionary<string, object?>();

        if (command.Fields != null)
        {
            foreach (var (key, value) in command.Fields)
            {
                fieldData[key] = QueryItemsHelpers.DeserializeJsonElement(value);
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
                fieldData[key] = guids.Select(g => new TagReference { Identifier = g }).ToList();
            }
        }

        var userId = serviceAccountResolver.ResolveUserId();
        var contentItemManager = contentItemManagerFactory.Create(userId);

        var tempFiles = new List<string>();
        try
        {
            if (command.Assets != null)
            {
                foreach (var asset in command.Assets.Where(a => a.IsValid()))
                {
                    var bytes = Convert.FromBase64String(asset.Base64);
                    var ext = Path.GetExtension(asset.FileName);
                    var tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ext);
                    await File.WriteAllBytesAsync(tempFile, bytes, cancellationToken);
                    tempFiles.Add(tempFile);

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
            }

            var itemData = new ContentItemData(fieldData);

            return await DraftEditFlow.EditAsync(
                subject,
                state,
                new DraftOperations(
                    CreateDraft: () => contentItemManager.TryCreateDraft(command.ContentItemId, languageName, cancellationToken),
                    SaveDraft: async () => DraftEditFlow.SavedOrError(
                        await contentItemManager.TryUpdateDraft(command.ContentItemId, languageName, itemData, cancellationToken),
                        subject),
                    Publish: () => contentItemManager.TryPublish(command.ContentItemId, languageName, cancellationToken),
                    DiscardDraft: () => contentItemManager.TryDiscardDraft(command.ContentItemId, languageName, cancellationToken),
                    ForgetCachedCopies: () => cacheCleaner.Forget(command.ContentItemId, languageName)),
                $"Updated {fieldData.Count} field(s) on content item {command.ContentItemId}.");
        }
        finally
        {
            foreach (var tempFile in tempFiles)
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }
    }

}
