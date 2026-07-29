using CMS.ContactManagement;
using CMS.DataEngine;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;

namespace Xperience.Relay.Kentico.Handlers;

public class DeleteContactsCommandHandler(
    IContactsBulkDeletionService contactsDeletionService) : IRelayCommandHandler<DeleteContactsCommand>
{
    public async Task<RelayCommandResult> HandleAsync(DeleteContactsCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.WhereClause))
        {
            return RelayCommandResult.Fail("WhereClause is required.");
        }

        var condition = new WhereCondition().Where(command.WhereClause);

        await contactsDeletionService.BulkDelete(condition, command.BatchSize ?? 0, cancellationToken);

        var batchNote = command.BatchSize.HasValue
            ? $" in a single batch of {command.BatchSize}"
            : " across all batches";

        return RelayCommandResult.Ok($"Deleted contacts matching condition '{command.WhereClause}'{batchNote}.");
    }
}
