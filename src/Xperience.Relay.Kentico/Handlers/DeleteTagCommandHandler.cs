using CMS.ContentEngine;
using CMS.DataEngine;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;

namespace Xperience.Relay.Kentico.Handlers;

public class DeleteTagCommandHandler(
    IInfoProvider<TagInfo> tagInfoProvider,
    ITaxonomyManager taxonomyManager) : IRelayCommandHandler<DeleteTagCommand>
{
    public async Task<RelayCommandResult> HandleAsync(DeleteTagCommand command, CancellationToken cancellationToken = default)
    {
        // Kentico deletes nothing, and says nothing, for a tag that isn't there.
        if (await tagInfoProvider.GetAsync(command.TagId, cancellationToken) is null)
        {
            return RelayCommandResult.Fail($"Tag {command.TagId} was not found.");
        }

        await taxonomyManager.DeleteTag(command.TagId, cancellationToken);

        return RelayCommandResult.Ok($"Deleted tag {command.TagId}.");
    }
}
