namespace Xperience.Relay.Contracts;

/// <summary>
/// System-level fields describing a reusable content item, without its content-type-specific
/// field data. Returned by "get-content-info" -- cheap enough to use for existence checks.
/// </summary>
public class ContentInfo
{
    public int ContentItemId { get; set; }
    public Guid ContentItemGuid { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public string LanguageName { get; set; } = string.Empty;
    public string WorkspaceName { get; set; } = string.Empty;
    public string? ContentFolderPath { get; set; }
}
