namespace Xperience.Relay.Hosting;

/// <summary>
/// Configuration for the relay's HTTP endpoints. Register via
/// <c>services.Configure&lt;RelayHostingOptions&gt;(...)</c> in the host application.
/// </summary>
public class RelayHostingOptions
{
    /// <summary>Base route the relay endpoints are mapped under.</summary>
    public string BasePath { get; set; } = "/api/relay";

    /// <summary>Header the caller must send a matching API key in.</summary>
    public string ApiKeyHeaderName { get; set; } = "X-Relay-Api-Key";

    /// <summary>Shared secret callers must present. Must be configured -- there is no default.</summary>
    public string ApiKey { get; set; } = string.Empty;
}
