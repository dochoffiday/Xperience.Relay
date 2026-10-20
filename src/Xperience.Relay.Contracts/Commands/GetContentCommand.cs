namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Fetches a reusable content item's system fields plus the field values of its latest version (drafts included).
/// Returns a <see cref="ContentData"/> in <see cref="RelayCommandResult.Data"/>.
/// </summary>
[RelayCommand("get-content", Access = RelayAccess.Read)]
public class GetContentCommand : IRelayCommand
{
    /// <summary>ID of the reusable content item. Required.</summary>
    public int ContentItemId { get; set; }

    /// <summary>Defaults to <c>RelayKenticoOptions.DefaultLanguageName</c> when null.</summary>
    public string? LanguageName { get; set; }
}
