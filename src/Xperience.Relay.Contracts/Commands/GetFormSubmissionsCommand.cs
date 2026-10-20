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

    /// <summary>Maximum number of submissions to return. All of them when omitted.</summary>
    public int? Top { get; set; }

    /// <summary>
    /// A SQL WHERE expression, without the <c>WHERE</c> keyword, over the form's table columns. Passed to Kentico as
    /// given.
    /// </summary>
    public string? WhereClause { get; set; }

    /// <summary>A SQL ORDER BY expression, without the keywords. Defaults to <c>FormInserted DESC</c>.</summary>
    public string? OrderBy { get; set; }
}
