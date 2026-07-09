namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Deletes a language variant of a reusable content item. If this is the last language variant
/// the parent content item is also removed. Returns <see cref="RelayCommandResult.Success"/> on
/// success.
/// </summary>
[RelayCommand("delete-content-item")]
public class DeleteContentItemCommand : IRelayCommand
{
    public int ContentItemId { get; set; }

    /// <summary>Defaults to <c>RelayKenticoOptions.DefaultLanguageName</c> when null.</summary>
    public string? LanguageName { get; set; }
}
