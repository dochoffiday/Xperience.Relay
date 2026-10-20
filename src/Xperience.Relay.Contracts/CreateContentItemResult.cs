namespace Xperience.Relay.Contracts;

/// <summary>
/// Returned in <see cref="RelayCommandResult.Data"/> after a successful "create-content-item" command.
/// </summary>
public class CreateContentItemResult
{
    /// <summary>GUID of the new item.</summary>
    public Guid ContentItemGuid { get; set; }

    /// <summary>ID of the new item.</summary>
    public int ContentItemId { get; set; }
}
