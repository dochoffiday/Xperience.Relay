namespace Xperience.Relay.Contracts.Commands;

[RelayCommand("search-content")]
public class SearchContentCommand : IRelayCommand
{
    public string ContentTypeName { get; set; } = string.Empty;
    public string Filter { get; set; } = string.Empty;
    public string? LanguageName { get; set; }
    public string? WebsiteChannelName { get; set; }
}
