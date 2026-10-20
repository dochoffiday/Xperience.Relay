namespace Xperience.Relay.Contracts.Commands;

/// <summary>
/// Runs an existing asset field's file through Kentico's asset optimization again (format conversion, quality, size
/// limits), without sending the file over the wire. The item's published/draft state is preserved. Useful after the
/// field's optimization settings change.
/// </summary>
[RelayCommand("reoptimize-asset", Access = RelayAccess.Write)]
public class ReoptimizeAssetCommand : IRelayCommand
{
    /// <summary>ID of the reusable content item. Required.</summary>
    public int ContentItemId { get; set; }

    /// <summary>Name of the asset field, e.g. <c>ImageFile</c>. Required.</summary>
    public string FieldName { get; set; } = string.Empty;

    /// <summary>Defaults to <c>RelayKenticoOptions.DefaultLanguageName</c> when null.</summary>
    public string? LanguageName { get; set; }
}
