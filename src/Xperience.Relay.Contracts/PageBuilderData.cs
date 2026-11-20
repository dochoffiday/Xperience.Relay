using System.Text.Json;

namespace Xperience.Relay.Contracts;

/// <summary>
/// The Page Builder data of a web page: the widgets on it and the page template it uses, as the JSON Kentico stores
/// them. Used in "create-web-page" and "update-web-page" to set the data, and in "get-page" to read it.
/// </summary>
public class PageBuilderData
{
    /// <summary>
    /// The widgets of the page, as Kentico stores them: an object with an <c>editableAreas</c> list. Each area holds
    /// sections, each section holds widget zones, and each zone holds widgets with their variants and properties. Read a
    /// page with <c>includePageBuilder</c> in "get-page" to see one. Left out, the widgets stay as they are; what is sent
    /// replaces all of them.
    /// </summary>
    public JsonElement? Widgets { get; set; }

    /// <summary>
    /// The page template and its properties, as Kentico stores them: an object with the template's <c>identifier</c>, its
    /// <c>properties</c> and its <c>fieldIdentifiers</c>, the last two <c>null</c> when there is nothing to set. Only the
    /// identifier is required. "get-page-templates" lists the identifiers a page can use. Left out, the template stays as
    /// it is.
    /// </summary>
    public JsonElement? Template { get; set; }
}
