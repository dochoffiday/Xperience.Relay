namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Fetches a reusable content item's system fields (content type, language, workspace, ...)
/// without its content-type-specific field data. Returns a <see cref="ContentInfo"/> in <see cref="RelayCommandResult.Data"/>.
/// </summary>
[RelayCommand("get-content-info", Access = RelayAccess.Read)]
public class GetContentInfoCommand : IRelayCommand
{
    /// <summary>ID of the reusable content item. Required.</summary>
    public int ContentItemId { get; set; }
}
