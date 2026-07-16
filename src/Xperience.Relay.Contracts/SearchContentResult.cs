namespace Xperience.Relay.Contracts;

public class SearchContentResult
{
    public List<SearchContentMatch> Matches { get; set; } = [];
}

public class SearchContentMatch
{
    public int ContentItemId { get; set; }
    public string ContentItemName { get; set; } = string.Empty;
    public string ContentTypeName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public Dictionary<string, string> MatchedFields { get; set; } = [];
}
