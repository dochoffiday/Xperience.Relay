using System.Text.Json;

namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Creates a new web page under a parent web page. The page is created in
/// <see cref="VersionStatus.InitialDraft"/> by default; set <see cref="PublishAfterCreate"/> to
/// true to immediately publish after creation. Returns a <see cref="CreateWebPageResult"/> in
/// <see cref="RelayCommandResult.Data"/>.
/// </summary>
[RelayCommand("create-web-page")]
public class CreateWebPageCommand : IRelayCommand
{
    /// <summary>Website channel that owns the page. Defaults to <c>RelayKenticoOptions.DefaultWebsiteChannelName</c> when null.</summary>
    public string? WebsiteChannelName { get; set; }

    /// <summary>ID of the parent web page. Use 0 to create at the root of the channel.</summary>
    public int ParentWebPageItemId { get; set; }

    /// <summary>Fully qualified content type name, e.g. "MyNamespace.ArticlePage".</summary>
    public string ContentTypeName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Defaults to <c>RelayKenticoOptions.DefaultLanguageName</c> when null.</summary>
    public string? LanguageName { get; set; }

    /// <summary>Optional URL slug. When null, Kentico generates one from the display name.</summary>
    public string? UrlSlug { get; set; }

    /// <summary>Content fields as JSON values, keyed by field name.</summary>
    public Dictionary<string, JsonElement>? Fields { get; set; }

    /// <summary>Linked item fields — field name to list of content item GUIDs.</summary>
    public Dictionary<string, List<Guid>>? LinkedItemFields { get; set; }

    /// <summary>Tag fields — field name to list of tag GUIDs.</summary>
    public Dictionary<string, List<Guid>>? TagFields { get; set; }

    /// <summary>When true, publishes the page immediately after creation. Defaults to false.</summary>
    public bool PublishAfterCreate { get; set; }
}
