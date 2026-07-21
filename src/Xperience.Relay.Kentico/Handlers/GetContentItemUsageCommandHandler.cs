using CMS.ContentEngine;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;

namespace Xperience.Relay.Kentico.Handlers;

public class GetContentItemUsageCommandHandler(IContentItemUsageRetriever usageRetriever) : IRelayCommandHandler<GetContentItemUsageCommand>
{
    public async Task<RelayCommandResult> HandleAsync(GetContentItemUsageCommand command, CancellationToken cancellationToken = default)
    {
        if (command.ContentItemId <= 0)
        {
            return RelayCommandResult.Fail("ContentItemId must be a positive integer.");
        }

        var items = await usageRetriever.Retrieve(command.ContentItemId, command.LanguageName, cancellationToken);

        var entries = items.Select(m => new ContentItemUsageEntry
        {
            ContentItemId = m.ContentItemID,
            LanguageName = m.LanguageName,
            DisplayName = m.DisplayName,
            LatestVersionStatus = m.LatestVersionStatus.ToString(),
            CreatedWhen = m.CreatedWhen,
            ModifiedWhen = m.ModifiedWhen,
            HasImageAsset = m.HasImageAsset,
            ScheduledPublishWhen = m.ScheduledPublishWhen,
            ScheduledUnpublishWhen = m.ScheduledUnpublishWhen,
        }).ToList();

        return RelayCommandResult.Ok(
            message: $"Found {entries.Count} usage(s) for content item {command.ContentItemId} in language '{command.LanguageName}'.",
            data: new GetContentItemUsageResult { Items = entries });
    }
}
