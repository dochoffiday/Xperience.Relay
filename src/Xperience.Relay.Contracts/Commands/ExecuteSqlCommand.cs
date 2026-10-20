namespace Xperience.Relay.Contracts.Commands;

[RelayCommand("execute-sql", Access = RelayAccess.Admin)]
public class ExecuteSqlCommand : IRelayCommand
{
    public string Query { get; set; } = string.Empty;
    public bool Confirm { get; set; }
}
