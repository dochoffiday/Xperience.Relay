namespace Xperience.Relay.Contracts;

/// <summary>
/// Describes one verb supported by a deployed relay endpoint, returned by its discovery endpoint.
/// Lets a client validate a batch locally before sending it, instead of failing partway through
/// a batch against a verb the deployed relay doesn't actually support yet.
/// </summary>
public class RelayVerbDescriptor
{
    /// <summary>The verb, e.g. <c>create-web-page</c>.</summary>
    public string Verb { get; set; } = string.Empty;

    /// <summary>
    /// Parameter name -> CLR type name, for basic client-side validation. The names are .NET's short type names, so
    /// generic types read like <c>List`1</c> and optional values like <c>Nullable`1</c>; the access tier isn't included.
    /// </summary>
    public Dictionary<string, string> Parameters { get; set; } = new();
}

/// <summary>
/// The response of a relay endpoint's discovery endpoint: which verbs it currently supports,
/// and the version of Xperience.Relay it's running.
/// </summary>
public class RelayDiscoveryResponse
{
    /// <summary>The version of Xperience.Relay the site is running. It is an assembly version, so 0.5.0 reads as 0.5.0.0.</summary>
    public string RelayVersion { get; set; } = string.Empty;

    /// <summary>Every verb the site accepts.</summary>
    public List<RelayVerbDescriptor> Verbs { get; set; } = new();
}
