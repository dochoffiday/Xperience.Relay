namespace Xperience.Relay.Contracts;

/// <summary>
/// A content item's system fields plus its content-type-specific field data. Returned by
/// "get-content". <see cref="Fields"/> is untyped since its shape depends on the content type.
/// </summary>
public class ContentData : ContentInfo
{
    /// <summary>
    /// Field values of the item's latest version, by field name, as JSON (text, numbers, dates, ...). Asset, tag and
    /// reference fields come as the JSON text Kentico stores in them. Only the content type's own fields are listed:
    /// fields that come from reusable field schemas are not included.
    /// </summary>
    public Dictionary<string, object?> Fields { get; set; } = new();
}
