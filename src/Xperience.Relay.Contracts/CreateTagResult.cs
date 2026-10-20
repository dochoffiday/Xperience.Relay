namespace Xperience.Relay.Contracts;

/// <summary>Returned in <see cref="RelayCommandResult.Data"/> after a successful "create-tag" command.</summary>
public class CreateTagResult
{
    /// <summary>ID of the new tag.</summary>
    public int TagId { get; set; }

    /// <summary>GUID of the new tag, for putting it in a tag field.</summary>
    public Guid TagGuid { get; set; }

    /// <summary>The tag's code name.</summary>
    public string TagName { get; set; } = string.Empty;

    /// <summary>Display title of the new tag.</summary>
    public string TagTitle { get; set; } = string.Empty;
}
