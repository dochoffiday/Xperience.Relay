namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Fetches a web page's system fields plus the field values of its latest version (drafts included).
/// Returns a <see cref="WebPageData"/> in <see cref="RelayCommandResult.Data"/>.
/// </summary>
[RelayCommand("get-page", Access = RelayAccess.Read)]
public class GetPageCommand : IRelayCommand
{
    /// <summary>ID of the web page. Required.</summary>
    public int WebPageId { get; set; }

    /// <summary>Defaults to <c>RelayKenticoOptions.DefaultLanguageName</c> when null.</summary>
    public string? LanguageName { get; set; }
}
