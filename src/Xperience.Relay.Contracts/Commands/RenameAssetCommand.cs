namespace Xperience.Relay.Contracts.Commands;

[RelayCommand("rename-asset")]
public class RenameAssetCommand : IRelayCommand
{
    public int ContentItemId { get; set; }
    public string FieldName { get; set; } = string.Empty;

    /// <summary>New filename including extension, e.g. "report-2024.pdf".</summary>
    public string AssetName { get; set; } = string.Empty;

    /// <summary>Defaults to <c>RelayKenticoOptions.DefaultLanguageName</c> when null.</summary>
    public string? LanguageName { get; set; }
}
