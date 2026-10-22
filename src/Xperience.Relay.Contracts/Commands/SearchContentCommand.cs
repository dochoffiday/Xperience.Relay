namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Searches the published items of one content type for text, case-insensitively, across every text, long text and
/// rich text field. The search runs in the application after the items are fetched, so avoid it on very large content
/// types. Returns a <see cref="SearchContentResult"/> in <see cref="RelayCommandResult.Data"/>.
/// </summary>
[RelayCommand("search-content", Access = RelayAccess.Read)]
public class SearchContentCommand : IRelayCommand
{
    /// <summary>Code name of the content type to search, e.g. <c>DancingGoat.ArticlePage</c>. Required.</summary>
    public string ContentTypeName { get; set; } = string.Empty;

    /// <summary>The text to look for, up to 1000 characters. Required.</summary>
    public string Filter { get; set; } = string.Empty;

    /// <summary>Defaults to <c>RelayKenticoOptions.DefaultLanguageName</c> when null.</summary>
    public string? LanguageName { get; set; }

    /// <summary>
    /// Website channel to search, for web page content types. Defaults to
    /// <c>RelayKenticoOptions.DefaultWebsiteChannelName</c> when null. Not used for reusable content types.
    /// </summary>
    public string? WebsiteChannelName { get; set; }
}
