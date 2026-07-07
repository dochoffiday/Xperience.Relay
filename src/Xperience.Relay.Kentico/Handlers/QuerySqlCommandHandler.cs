using System.Data;
using System.Text.RegularExpressions;
using CMS.DataEngine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;

namespace Xperience.Relay.Kentico.Handlers;

/// <summary>
/// Executes a read-only SQL query and returns columns + rows. Guards are application-level;
/// configuring a read-only DB login at the database level is strongly recommended as the
/// primary control.
/// </summary>
public class QuerySqlCommandHandler(
    ILogger<QuerySqlCommandHandler> logger,
    IOptions<RelayKenticoOptions> options) : IRelayCommandHandler<QuerySqlCommand>
{
    private readonly RelayKenticoOptions _options = options.Value;

    // Blunt app-side guard — not the primary control, just a backstop.
    private static readonly Regex ForbiddenPattern = new(
        @"\b(INSERT|UPDATE|DELETE|RENAME|DROP|ALTER|EXEC|EXECUTE|TRUNCATE|MERGE|GRANT|REVOKE|CREATE|COMMIT|ROLLBACK|CALL|LOCK|SAVEPOINT|TRANSACTION|SET)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public Task<RelayCommandResult> HandleAsync(QuerySqlCommand command, CancellationToken cancellationToken = default)
    {
        var query = command.Query?.Trim().TrimEnd(';');

        if (string.IsNullOrWhiteSpace(query))
        {
            return Task.FromResult(RelayCommandResult.Fail("Query must not be empty."));
        }

        if (!query.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) &&
            !query.StartsWith("WITH", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(RelayCommandResult.Fail("Only SELECT or WITH...SELECT statements are allowed."));
        }

        if (ForbiddenPattern.IsMatch(query))
        {
            return Task.FromResult(RelayCommandResult.Fail(
                "Query contains a disallowed keyword (INSERT/UPDATE/DELETE/RENAME/DROP/ALTER/EXEC/TRUNCATE/MERGE/GRANT/REVOKE/CREATE/COMMIT/ROLLBACK/CALL/LOCK/SAVEPOINT/TRANSACTION/SET)."));
        }

        logger.LogInformation("query-sql executing: {Query}", query);

        try
        {
            DataSet dataSet;
            using (var scope = new CMSConnectionScope { CommandTimeout = _options.SqlQueryTimeoutSeconds })
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
