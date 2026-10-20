namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Fetches a web page's system fields (content type, tree path, parent ID, ...) without its
/// content-type-specific field data. Returns a <see cref="WebPageInfo"/> in <see cref="RelayCommandResult.Data"/>.
/// </summary>
[RelayCommand("get-page-info", Access = RelayAccess.Read)]
public class GetPageInfoCommand : IRelayCommand
{
    /// <summary>ID of the web page. Required.</summary>
    public int WebPageId { get; set; }

    /// <summary>Defaults to <c>RelayKenticoOptions.DefaultLanguageName</c> when null.</summary>
    public string? LanguageName { get; set; }
}
