namespace Xperience.Relay.Client;

/// <summary>
/// Configuration for <see cref="RelayClient"/>. Register via
/// <c>services.AddRelayClient(options => ...)</c>.
/// </summary>
public class RelayClientOptions
{
    /// <summary>
    /// Base address of the deployed relay's endpoints (i.e. the address
    /// <see cref="Xperience.Relay.Hosting.RelayHostingOptions.BasePath"/> was mapped under on the host),
    /// e.g. <c>https://example.com/api/relay/</c>. Must end with a trailing slash -- <see cref="RelayClient"/>
    /// resolves "commands", "batch", and "verbs" relative to it, and a missing slash would otherwise
    /// silently replace the last path segment instead of appending.
    /// </summary>
    public Uri BaseAddress { get; set; } = null!;

    /// <summary>Header to send <see cref="ApiKey"/> in. Must match the host's configured header name.</summary>
    public string ApiKeyHeaderName { get; set; } = "X-Relay-Api-Key";

    /// <summary>Shared secret matching the host's configured API key.</summary>
    public string ApiKey { get; set; } = string.Empty;
}
