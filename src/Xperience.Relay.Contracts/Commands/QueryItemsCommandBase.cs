using System.Text.Json;

namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Shared base for <see cref="QueryWebPageItemsCommand"/> and <see cref="QueryReusableItemsCommand"/>.
/// </summary>
public abstract class QueryItemsCommandBase : IRelayCommand
{
    /// <summary>
    /// Code names of the content types to include, e.g. <c>DancingGoat.ArticlePage</c>. Required; at least one, and at
    /// most 25.
    /// </summary>
    public List<string> ContentTypeNames { get; set; } = [];

    /// <summary>Defaults to <c>RelayKenticoOptions.DefaultLanguageName</c> when null.</summary>
    public string? LanguageName { get; set; }

    /// <summary>
    /// Columns to fetch and return. Required; at least one, and at most 500. Each is a plain column name (letters, digits
    /// and underscores) that exists on every content type listed: one of its fields, or a system column such as
    /// <c>ContentItemID</c>.
    /// </summary>
    public List<string> Columns { get; set; } = [];

    /// <summary>
    /// Optional column-equality filters, at most 50. All conditions are ANDed together. Each column must also be listed in
    /// <see cref="Columns"/>, and each value is text, a number, <c>true</c>, <c>false</c> or <c>null</c>.
    /// </summary>
    public Dictionary<string, JsonElement>? WhereEquals { get; set; }

    /// <summary>
    /// When true, returns the published version of each item, and leaves out items that have none. Defaults to false,
    /// which returns each item's latest version, drafts included.
    /// </summary>
    public bool PublishedOnly { get; set; }
}
