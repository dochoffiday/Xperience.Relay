using System.Text.Json;
using CMS.ContentEngine;
using CMS.Websites;
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
    IContentQueryExecutor contentQueryExecutor,
    ServiceAccountResolver serviceAccountResolver,
    IOptions<RelayKenticoOptions> options) : IRelayCommandHandler<UpdateWebPageCommand>
{
    private readonly RelayKenticoOptions _options = options.Value;

    public async Task<RelayCommandResult> HandleAsync(UpdateWebPageCommand command, CancellationToken cancellationToken = default)
    {
        if ((command.Fields == null || command.Fields.Count == 0)
            && (command.LinkedItemFields == null || command.LinkedItemFields.Count == 0))
        {
            return RelayCommandResult.Fail("No fields provided — nothing to update.");
        }

        var languageName = command.LanguageName ?? _options.DefaultLanguageName;

        var builder = new ContentItemQueryBuilder()
            .ForContentTypes(_ => { })
            .Parameters(p => p.Where(w => w.WhereEquals("WebPageItemID", command.WebPageId)));

        var pages = await contentQueryExecutor.GetWebPageResult(
            builder,
            container =>
            {
                container.TryGetValue<int>("ContentItemCommonDataVersionStatus", out var versionStatusInt);
                return (
                    ChannelId: container.WebPageItemWebsiteChannelID,
                    IsPublished: (VersionStatus)versionStatusInt == VersionStatus.Published
                );
            },
            cancellationToken: cancellationToken);

        var page = pages.FirstOrDefault();
        if (page == default)
        {
            return RelayCommandResult.Fail($"Web page {command.WebPageId} was not found.");
        }

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

        var userId = serviceAccountResolver.ResolveUserId();
        var webPageManager = webPageManagerFactory.Create(page.ChannelId, userId);
        var itemData = new ContentItemData(fieldData);

        await webPageManager.TryCreateDraft(command.WebPageId, languageName);
        await webPageManager.TryUpdateDraft(command.WebPageId, languageName, new UpdateDraftData(itemData));

        if (page.IsPublished)
        {
            await webPageManager.TryPublish(command.WebPageId, languageName);
        }

        return RelayCommandResult.Ok($"Updated {fieldData.Count} field(s) on web page {command.WebPageId}.");
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
