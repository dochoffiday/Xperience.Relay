namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Queries web pages of one or more content types, returning only the requested columns.
/// Results land in <see cref="QueryItemsResult"/> in <see cref="RelayCommandResult.Data"/>.
/// </summary>
[RelayCommand("query-web-page-items")]
public class QueryWebPageItemsCommand : QueryItemsCommandBase
{
    /// <summary>
    /// Required. Defaults to <c>RelayKenticoOptions.DefaultWebsiteChannelName</c> when null.
    /// </summary>
    public string? WebsiteChannelName { get; set; }
}
