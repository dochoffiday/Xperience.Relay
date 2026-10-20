namespace Xperience.Relay.Contracts.Commands;

[RelayCommand("get-content-item-usage", Access = RelayAccess.Read)]
public class GetContentItemUsageCommand : IRelayCommand
{
    public int ContentItemId { get; set; }
    public string LanguageName { get; set; } = string.Empty;
}
