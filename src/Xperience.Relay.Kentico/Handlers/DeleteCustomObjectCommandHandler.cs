using CMS.DataEngine;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;

namespace Xperience.Relay.Kentico.Handlers;

public class DeleteCustomObjectCommandHandler : IRelayCommandHandler<DeleteCustomObjectCommand>
{
    public Task<RelayCommandResult> HandleAsync(DeleteCustomObjectCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.ObjectTypeName))
        {
            return Task.FromResult(RelayCommandResult.Fail("ObjectTypeName is required."));
        }

        if (command.Id <= 0)
        {
            return Task.FromResult(RelayCommandResult.Fail("Id must be a positive integer."));
        }

        var typeInfo = ObjectTypeManager.GetTypeInfo(command.ObjectTypeName, false);

        if (typeInfo is null)
        {
            return Task.FromResult(RelayCommandResult.Fail($"Object type '{command.ObjectTypeName}' was not found."));
        }

        var info = new ObjectQuery(command.ObjectTypeName, false).WithID(command.Id).FirstOrDefault();

        if (info is null)
        {
            return Task.FromResult(RelayCommandResult.Fail($"'{command.ObjectTypeName}' with ID {command.Id} was not found."));
        }

        info.Delete();

        return Task.FromResult(RelayCommandResult.Ok($"Deleted '{command.ObjectTypeName}' with ID {command.Id}."));
    }
}
