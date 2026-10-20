using System.Text.Json;

namespace Xperience.Relay.Contracts;

/// <summary>
/// The wire format for a single command, both on its own ("POST /commands") and inside a
/// <see cref="RelayBatchRequest"/>. <see cref="Verb"/> identifies which <see cref="IRelayCommand"/>
/// type to deserialize <see cref="Parameters"/> into.
/// </summary>
public class RelayCommandEnvelope
{
    /// <summary>The command's verb, e.g. <c>create-web-page</c>. See "GET /verbs" for the full list.</summary>
    public string Verb { get; set; } = string.Empty;

    /// <summary>
    /// Reserved for naming a command's result so a later command in the same batch could refer to it. The
    /// relay accepts the value but doesn't do anything with it yet: commands in a batch can't use each
    /// other's results, so look up the IDs you need before building the batch.
    /// </summary>
    public string? As { get; set; }

    /// <summary>The command's properties, as the JSON object they serialize to (property names in camelCase).</summary>
    public JsonElement Parameters { get; set; }
}
