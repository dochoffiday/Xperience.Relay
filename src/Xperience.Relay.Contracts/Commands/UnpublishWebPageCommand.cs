namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Unpublishes a web page language variant. No-ops if the page is already unpublished.
/// </summary>
[RelayCommand("unpublish-web-page")]
public class UnpublishWebPageCommand : IRelayCommand
{
    public int WebPageId { get; set; }

    /// <summary>Defaults to <c>RelayKenticoOptions.DefaultLanguageName</c> when null.</summary>
    public string? LanguageName { get; set; }
}
