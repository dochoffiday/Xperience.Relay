using CMS.ContentEngine.Internal;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;

namespace Xperience.Relay.Kentico.Handlers;

public class MoveTagCommandHandler(
    ITaxonomyManager taxonomyManager) : IRelayCommandHandler<MoveTagCommand>
{
    public async Task<RelayCommandResult> HandleAsync(MoveTagCommand command, CancellationToken cancellationToken = default)
    {
        await taxonomyManager.MoveTag(command.TagId, command.TargetParentTagId, command.Order, cancellationToken);

        return RelayCommandResult.Ok($"Moved tag {command.TagId} to parent {command.TargetParentTagId} at order {command.Order}.");
    }
}
