using System.Text.Json;
using CMS.ContentEngine;
using CMS.ContentEngine.Internal;
using CMS.DataEngine;
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
    IInfoProvider<ContentItemCommonDataInfo> contentItemCommonDataInfoProvider,
    ServiceAccountResolver serviceAccountResolver,
    IOptions<RelayKenticoOptions> options) : IRelayCommandHandler<UpdateContentItemCommand>
{
    private readonly RelayKenticoOptions _options = options.Value;

    public async Task<RelayCommandResult> HandleAsync(UpdateContentItemCommand command, CancellationToken cancellationToken = default)
    {
        if ((command.Fields == null || command.Fields.Count == 0)
            && (command.LinkedItemFields == null || command.LinkedItemFields.Count == 0)
            && (command.TagFields == null || command.TagFields.Count == 0))
        {
            return RelayCommandResult.Fail("No fields provided — nothing to update.");
        }

        var languageName = command.LanguageName ?? _options.DefaultLanguageName;

        var commonData = contentItemCommonDataInfoProvider.Get()
            .WhereEquals(nameof(ContentItemCommonDataInfo.ContentItemCommonDataContentItemID), command.ContentItemId)
            .WhereTrue(nameof(ContentItemCommonDataInfo.ContentItemCommonDataIsLatest))
            .FirstOrDefault();

        if (commonData is null)
        {
            return RelayCommandResult.Fail($"Content item {command.ContentItemId} was not found.");
        }

        var isPublished = commonData.ContentItemCommonDataVersionStatus == VersionStatus.Published;

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
                    Tags = guids.Select(g => new TagReference { Identifier = g })
                };
            }
        }

        var userId = serviceAccountResolver.ResolveUserId();
        var contentItemManager = contentItemManagerFactory.Create(userId);
        var itemData = new ContentItemData(fieldData);

        await contentItemManager.TryCreateDraft(command.ContentItemId, languageName, cancellationToken);
        await contentItemManager.TryUpdateDraft(command.ContentItemId, languageName, itemData, cancellationToken);

        if (isPublished)
        {
            await contentItemManager.TryPublish(command.ContentItemId, languageName, cancellationToken);
        }

        return RelayCommandResult.Ok($"Updated {fieldData.Count} field(s) on content item {command.ContentItemId}.");
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
