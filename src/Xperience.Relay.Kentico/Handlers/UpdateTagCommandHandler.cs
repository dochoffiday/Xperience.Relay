using CMS.ContentEngine;
using CMS.DataEngine;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;

namespace Xperience.Relay.Kentico.Handlers;

public class UpdateTagCommandHandler(
    IInfoProvider<TagInfo> tagInfoProvider) : IRelayCommandHandler<UpdateTagCommand>
{
    public async Task<RelayCommandResult> HandleAsync(UpdateTagCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Title is null && command.Description is null)
        {
            return RelayCommandResult.Fail("At least one of Title or Description must be provided.");
        }

        var tag = await tagInfoProvider.GetAsync(command.TagId, cancellationToken);

        if (tag is null)
        {
            return RelayCommandResult.Fail($"Tag {command.TagId} was not found.");
        }

        if (command.Title is not null)
        {
            tag.TagTitle = command.Title;
        }

        if (command.Description is not null)
        {
            tag.TagDescription = command.Description;
        }

        await tagInfoProvider.SetAsync(tag, cancellationToken);

        return RelayCommandResult.Ok($"Updated tag {command.TagId}.");
    }
}
