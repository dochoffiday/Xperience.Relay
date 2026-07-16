using CMS.DataEngine;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;

namespace Xperience.Relay.Kentico.Handlers;

public class CreateCustomObjectCommandHandler : IRelayCommandHandler<CreateCustomObjectCommand>
{
    public Task<RelayCommandResult> HandleAsync(CreateCustomObjectCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.ObjectTypeName))
        {
            return Task.FromResult(RelayCommandResult.Fail("ObjectTypeName is required."));
        }

        var typeInfo = ObjectTypeManager.GetTypeInfo(command.ObjectTypeName, false);

        if (typeInfo is null)
        {
            return Task.FromResult(RelayCommandResult.Fail($"Object type '{command.ObjectTypeName}' was not found."));
        }

        var info = ModuleManager.GetObject(command.ObjectTypeName, false);

        if (info is null)
        {
            return Task.FromResult(RelayCommandResult.Fail($"Could not instantiate object of type '{command.ObjectTypeName}'."));
        }

        foreach (var (col, val) in command.Fields)
        {
            info.SetValue(col, QueryItemsHelpers.GetScalarValue(val));
        }

        info.Insert();

        var newId = Convert.ToInt32(info.GetValue(typeInfo.IDColumn));

        return Task.FromResult(RelayCommandResult.Ok(
            message: $"Created '{command.ObjectTypeName}' with ID {newId}.",
            data: new CreateCustomObjectResult { Id = newId }));
    }
}
