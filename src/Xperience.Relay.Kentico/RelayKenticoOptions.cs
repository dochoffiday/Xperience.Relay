namespace Xperience.Relay.Kentico;

/// <summary>
/// Configuration for relay handlers that talk to the Kentico Xperience API. Register via
/// <c>services.Configure&lt;RelayKenticoOptions&gt;(...)</c> in the host application.
/// </summary>
public class RelayKenticoOptions
{
    /// <summary>
    /// User name of the Kentico user attributed to relay-driven changes (drafts, moves, ...) for
    /// auditing purposes. The user's actual Kentico permissions are not checked by the relay.
    /// </summary>
    public string ServiceAccountUserName { get; set; } = string.Empty;

    /// <summary>Language used by commands that don't specify one explicitly.</summary>
    public string DefaultLanguageName { get; set; } = "en";

    /// <summary>Workspace used by commands that don't specify one explicitly.</summary>
    public string DefaultWorkspaceName { get; set; } = string.Empty;

    /// <summary>
    /// Code name of the website channel used by <c>create-web-page</c>, <c>query-web-page-items</c> and
    /// <c>search-content</c> (for web page content types) when the command doesn't name one. Leave empty to
    /// make those commands require an explicit <c>WebsiteChannelName</c>.
    /// </summary>
    public string DefaultWebsiteChannelName { get; set; } = string.Empty;

    /// <summary>
    /// Command timeout in seconds for <c>query-sql</c> execution. Defaults to 30 seconds.
    /// </summary>
    public int SqlQueryTimeoutSeconds { get; set; } = 30;
}
