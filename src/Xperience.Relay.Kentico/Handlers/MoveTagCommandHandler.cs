using CMS.ContentEngine;
using CMS.DataEngine;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;

namespace Xperience.Relay.Kentico.Handlers;

/// <summary>
/// Moves a tag under another tag of the same taxonomy, or to the root of its taxonomy. Kentico doesn't check the move, so
/// the handler does: it would otherwise put a tag under a parent from another taxonomy, or under itself.
/// </summary>
public class MoveTagCommandHandler(
    IInfoProvider<TagInfo> tagInfoProvider,
    ITaxonomyManager taxonomyManager) : IRelayCommandHandler<MoveTagCommand>
{
    // Far more levels than a taxonomy has; stops the walk up if the data already loops.
    private const int MaxDepth = 100;

    public async Task<RelayCommandResult> HandleAsync(MoveTagCommand command, CancellationToken cancellationToken = default)
    {
        var tag = await tagInfoProvider.GetAsync(command.TagId, cancellationToken);

        if (tag is null)
        {
            return RelayCommandResult.Fail($"Tag {command.TagId} was not found.");
        }

        if (command.TargetParentTagId != 0)
        {
            var problem = await CheckParentAsync(tag, command.TargetParentTagId, cancellationToken);

            if (problem is not null)
            {
                return RelayCommandResult.Fail(problem);
            }
        }

        await taxonomyManager.MoveTag(command.TagId, command.TargetParentTagId, command.Order, cancellationToken);

        return RelayCommandResult.Ok($"Moved tag {command.TagId} to parent {command.TargetParentTagId} at order {command.Order}.");
    }

    private async Task<string?> CheckParentAsync(TagInfo tag, int parentId, CancellationToken cancellationToken)
    {
        var parent = await tagInfoProvider.GetAsync(parentId, cancellationToken);

        if (parent is null)
        {
            return $"Tag {parentId} (the new parent) was not found.";
        }

        if (parent.TagTaxonomyID != tag.TagTaxonomyID)
        {
            return $"Tag {parentId} belongs to a different taxonomy, so tag {tag.TagID} can't be moved under it.";
        }

        // The new parent can't be the tag itself, or anything below it.
        var depth = 0;
        for (var ancestor = parent; ancestor is not null && depth++ < MaxDepth; ancestor = await ParentOfAsync(ancestor, cancellationToken))
        {
            if (ancestor.TagID == tag.TagID)
            {
                return "A tag can't be moved under itself or under one of its own child tags.";
            }
        }

        return null;
    }

    private async Task<TagInfo?> ParentOfAsync(TagInfo tag, CancellationToken cancellationToken) =>
        tag.TagParentID > 0 ? await tagInfoProvider.GetAsync(tag.TagParentID, cancellationToken) : null;
}
