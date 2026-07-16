using CMS.ContentEngine.Internal;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;

namespace Xperience.Relay.Kentico.Handlers;

public class DeleteTagCommandHandler(
    ITaxonomyManager taxonomyManager) : IRelayCommandHandler<DeleteTagCommand>
{
    public async Task<RelayCommandResult> HandleAsync(DeleteTagCommand command, CancellationToken cancellationToken = default)
    {
        await taxonomyManager.DeleteTag(command.TagId, cancellationToken);

        return RelayCommandResult.Ok($"Deleted tag {command.TagId}.");
    }
}
