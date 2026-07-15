namespace Xperience.Relay.Contracts.Commands;

[RelayCommand("reoptimize-asset")]
public class ReoptimizeAssetCommand : IRelayCommand
{
    public int ContentItemId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string? LanguageName { get; set; }
}
