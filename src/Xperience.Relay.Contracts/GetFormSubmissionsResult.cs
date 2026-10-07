namespace Xperience.Relay.Contracts;

public class GetFormSubmissionsResult
{
    public string FormClassName { get; set; } = string.Empty;
    public List<string> Columns { get; set; } = [];
    public List<List<string?>> Rows { get; set; } = [];
}
