namespace Xperience.Relay.Contracts;

/// <summary>
/// A content item's system fields plus its content-type-specific field data. Returned by
/// "get-content". <see cref="Fields"/> is untyped since its shape depends on the content type.
/// </summary>
public class ContentData : ContentInfo
{
    public Dictionary<string, object?> Fields { get; set; } = new();
}
