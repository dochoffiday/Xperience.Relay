namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Changes the URL slug of a web page. The page's published/draft state is preserved: a published page is published
/// again so the new URL goes live, and a draft stays a draft.
/// </summary>
[RelayCommand("update-slug", Access = RelayAccess.Write)]
public class UpdateSlugCommand : IRelayCommand
{
    /// <summary>ID of the web page. Required.</summary>
    public int WebPageId { get; set; }

    /// <summary>Defaults to <c>RelayKenticoOptions.DefaultLanguageName</c> when null.</summary>
    public string? LanguageName { get; set; }

    /// <summary>The new slug (the last part of the URL path). Required.</summary>
    public string Slug { get; set; } = string.Empty;
}
