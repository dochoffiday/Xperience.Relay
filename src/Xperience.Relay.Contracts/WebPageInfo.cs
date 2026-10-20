namespace Xperience.Relay.Contracts;

/// <summary>
/// System-level fields describing a web page, without its content-type-specific field data.
/// Returned by "get-page-info" -- cheap enough to use for existence checks or path resolution
/// (e.g. before sending a "move-web-page" command).
/// </summary>
public class WebPageInfo
{
    /// <summary>ID of the web page.</summary>
    public int WebPageId { get; set; }

    /// <summary>GUID of the web page.</summary>
    public Guid WebPageGuid { get; set; }

    /// <summary>The page's code name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Code name of the page's content type, e.g. <c>DancingGoat.ArticlePage</c>.</summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>The page's position in the content tree, e.g. <c>/Articles/Coffee_Beverages_Explained</c>. Not the URL.</summary>
    public string TreePath { get; set; } = string.Empty;

    /// <summary>ID of the parent page, or null for a page at the root of the channel.</summary>
    public int? ParentWebPageId { get; set; }

    /// <summary>The language that was asked for (the page isn't checked to have a variant in it).</summary>
    public string LanguageName { get; set; } = string.Empty;

    /// <summary>Code name of the website channel the page belongs to, e.g. <c>DancingGoatPages</c>.</summary>
    public string ChannelName { get; set; } = string.Empty;
}
