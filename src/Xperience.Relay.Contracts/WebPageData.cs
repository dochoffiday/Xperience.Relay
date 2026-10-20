namespace Xperience.Relay.Contracts;

/// <summary>
/// A web page's system fields plus its content-type-specific field data. Returned by "get-page".
/// <see cref="Fields"/> is untyped since its shape depends on the page's content type.
/// </summary>
public class WebPageData : WebPageInfo
{
    /// <summary>
    /// Field values of the page's latest version, by field name, as JSON (text, numbers, dates, ...). Asset, tag and
    /// reference fields come as the JSON text Kentico stores in them. Only the content type's own fields are listed:
    /// fields that come from reusable field schemas are not included.
    /// </summary>
    public Dictionary<string, object?> Fields { get; set; } = new();
}
