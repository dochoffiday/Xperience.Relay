using System.Text.Json;

namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Shared base for <see cref="QueryWebPageItemsCommand"/> and <see cref="QueryReusableItemsCommand"/>.
/// </summary>
public abstract class QueryItemsCommandBase : IRelayCommand
{
    /// <summary>Content types to include. At least one entry is required.</summary>
    public List<string> ContentTypeNames { get; set; } = [];

    /// <summary>Defaults to <c>RelayKenticoOptions.DefaultLanguageName</c> when null.</summary>
    public string? LanguageName { get; set; }

    /// <summary>Columns to fetch and return. At least one entry is required.</summary>
    public List<string> Columns { get; set; } = [];

    /// <summary>Optional column-equality filters. All conditions are ANDed together.</summary>
    public Dictionary<string, JsonElement>? WhereEquals { get; set; }

    /// <summary>
    /// When true, returns the published version of each item, and leaves out items that have none. Defaults to false,
    /// which returns each item's latest version, drafts included.
    /// </summary>
    public bool PublishedOnly { get; set; }
}
