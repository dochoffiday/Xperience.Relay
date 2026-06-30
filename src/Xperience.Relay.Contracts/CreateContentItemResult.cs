namespace Xperience.Relay.Contracts;

/// <summary>
/// Returned in <see cref="RelayCommandResult.Data"/> after a successful "create-content-item" command.
/// </summary>
public class CreateContentItemResult
{
    public Guid ContentItemGuid { get; set; }
    public int ContentItemId { get; set; }
}
