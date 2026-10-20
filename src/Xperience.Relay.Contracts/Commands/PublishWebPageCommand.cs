namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Publishes a web page language variant: a draft is published, and an unpublished page gets a draft made first.
/// Succeeds without changing anything if the page is already published.
/// </summary>
[RelayCommand("publish-web-page", Access = RelayAccess.Write)]
public class PublishWebPageCommand : IRelayCommand
{
    /// <summary>ID of the web page. Required.</summary>
    public int WebPageId { get; set; }

    /// <summary>Defaults to <c>RelayKenticoOptions.DefaultLanguageName</c> when null.</summary>
    public string? LanguageName { get; set; }
}
