namespace Xperience.Relay.Contracts.Commands;

[RelayCommand("get-form-submissions")]
public class GetFormSubmissionsCommand : IRelayCommand
{
    public string FormClassName { get; set; } = string.Empty;
    public int? Top { get; set; }
    public string? WhereClause { get; set; }
    public string? OrderBy { get; set; }
}
