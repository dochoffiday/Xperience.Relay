namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Deletes a language variant of a web page. If <see cref="Permanently"/> is false and this is
/// not the last language variant the page is moved to the recycle bin; set it to true to bypass
/// the recycle bin entirely. Optionally creates a redirect to another page after deletion.
/// </summary>
[RelayCommand("delete-web-page")]
public class DeleteWebPageCommand : IRelayCommand
{
    public int WebPageId { get; set; }

    /// <summary>Defaults to <c>RelayKenticoOptions.DefaultLanguageName</c> when null.</summary>
    public string? LanguageName { get; set; }

    /// <summary>When set, a redirect is created pointing to this web page after deletion.</summary>
    public int? RedirectToWebPageId { get; set; }

    /// <summary>Bypasses the recycle bin and permanently removes the page. Defaults to false.</summary>
    public bool Permanently { get; set; }
}
