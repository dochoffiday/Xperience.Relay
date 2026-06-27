namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Fetches a reusable content item's system fields plus its content-type-specific field data.
/// Result payload is a <see cref="ContentData"/>.
/// </summary>
[RelayCommand("get-content")]
public class GetContentCommand : IRelayCommand
{
    public int ContentItemId { get; set; }

    /// <summary>Defaults to the handler's configured default language when null.</summary>
    public string? LanguageName { get; set; }
}
