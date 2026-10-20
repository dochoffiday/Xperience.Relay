using System.Text.Json;

namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Creates a new web page under a parent web page. The page is created in
/// the <c>InitialDraft</c> status by default; set <see cref="PublishAfterCreate"/> to
/// true to immediately publish after creation. Every field is checked against the content type before anything is
/// created. Returns a <see cref="CreateWebPageResult"/> in <see cref="RelayCommandResult.Data"/>.
/// </summary>
[RelayCommand("create-web-page", Access = RelayAccess.Write)]
public class CreateWebPageCommand : IRelayCommand
{
    /// <summary>Website channel that owns the page. Defaults to <c>RelayKenticoOptions.DefaultWebsiteChannelName</c> when null.</summary>
    public string? WebsiteChannelName { get; set; }

    /// <summary>
    /// ID of the parent web page, or 0 (the default) for the root of the channel. The parent's content type has to allow
    /// the new page's content type as a child.
    /// </summary>
    public int ParentWebPageItemId { get; set; }

    /// <summary>Code name of the page's content type, e.g. <c>DancingGoat.ArticlePage</c>. Required.</summary>
    public string ContentTypeName { get; set; } = string.Empty;

    /// <summary>Display name of the new page. Required.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Defaults to <c>RelayKenticoOptions.DefaultLanguageName</c> when null.</summary>
    public string? LanguageName { get; set; }

    /// <summary>Optional URL slug. When null, Kentico generates one from the display name.</summary>
    public string? UrlSlug { get; set; }

    /// <summary>Plain field values by field name (text, numbers, booleans, dates, ...).</summary>
    public Dictionary<string, JsonElement>? Fields { get; set; }

    /// <summary>
    /// Linked item fields -- field name to list of content item GUIDs. The field must be a content item reference
    /// field.
    /// </summary>
    public Dictionary<string, List<Guid>>? LinkedItemFields { get; set; }

    /// <summary>Tag fields -- field name to list of tag GUIDs. The field must be a tag (taxonomy) field.</summary>
    public Dictionary<string, List<Guid>>? TagFields { get; set; }

    /// <summary>When true, publishes the page immediately after creation. Defaults to false.</summary>
    public bool PublishAfterCreate { get; set; }

    /// <summary>Optional binary assets to upload into asset fields. Each entry maps to one field.</summary>
    public List<RelayAsset>? Assets { get; set; }
}
