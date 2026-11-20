namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Lists the page templates that a page can use, which are the identifiers that go in the <c>template</c> of
/// <see cref="PageBuilderData"/>. Name an existing page with <see cref="WebPageId"/>, or the content type and parent of a
/// page that isn't created yet with <see cref="ContentTypeName"/>. The templates are registered by the site's code, so a
/// site that doesn't use Page Builder has none. Returns a <see cref="GetPageTemplatesResult"/> in
/// <see cref="RelayCommandResult.Data"/>.
/// </summary>
[RelayCommand("get-page-templates", Access = RelayAccess.Read)]
public class GetPageTemplatesCommand : IRelayCommand
{
    /// <summary>ID of an existing web page. Give this or <see cref="ContentTypeName"/>.</summary>
    public int? WebPageId { get; set; }

    /// <summary>
    /// Code name of the content type of a page that isn't created yet, e.g. <c>DancingGoat.ArticlePage</c>. Give this or
    /// <see cref="WebPageId"/>.
    /// </summary>
    public string? ContentTypeName { get; set; }

    /// <summary>
    /// ID of the parent of the page that isn't created yet, or 0 (the default) for the root of the channel. A site can limit
    /// the templates by where the page goes. Used with <see cref="ContentTypeName"/>.
    /// </summary>
    public int ParentWebPageItemId { get; set; }

    /// <summary>Defaults to <c>RelayKenticoOptions.DefaultLanguageName</c> when null.</summary>
    public string? LanguageName { get; set; }
}
