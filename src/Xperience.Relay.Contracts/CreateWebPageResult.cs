namespace Xperience.Relay.Contracts;

/// <summary>
/// Returned in <see cref="RelayCommandResult.Data"/> after a successful "create-web-page" command.
/// </summary>
public class CreateWebPageResult
{
    /// <summary>ID of the new page, for the other page commands.</summary>
    public int WebPageItemId { get; set; }

    /// <summary>GUID of the new page.</summary>
    public Guid WebPageItemGuid { get; set; }
}
