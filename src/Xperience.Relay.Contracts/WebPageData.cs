namespace Xperience.Relay.Contracts;

/// <summary>
/// A web page's system fields plus its content-type-specific field data. Returned by "get-page".
/// <see cref="Fields"/> is untyped since its shape depends on the page's content type.
/// </summary>
public class WebPageData : WebPageInfo
{
    public Dictionary<string, object?> Fields { get; set; } = new();
}
