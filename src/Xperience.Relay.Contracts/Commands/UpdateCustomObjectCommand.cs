using System.Text.Json;

namespace Xperience.Relay.Contracts.Commands;

[RelayCommand("update-custom-object", Access = RelayAccess.Admin)]
public class UpdateCustomObjectCommand : IRelayCommand
{
    public string ObjectTypeName { get; set; } = string.Empty;
    public int Id { get; set; }
    public Dictionary<string, JsonElement> Fields { get; set; } = [];
}
