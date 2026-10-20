namespace Xperience.Relay.Contracts;

/// <summary>
/// System-level fields describing a reusable content item, without its content-type-specific
/// field data. Returned by "get-content-info" -- cheap enough to use for existence checks.
/// </summary>
public class ContentInfo
{
    /// <summary>ID of the content item.</summary>
    public int ContentItemId { get; set; }

    /// <summary>GUID of the content item.</summary>
    public Guid ContentItemGuid { get; set; }

    /// <summary>The item's code name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Code name of the item's content type, e.g. <c>DancingGoat.Image</c>.</summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>Empty from "get-content-info" (the item's system fields don't vary by language); the requested language from "get-content".</summary>
    public string LanguageName { get; set; } = string.Empty;

    /// <summary>The workspace's numeric ID, as text. Kentico's public API doesn't expose a workspace's name.</summary>
    public string WorkspaceName { get; set; } = string.Empty;

    /// <summary>Reserved. Not filled in yet, so currently always null.</summary>
    public string? ContentFolderPath { get; set; }
}
