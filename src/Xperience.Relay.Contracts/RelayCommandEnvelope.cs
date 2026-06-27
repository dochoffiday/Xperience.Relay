using System.Text.Json;

namespace Xperience.Relay.Contracts;

/// <summary>
/// The wire format for a single command inside a <see cref="RelayBatchRequest"/>. <see cref="Verb"/>
/// identifies which <see cref="IRelayCommand"/> type to deserialize <see cref="Parameters"/> into.
/// <see cref="As"/> optionally names this command's result so a later command in the same batch can
/// reference it (e.g. a "move" command referencing the output of an earlier "query").
/// </summary>
public class RelayCommandEnvelope
{
    public string Verb { get; set; } = string.Empty;
    public string? As { get; set; }
    public JsonElement Parameters { get; set; }
}
