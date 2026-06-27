namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Fetches a web page's system fields (content type, tree path, parent ID, ...) without its
/// content-type-specific field data. Result payload is a <see cref="WebPageInfo"/>.
/// </summary>
[RelayCommand("get-page-info")]
public class GetPageInfoCommand : IRelayCommand
{
    public int WebPageId { get; set; }

    /// <summary>Defaults to the handler's configured default language when null.</summary>
    public string? LanguageName { get; set; }
}
