namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Fetches a web page's system fields plus the field values of its latest version (drafts included), and optionally its
/// Page Builder data. Returns a <see cref="WebPageData"/> in <see cref="RelayCommandResult.Data"/>.
/// </summary>
[RelayCommand("get-page", Access = RelayAccess.Read)]
public class GetPageCommand : IRelayCommand
{
    /// <summary>ID of the web page. Required.</summary>
    public int WebPageId { get; set; }

    /// <summary>Defaults to <c>RelayKenticoOptions.DefaultLanguageName</c> when null.</summary>
    public string? LanguageName { get; set; }

    /// <summary>
    /// When true, the result also holds the page's widgets and page template. Defaults to false, since the widgets of a
    /// page can be large.
    /// </summary>
    public bool IncludePageBuilder { get; set; }
}
