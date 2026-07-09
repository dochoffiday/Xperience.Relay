namespace Xperience.Relay.Contracts;

/// <summary>
/// Returned in <see cref="RelayCommandResult.Data"/> after a successful "create-web-page" command.
/// </summary>
public class CreateWebPageResult
{
    public int WebPageItemId { get; set; }
    public Guid WebPageItemGuid { get; set; }
}
