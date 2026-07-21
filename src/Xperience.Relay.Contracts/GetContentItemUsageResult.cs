namespace Xperience.Relay.Contracts;

public class GetContentItemUsageResult
{
    public List<ContentItemUsageEntry> Items { get; set; } = [];
}

public class ContentItemUsageEntry
{
    public int ContentItemId { get; set; }
    public string LanguageName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string LatestVersionStatus { get; set; } = string.Empty;
    public DateTime CreatedWhen { get; set; }
    public DateTime ModifiedWhen { get; set; }
    public bool HasImageAsset { get; set; }
    public DateTime? ScheduledPublishWhen { get; set; }
    public DateTime? ScheduledUnpublishWhen { get; set; }
}
