using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
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
    IInfoProvider<ContentItemCommonDataInfo> contentItemCommonDataInfoProvider,
    ServiceAccountResolver serviceAccountResolver,
    IOptions<RelayKenticoOptions> options) : IRelayCommandHandler<UpdateWebPageCommand>
{
    private readonly RelayKenticoOptions _options = options.Value;

    public async Task<RelayCommandResult> HandleAsync(UpdateWebPageCommand command, CancellationToken cancellationToken = default)
    {
        if ((command.Fields == null || command.Fields.Count == 0)
            && (command.LinkedItemFields == null || command.LinkedItemFields.Count == 0)
            && (command.TagFields == null || command.TagFields.Count == 0))
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

        // Check whether the latest version is published so we can restore state after the update.
        var commonData = contentItemCommonDataInfoProvider.Get()
            .WhereEquals(nameof(ContentItemCommonDataInfo.ContentItemCommonDataContentItemID), webPageItem.WebPageItemContentItemID)
            .WhereTrue(nameof(ContentItemCommonDataInfo.ContentItemCommonDataIsLatest))
            .FirstOrDefault();

        var isPublished = commonData is not null
            && commonData.ContentItemCommonDataVersionStatus == VersionStatus.Published;

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
        var itemData = new ContentItemData(fieldData);

        await webPageManager.TryCreateDraft(command.WebPageId, languageName);
        await webPageManager.TryUpdateDraft(command.WebPageId, languageName, new UpdateDraftData(itemData));

        if (isPublished)
        {
            await webPageManager.TryPublish(command.WebPageId, languageName);
        }

        return RelayCommandResult.Ok($"Updated {fieldData.Count} field(s) on web page {command.WebPageId}.");
    }

}
