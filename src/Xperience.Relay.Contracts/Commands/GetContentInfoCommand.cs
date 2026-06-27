namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Fetches a reusable content item's system fields (content type, language, workspace, ...)
/// without its content-type-specific field data. Result payload is a <see cref="ContentInfo"/>.
/// </summary>
[RelayCommand("get-content-info")]
public class GetContentInfoCommand : IRelayCommand
{
    public int ContentItemId { get; set; }
}
