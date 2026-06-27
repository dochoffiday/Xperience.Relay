namespace Xperience.Relay.Contracts;

/// <summary>
/// Describes one verb supported by a deployed relay endpoint, returned by its discovery endpoint.
/// Lets a client validate a batch locally before sending it, instead of failing partway through
/// a batch against a verb the deployed relay doesn't actually support yet.
/// </summary>
public class RelayVerbDescriptor
{
    public string Verb { get; set; } = string.Empty;

    /// <summary>Parameter name -> CLR type name, for basic client-side validation.</summary>
    public Dictionary<string, string> Parameters { get; set; } = new();
}

/// <summary>
/// The response of a relay endpoint's discovery endpoint: which verbs it currently supports,
/// and the version of Xperience.Relay it's running.
/// </summary>
public class RelayDiscoveryResponse
{
    public string RelayVersion { get; set; } = string.Empty;
    public List<RelayVerbDescriptor> Verbs { get; set; } = new();
}
