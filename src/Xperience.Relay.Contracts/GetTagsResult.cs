namespace Xperience.Relay.Contracts;

/// <summary>Returned in <see cref="RelayCommandResult.Data"/> after a successful "get-tags" command.</summary>
public class GetTagsResult
{
    /// <summary>The tags that matched.</summary>
    public List<TagResult> Tags { get; set; } = [];
}

/// <summary>A tag, as listed by "get-tags".</summary>
public class TagResult
{
    /// <summary>ID of the tag.</summary>
    public int TagId { get; set; }

    /// <summary>GUID of the tag, for putting it in a tag field.</summary>
    public Guid TagGuid { get; set; }

    /// <summary>The tag's code name.</summary>
    public string TagName { get; set; } = string.Empty;

    /// <summary>Display title of the tag.</summary>
    public string TagTitle { get; set; } = string.Empty;

    /// <summary>Description of the tag; empty when it has none.</summary>
    public string? TagDescription { get; set; }

    /// <summary>ID of the taxonomy the tag belongs to.</summary>
    public int TaxonomyId { get; set; }

    /// <summary>ID of the parent tag, or null for a tag at the root of its taxonomy.</summary>
    public int? ParentTagId { get; set; }

    /// <summary>Position among the tag's siblings.</summary>
    public int TagOrder { get; set; }
}
