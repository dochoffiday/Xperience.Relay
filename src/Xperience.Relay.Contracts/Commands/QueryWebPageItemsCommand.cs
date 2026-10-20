namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Queries web pages of one or more content types, returning only the requested columns.
/// Results land in <see cref="QueryItemsResult"/> in <see cref="RelayCommandResult.Data"/>.
/// </summary>
[RelayCommand("query-web-page-items", Access = RelayAccess.Read)]
public class QueryWebPageItemsCommand : QueryItemsCommandBase
{
    /// <summary>
    /// Code name of the website channel to query. Defaults to <c>RelayKenticoOptions.DefaultWebsiteChannelName</c> when
    /// null; the command fails if neither is set.
    /// </summary>
    public string? WebsiteChannelName { get; set; }

    /// <summary>
    /// When true, each item also gets <c>WebPageRelativeUrl</c> and <c>WebPageAbsoluteUrl</c>
    /// (empty strings for pages with no URL path). Defaults to false.
    /// </summary>
    public bool IncludeUrls { get; set; }

    /// <summary>
    /// When true, each item also gets <c>WebPageStatus</c> -- the status of its latest version: InitialDraft, Draft,
    /// Published or Unpublished -- and <c>WebPageIsPublished</c>: whether a published version exists. That is true for
    /// Published and for Draft, which is a change on top of a published version (a page that was never published is an
    /// InitialDraft), so a live page with unpublished changes has status Draft and is still published. The query still
    /// returns every page, drafts included. Defaults to false.
    /// </summary>
    public bool IncludeStatus { get; set; }
}
