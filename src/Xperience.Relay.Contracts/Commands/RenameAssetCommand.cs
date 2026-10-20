namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Renames the file in an existing asset field of a reusable content item, without sending the file over the wire.
/// The item's published/draft state is preserved: a published item is published again with the new name, and a draft
/// stays a draft.
/// </summary>
[RelayCommand("rename-asset", Access = RelayAccess.Write)]
public class RenameAssetCommand : IRelayCommand
{
    /// <summary>ID of the reusable content item. Required.</summary>
    public int ContentItemId { get; set; }

    /// <summary>Name of the asset field, e.g. <c>ImageFile</c>. Required.</summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>New filename including extension, e.g. "report-2024.pdf". Required.</summary>
    public string AssetName { get; set; } = string.Empty;

    /// <summary>Defaults to <c>RelayKenticoOptions.DefaultLanguageName</c> when null.</summary>
    public string? LanguageName { get; set; }
}
