using CMS.ContentEngine;
using CMS.DataEngine;
using CMS.Websites;
using CMS.Websites.Internal;
using Microsoft.Extensions.Options;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;
using Xperience.Relay.Kentico.Internal;

namespace Xperience.Relay.Kentico.Handlers;

/// <summary>
/// Updates fields on an existing web page. Preserves the page's current published/draft state —
/// if it was published the draft is immediately re-published after the update; if it was a draft
/// the update stays as a draft.
/// </summary>
public class UpdateWebPageCommandHandler(
    IWebPageManagerFactory webPageManagerFactory,
    IInfoProvider<WebPageItemInfo> webPageItemInfoProvider,
    ContentVersionReader versionReader,
    ContentFieldChecker fieldChecker,
    FailedWriteCacheCleaner cacheCleaner,
    ServiceAccountResolver serviceAccountResolver,
    IOptions<RelayKenticoOptions> options) : IRelayCommandHandler<UpdateWebPageCommand>
{
    private readonly RelayKenticoOptions _options = options.Value;

    public async Task<RelayCommandResult> HandleAsync(UpdateWebPageCommand command, CancellationToken cancellationToken = default)
    {
        if ((command.Fields == null || command.Fields.Count == 0)
            && (command.LinkedItemFields == null || command.LinkedItemFields.Count == 0)
            && (command.TagFields == null || command.TagFields.Count == 0)
            && (command.Assets == null || command.Assets.Count == 0))
        {
            return RelayCommandResult.Fail("No fields provided — nothing to update.");
        }

        var languageName = command.LanguageName ?? _options.DefaultLanguageName;

        var webPageItem = webPageItemInfoProvider.Get()
            .WhereEquals(nameof(WebPageItemInfo.WebPageItemID), command.WebPageId)
            .FirstOrDefault();

        if (webPageItem is null)
        {
            return RelayCommandResult.Fail($"Web page {command.WebPageId} was not found.");
        }

        // The page's state in this language decides how to edit it and whether to publish the edit afterwards.
        var subject = $"web page {command.WebPageId} ({languageName})";
        var state = versionReader.Read(webPageItem.WebPageItemContentItemID, languageName);

        if (state is null)
        {
            return DraftEditFlow.NotFound(subject);
        }

        var problem = fieldChecker.CheckItem(
            webPageItem.WebPageItemContentItemID,
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
        var webPageManager = webPageManagerFactory.Create(webPageItem.WebPageItemWebsiteChannelID, userId);

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
                    CreateDraft: () => webPageManager.TryCreateDraft(command.WebPageId, languageName, cancellationToken),
                    SaveDraft: async () => DraftEditFlow.SavedOrError(
                        await webPageManager.TryUpdateDraft(command.WebPageId, languageName, new UpdateDraftData(itemData), cancellationToken),
                        subject),
                    Publish: () => webPageManager.TryPublish(command.WebPageId, languageName, cancellationToken),
                    DiscardDraft: () => webPageManager.TryDiscardDraft(command.WebPageId, languageName, cancellationToken),
                    ForgetCachedCopies: () => cacheCleaner.Forget(webPageItem.WebPageItemContentItemID, languageName)),
                $"Updated {fieldData.Count} field(s) on web page {command.WebPageId}.");
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
