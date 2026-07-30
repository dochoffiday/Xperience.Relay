namespace Xperience.Relay.Contracts.Commands;

[RelayCommand("execute-sql")]
public class ExecuteSqlCommand : IRelayCommand
{
    public string Query { get; set; } = string.Empty;
    public bool Confirm { get; set; }
}
