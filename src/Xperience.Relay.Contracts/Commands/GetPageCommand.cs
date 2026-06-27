namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Fetches a web page's system fields plus its content-type-specific field data.
/// Result payload is a <see cref="WebPageData"/>.
/// </summary>
[RelayCommand("get-page")]
public class GetPageCommand : IRelayCommand
{
    public int WebPageId { get; set; }

    /// <summary>Defaults to the handler's configured default language when null.</summary>
    public string? LanguageName { get; set; }
}
