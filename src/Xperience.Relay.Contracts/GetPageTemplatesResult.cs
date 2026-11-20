namespace Xperience.Relay.Contracts;

/// <summary>Returned in <see cref="RelayCommandResult.Data"/> after a successful "get-page-templates" command.</summary>
public class GetPageTemplatesResult
{
    /// <summary>The templates the page can use, by name.</summary>
    public List<PageTemplateResult> Templates { get; set; } = [];

    /// <summary>
    /// Identifier of the template the page uses now, when an existing page was asked about and it has one. It is listed
    /// among <see cref="Templates"/> unless the site no longer offers it.
    /// </summary>
    public string? CurrentTemplate { get; set; }
}

/// <summary>A page template, as listed by "get-page-templates".</summary>
public class PageTemplateResult
{
    /// <summary>The template's identifier, for the <c>template</c> of <see cref="PageBuilderData"/>.</summary>
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    /// Name of the template, localized the way the administration shows it: a name the site registers as a localization
    /// key comes back as its text.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Description of the template, localized the same way; null when it has none.</summary>
    public string? Description { get; set; }

    /// <summary>CSS class of the icon the administration shows for the template; null when it has none.</summary>
    public string? IconClass { get; set; }

    /// <summary>
    /// Code names of the content types the template is registered for. Empty when it is registered for all page content
    /// types.
    /// </summary>
    public List<string> ContentTypeNames { get; set; } = [];
}
