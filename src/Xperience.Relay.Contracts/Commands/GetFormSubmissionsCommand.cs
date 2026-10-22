namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Reads the submissions of a Kentico form (BizForm). Rows come back as strings, newest first by default. Returns a
/// <see cref="GetFormSubmissionsResult"/> in <see cref="RelayCommandResult.Data"/>.
/// </summary>
[RelayCommand("get-form-submissions", Access = RelayAccess.Read)]
public class GetFormSubmissionsCommand : IRelayCommand
{
    /// <summary>
    /// Class name of the form, as shown in the admin under the form's properties, e.g.
    /// <c>BizForm.DancingGoatContactUs</c>. Required.
    /// </summary>
    public string FormClassName { get; set; } = string.Empty;

    /// <summary>Maximum number of submissions to return. All of them when omitted. Can't be negative.</summary>
    public int? Top { get; set; }

    /// <summary>
    /// A condition over the form's table columns, without the <c>WHERE</c> keyword: comparisons of columns with values
    /// (<c>=</c>, <c>LIKE</c>, <c>IN</c>, <c>BETWEEN</c>, <c>IS NULL</c> and the like) combined with <c>AND</c>,
    /// <c>OR</c>, <c>NOT</c> and parentheses, at most 2000 characters. Function calls, subqueries and comments are refused.
    /// </summary>
    public string? WhereClause { get; set; }

    /// <summary>
    /// The form's table columns to sort by, separated by commas, each optionally followed by <c>ASC</c> or <c>DESC</c>,
    /// without the <c>ORDER BY</c> keywords. Defaults to <c>FormInserted DESC</c>.
    /// </summary>
    public string? OrderBy { get; set; }
}
