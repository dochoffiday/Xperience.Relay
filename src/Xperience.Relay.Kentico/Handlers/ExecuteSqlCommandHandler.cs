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
/// Two-step SQL executor that allows INSERT/UPDATE/DELETE/CREATE/ALTER/DROP/EXECUTE.
/// Without <c>Confirm: true</c> the handler validates the SQL and returns a statement
/// summary so the caller can review what would run before committing. With
/// <c>Confirm: true</c> the query is executed and the result set (if any) is returned.
/// </summary>
public class ExecuteSqlCommandHandler(
    ILogger<ExecuteSqlCommandHandler> logger,
    IOptions<RelayKenticoOptions> options) : IRelayCommandHandler<ExecuteSqlCommand>
{
    private readonly RelayKenticoOptions _options = options.Value;

    public Task<RelayCommandResult> HandleAsync(ExecuteSqlCommand command, CancellationToken cancellationToken = default)
    {
        var query = command.Query?.Trim().TrimEnd(';');

        if (string.IsNullOrWhiteSpace(query))
        {
            return Task.FromResult(RelayCommandResult.Fail("Query must not be empty."));
        }

        var analysis = SqlStatementAnalyzer.Analyze(query, readOnly: false);

        if (!analysis.IsValid)
        {
            return Task.FromResult(RelayCommandResult.Fail(analysis.ErrorMessage!));
        }

        if (!command.Confirm)
        {
            return Task.FromResult(RelayCommandResult.Ok(
                message: "Preview only — set Confirm: true to execute.",
                data: new ExecuteSqlResult
                {
                    Confirmed = false,
                    StatementSummaries = analysis.StatementSummaries,
                }));
        }

        logger.LogInformation("execute-sql executing: {Query}", query);

        try
        {
            DataSet dataSet;
            using (new CMSConnectionScope { CommandTimeout = _options.SqlQueryTimeoutSeconds })
            {
                dataSet = ConnectionHelper.ExecuteQuery(query, null, QueryTypeEnum.SQLQuery);
            }

            var result = new ExecuteSqlResult
            {
                Confirmed = true,
                StatementSummaries = analysis.StatementSummaries,
            };

            string message;

            if (dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                var table = dataSet.Tables[0];
                result.Columns = table.Columns.Cast<DataColumn>().Select(c => c.ColumnName).ToList();
                result.Rows = table.Rows.Cast<DataRow>()
                    .Select(r => r.ItemArray.Select(v => v == DBNull.Value ? null : v?.ToString()).ToList())
                    .ToList();
                message = $"Query returned {result.Rows.Count} row(s).";
                logger.LogInformation("execute-sql returned {RowCount} row(s).", result.Rows.Count);
            }
            else
            {
                message = "Command executed successfully.";
                logger.LogInformation("execute-sql completed with no result set.");
            }

            return Task.FromResult(RelayCommandResult.Ok(message: message, data: result));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "execute-sql failed: {Query}", query);
            return Task.FromResult(RelayCommandResult.Fail($"Query failed: {ex.Message}"));
        }
    }
}
