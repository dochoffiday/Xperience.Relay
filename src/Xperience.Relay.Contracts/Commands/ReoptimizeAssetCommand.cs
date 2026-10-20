namespace Xperience.Relay.Contracts.Commands;

[RelayCommand("reoptimize-asset", Access = RelayAccess.Write)]
public class ReoptimizeAssetCommand : IRelayCommand
{
    public int ContentItemId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string? LanguageName { get; set; }
}
