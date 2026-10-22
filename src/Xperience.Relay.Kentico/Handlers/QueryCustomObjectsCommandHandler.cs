using CMS.DataEngine;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;
using Xperience.Relay.Kentico.Internal;

namespace Xperience.Relay.Kentico.Handlers;

public class QueryCustomObjectsCommandHandler : IRelayCommandHandler<QueryCustomObjectsCommand>
{
    public Task<RelayCommandResult> HandleAsync(QueryCustomObjectsCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.ObjectTypeName))
        {
            return Task.FromResult(RelayCommandResult.Fail("ObjectTypeName is required."));
        }

        if (ProtectedObjects.IsProtectedObjectType(command.ObjectTypeName))
        {
            return Task.FromResult(RelayCommandResult.Fail($"Object type '{command.ObjectTypeName}' is protected."));
        }

        var typeInfo = ObjectTypeManager.GetTypeInfo(command.ObjectTypeName, false);

        if (typeInfo is null)
        {
            return Task.FromResult(RelayCommandResult.Fail($"Object type '{command.ObjectTypeName}' was not found."));
        }

        // Kentico takes a column as an expression, so each one has to be plainly a name, and one the type has.
        var problem = QueryInputChecker.CheckQuery([], command.Columns ?? [], command.WhereEquals)
            ?? ObjectColumnChecker.Check(
                command.ObjectTypeName,
                typeInfo.ClassStructureInfo?.ColumnNames ?? [],
                (command.Columns ?? []).Concat(command.WhereEquals?.Keys ?? Enumerable.Empty<string>()));

        if (problem is not null)
        {
            return Task.FromResult(RelayCommandResult.Fail(problem));
        }

        var query = new ObjectQuery(command.ObjectTypeName, false);

        if (command.Columns?.Count > 0)
        {
            query.Columns(command.Columns.ToArray());
        }

        if (command.WhereEquals?.Count > 0)
        {
            foreach (var (col, val) in command.WhereEquals)
            {
                query.WhereEquals(col, QueryItemsHelpers.GetScalarValue(val));
            }
        }

        var items = new List<Dictionary<string, string?>>();

        foreach (BaseInfo info in query)
        {
            var cols = command.Columns?.Count > 0
                ? (IEnumerable<string>)command.Columns
                : info.ColumnNames;

            var dict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            foreach (var col in cols)
            {
                var val = info.GetValue(col);
                dict[col] = val is null or DBNull ? null : val.ToString();
            }

            items.Add(dict);
        }

        return Task.FromResult(RelayCommandResult.Ok(
            message: $"Found {items.Count} object(s) of type '{command.ObjectTypeName}'.",
            data: new QueryCustomObjectsResult { Items = items }));
    }
}
