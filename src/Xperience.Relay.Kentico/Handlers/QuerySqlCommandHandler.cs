using System.Data;
using CMS.DataEngine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;
using Xperience.Relay.Kentico.Internal;

namespace Xperience.Relay.Kentico.Handlers;

/// <summary>
/// Executes a read-only SQL query (SELECT/DECLARE) and returns columns + rows.
/// Validation uses the ScriptDom AST parser — regex bypasses via string literals
/// or comments are not possible. Guards are still application-level; a read-only
/// DB login at the database level is strongly recommended as the primary control.
/// </summary>
public class QuerySqlCommandHandler(
    ILogger<QuerySqlCommandHandler> logger,
    IOptions<RelayKenticoOptions> options) : IRelayCommandHandler<QuerySqlCommand>
{
    private readonly RelayKenticoOptions _options = options.Value;

    public Task<RelayCommandResult> HandleAsync(QuerySqlCommand command, CancellationToken cancellationToken = default)
    {
        var query = command.Query?.Trim().TrimEnd(';');

        if (string.IsNullOrWhiteSpace(query))
        {
            return Task.FromResult(RelayCommandResult.Fail("Query must not be empty."));
        }

        var analysis = SqlStatementAnalyzer.Analyze(query, readOnly: true);

        if (!analysis.IsValid)
        {
            return Task.FromResult(RelayCommandResult.Fail(analysis.ErrorMessage!));
        }

        logger.LogInformation("query-sql executing: {Query}", query);

        try
        {
            DataSet dataSet;
            using (new CMSConnectionScope { CommandTimeout = _options.SqlQueryTimeoutSeconds })
            {
                dataSet = ConnectionHelper.ExecuteQuery(query, null, QueryTypeEnum.SQLQuery);
            }

            var table = dataSet.Tables[0];
            var result = new QuerySqlResult
            {
                Columns = table.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList(),
                Rows = table.Rows.Cast<DataRow>()
                    .Select(r => r.ItemArray.Select(v => v == DBNull.Value ? null : v?.ToString()).ToList())
                    .ToList(),
            };

            logger.LogInformation("query-sql returned {RowCount} row(s).", result.Rows.Count);

            return Task.FromResult(RelayCommandResult.Ok(
                message: $"Query returned {result.Rows.Count} row(s).",
                data: result));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "query-sql failed: {Query}", query);
            return Task.FromResult(RelayCommandResult.Fail($"Query failed: {ex.Message}"));
        }
    }
}
