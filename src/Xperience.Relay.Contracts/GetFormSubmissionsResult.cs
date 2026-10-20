namespace Xperience.Relay.Contracts;

/// <summary>Returned in <see cref="RelayCommandResult.Data"/> after a successful "get-form-submissions" command.</summary>
public class GetFormSubmissionsResult
{
    /// <summary>The class name of the form, as it was given.</summary>
    public string FormClassName { get; set; } = string.Empty;

    /// <summary>Column names, taken from the first submission, so they are empty when there are no submissions.</summary>
    public List<string> Columns { get; set; } = [];

    /// <summary>One list of values per submission, in <see cref="Columns"/> order. Every value is text; null for SQL NULL.</summary>
    public List<List<string?>> Rows { get; set; } = [];
}
