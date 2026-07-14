namespace Xperience.Relay.Contracts.Commands;

[RelayCommand("update-slug")]
public class UpdateSlugCommand : IRelayCommand
{
    public int WebPageId { get; set; }
    public string? LanguageName { get; set; }
    public string Slug { get; set; } = string.Empty;
}
