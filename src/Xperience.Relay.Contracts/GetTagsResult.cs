namespace Xperience.Relay.Contracts;

public class GetTagsResult
{
    public List<TagResult> Tags { get; set; } = [];
}

public class TagResult
{
    public int TagId { get; set; }
    public Guid TagGuid { get; set; }
    public string TagName { get; set; } = string.Empty;
    public string TagTitle { get; set; } = string.Empty;
    public string? TagDescription { get; set; }
    public int TaxonomyId { get; set; }
    public int? ParentTagId { get; set; }
    public int TagOrder { get; set; }
}
