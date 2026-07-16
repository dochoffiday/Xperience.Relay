using System.Text.Json;

namespace Xperience.Relay.Contracts.Commands;

[RelayCommand("create-custom-object")]
public class CreateCustomObjectCommand : IRelayCommand
{
    public string ObjectTypeName { get; set; } = string.Empty;
    public Dictionary<string, JsonElement> Fields { get; set; } = [];
}
