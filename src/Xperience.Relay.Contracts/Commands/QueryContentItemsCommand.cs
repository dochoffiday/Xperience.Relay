using System.Text.Json;

namespace Xperience.Relay.Contracts.Commands;

public enum RelayContentKind
{
    ReusableContent,
    WebPage,
}

/// <summary>
/// Queries reusable content items or web pages of a given content type, returning only the
/// requested columns. Use <see cref="ContentKind"/> to switch between the web-page and reusable
/// content item executor paths. Results land in <see cref="QueryContentItemsResult"/> in
/// <see cref="RelayCommandResult.Data"/>.
/// </summary>
[RelayCommand("query-content-items")]
public class QueryContentItemsCommand : IRelayCommand
{
    public string ContentTypeName { get; set; } = string.Empty;

    public RelayContentKind ContentKind { get; set; } = RelayContentKind.ReusableContent;

    /// <summary>Defaults to <c>RelayKenticoOptions.DefaultLanguageName</c> when null.</summary>
    public string? LanguageName { get; set; }

    /// <summary>
    /// Required when <see cref="ContentKind"/> is <see cref="RelayContentKind.WebPage"/>.
    /// Defaults to <c>RelayKenticoOptions.DefaultWebsiteChannelName</c> when null.
    /// </summary>
    public string? WebsiteChannelName { get; set; }

    /// <summary>Columns to fetch and return. At least one column is required.</summary>
    public List<string> Columns { get; set; } = [];

    /// <summary>Optional column-equality filters. All conditions are ANDed together.</summary>
    public Dictionary<string, JsonElement>? WhereEquals { get; set; }
}
