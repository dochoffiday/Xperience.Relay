namespace Xperience.Relay.Contracts.Commands;

[RelayCommand("get-content-item-usage")]
public class GetContentItemUsageCommand : IRelayCommand
{
    public int ContentItemId { get; set; }
    public string LanguageName { get; set; } = string.Empty;
}
