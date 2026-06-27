namespace Xperience.Relay.Contracts;

/// <summary>
/// System-level fields describing a web page, without its content-type-specific field data.
/// Returned by "get-page-info" -- cheap enough to use for existence checks or path resolution
/// (e.g. before sending a "move" command).
/// </summary>
public class WebPageInfo
{
    public int WebPageId { get; set; }
    public Guid WebPageGuid { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string TreePath { get; set; } = string.Empty;
    public int? ParentWebPageId { get; set; }
    public string LanguageName { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
}
