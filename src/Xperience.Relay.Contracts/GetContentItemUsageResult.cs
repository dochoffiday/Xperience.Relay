namespace Xperience.Relay.Contracts;

/// <summary>Returned in <see cref="RelayCommandResult.Data"/> after a successful "get-content-item-usage" command.</summary>
public class GetContentItemUsageResult
{
    /// <summary>The content items that reference the item, one entry per language variant.</summary>
    public List<ContentItemUsageEntry> Items { get; set; } = [];
}

/// <summary>A content item language variant that references another content item.</summary>
public class ContentItemUsageEntry
{
    /// <summary>ID of the referencing content item. For a web page this is the page's content item ID, not its web page ID.</summary>
    public int ContentItemId { get; set; }

    /// <summary>Language of the referencing variant, e.g. <c>en</c>.</summary>
    public string LanguageName { get; set; } = string.Empty;

    /// <summary>Display name of the referencing item in that language.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Status of the variant's latest version: <c>InitialDraft</c>, <c>Draft</c>, <c>Published</c> or <c>Unpublished</c>.</summary>
    public string LatestVersionStatus { get; set; } = string.Empty;

    /// <summary>When the variant was created.</summary>
    public DateTime CreatedWhen { get; set; }

    /// <summary>When the variant was last changed.</summary>
    public DateTime ModifiedWhen { get; set; }

    /// <summary>True when the variant has an image asset.</summary>
    public bool HasImageAsset { get; set; }

    /// <summary>When the variant is scheduled to be published, if it is.</summary>
    public DateTime? ScheduledPublishWhen { get; set; }

    /// <summary>When the variant is scheduled to be unpublished, if it is.</summary>
    public DateTime? ScheduledUnpublishWhen { get; set; }
}
