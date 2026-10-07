using CMS.OnlineForms;
using Microsoft.Extensions.Logging;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;

namespace Xperience.Relay.Kentico.Handlers;

public class GetFormSubmissionsCommandHandler(
    ILogger<GetFormSubmissionsCommandHandler> logger) : IRelayCommandHandler<GetFormSubmissionsCommand>
{
    public Task<RelayCommandResult> HandleAsync(GetFormSubmissionsCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.FormClassName))
            return Task.FromResult(RelayCommandResult.Fail("FormClassName is required."));

        try
        {
            var query = BizFormItemProvider.GetItems(command.FormClassName);

            if (!string.IsNullOrWhiteSpace(command.WhereClause))
                query.Where(command.WhereClause);

            query.OrderBy(string.IsNullOrWhiteSpace(command.OrderBy)
                ? "FormInserted DESC"
                : command.OrderBy);

            if (command.Top.HasValue)
                query.TopN(command.Top.Value);

            var columns = new List<string>();
            var rows = new List<List<string?>>();

            foreach (BizFormItem item in query)
            {
                if (columns.Count == 0)
                    columns.AddRange(item.ColumnNames);

                rows.Add(columns.Select(col =>
                {
                    var val = item.GetValue(col);
                    return val is null or DBNull ? null : val.ToString();
                }).ToList());
            }

            logger.LogInformation("get-form-submissions retrieved {Count} submission(s) for '{FormClassName}'.", rows.Count, command.FormClassName);

            return Task.FromResult(RelayCommandResult.Ok(
                message: $"Retrieved {rows.Count} submission(s) for '{command.FormClassName}'.",
                data: new GetFormSubmissionsResult
                {
                    FormClassName = command.FormClassName,
                    Columns = columns,
                    Rows = rows,
                }));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "get-form-submissions failed for '{FormClassName}'.", command.FormClassName);
            return Task.FromResult(RelayCommandResult.Fail($"Failed to retrieve form submissions: {ex.Message}"));
        }
    }
}
