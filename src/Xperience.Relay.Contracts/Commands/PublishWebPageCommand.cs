namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Publishes a web page language variant. No-ops if the page is already published.
/// </summary>
[RelayCommand("publish-web-page")]
public class PublishWebPageCommand : IRelayCommand
{
    public int WebPageId { get; set; }

    /// <summary>Defaults to <c>RelayKenticoOptions.DefaultLanguageName</c> when null.</summary>
    public string? LanguageName { get; set; }
}
