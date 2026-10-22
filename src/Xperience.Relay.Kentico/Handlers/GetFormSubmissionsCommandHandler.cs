using CMS.DataEngine;
using CMS.OnlineForms;
using Microsoft.Extensions.Logging;
using Xperience.Relay.Contracts;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Core;
using Xperience.Relay.Kentico.Internal;

namespace Xperience.Relay.Kentico.Handlers;

public class GetFormSubmissionsCommandHandler(
    ILogger<GetFormSubmissionsCommandHandler> logger) : IRelayCommandHandler<GetFormSubmissionsCommand>
{
    private const string DefaultOrderBy = "FormInserted DESC";

    public Task<RelayCommandResult> HandleAsync(GetFormSubmissionsCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.FormClassName))
            return Task.FromResult(RelayCommandResult.Fail("FormClassName is required."));

        if (command.Top is < 0)
            return Task.FromResult(RelayCommandResult.Fail("Top can't be negative."));

        var formClass = DataClassInfoProvider.GetDataClassInfo(command.FormClassName);
        if (formClass is null || !string.Equals(formClass.ClassType, ClassType.FORM, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(RelayCommandResult.Fail(
                $"Form '{command.FormClassName}' was not found. Use the form's class name, shown in the admin under the form's properties (for example BizForm.ContactUs)."));
        }

        var filter = CheckFilter(command);
        if (filter.Error is not null)
            return Task.FromResult(RelayCommandResult.Fail(filter.Error));

        try
        {
            var query = BizFormItemProvider.GetItems(command.FormClassName);

            if (filter.Where is not null)
                query.Where(filter.Where);

            query.OrderBy(filter.OrderBy);

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

    private readonly record struct Filter(string? Where, string OrderBy, string? Error);

    /// <summary>
    /// The condition and the ordering are checked against the columns of the form's table, and what is handed to Kentico is
    /// the SQL generated from what was checked. See <see cref="FormFilterChecker"/>.
    /// </summary>
    private static Filter CheckFilter(GetFormSubmissionsCommand command)
    {
        var hasWhere = !string.IsNullOrWhiteSpace(command.WhereClause);
        var hasOrderBy = !string.IsNullOrWhiteSpace(command.OrderBy);

        if (!hasWhere && !hasOrderBy)
        {
            return new Filter(null, DefaultOrderBy, null);
        }

        var columns = ClassStructureInfo.GetClassInfo(command.FormClassName)?.ColumnNames ?? [];

        // A condition can only be checked against columns that are known, so without them it isn't run.
        if (columns.Count == 0)
        {
            return new Filter(null, DefaultOrderBy,
                $"The columns of form '{command.FormClassName}' couldn't be read, so WhereClause and OrderBy can't be used with it.");
        }

        string? where = null;

        if (hasWhere)
        {
            var checkedWhere = FormFilterChecker.CheckCondition(command.WhereClause!, command.FormClassName, columns);

            if (checkedWhere.Error is not null)
                return new Filter(null, DefaultOrderBy, checkedWhere.Error);

            where = checkedWhere.Sql;
        }

        var orderBy = DefaultOrderBy;

        if (hasOrderBy)
        {
            var checkedOrderBy = FormFilterChecker.CheckOrderBy(command.OrderBy!, command.FormClassName, columns);

            if (checkedOrderBy.Error is not null)
                return new Filter(null, DefaultOrderBy, checkedOrderBy.Error);

            orderBy = checkedOrderBy.Sql!;
        }

        return new Filter(where, orderBy, null);
    }
}
