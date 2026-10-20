namespace Xperience.Relay.Contracts;

/// <summary>Returned in <see cref="RelayCommandResult.Data"/> after a successful "search-content" command.</summary>
public class SearchContentResult
{
    /// <summary>The items that matched.</summary>
    public List<SearchContentMatch> Matches { get; set; } = [];
}

/// <summary>An item with at least one field that contains the searched text.</summary>
public class SearchContentMatch
{
    /// <summary>ID of the matching content item. For a web page this is the page's content item ID, not its web page ID.</summary>
    public int ContentItemId { get; set; }

    /// <summary>Code name of the matching item.</summary>
    public string ContentItemName { get; set; } = string.Empty;

    /// <summary>Code name of the item's content type.</summary>
    public string ContentTypeName { get; set; } = string.Empty;

    /// <summary>The page's tree path for web pages; null for reusable content items.</summary>
    public string? Location { get; set; }

    /// <summary>The fields that contain the text: field name to the field's whole value.</summary>
    public Dictionary<string, string> MatchedFields { get; set; } = [];
}
