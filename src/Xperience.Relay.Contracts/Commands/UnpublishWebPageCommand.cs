namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Unpublishes a web page language variant. Succeeds without changing anything if the page is already unpublished or
/// has only a draft. Fails if the page is published and also has a draft with unpublished changes, because Kentico
/// won't unpublish it until that draft is published or discarded.
/// </summary>
[RelayCommand("unpublish-web-page", Access = RelayAccess.Write)]
public class UnpublishWebPageCommand : IRelayCommand
{
    /// <summary>ID of the web page. Required.</summary>
    public int WebPageId { get; set; }

    /// <summary>Defaults to <c>RelayKenticoOptions.DefaultLanguageName</c> when null.</summary>
    public string? LanguageName { get; set; }
}
