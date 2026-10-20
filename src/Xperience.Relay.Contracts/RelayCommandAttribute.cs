namespace Xperience.Relay.Contracts;

/// <summary>
/// Associates an <see cref="IRelayCommand"/> type with the verb name used to identify it over the
/// wire -- in a <see cref="RelayCommandEnvelope"/> sent to a relay endpoint, and in the verb list
/// returned by the discovery endpoint.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class RelayCommandAttribute : Attribute
{
    public string Verb { get; }

    /// <summary>
    /// The minimum API key access needed to run this command. Defaults to <see cref="RelayAccess.Admin"/>
    /// so a command nobody classified is never reachable by a lower-tier key.
    /// </summary>
    public RelayAccess Access { get; set; } = RelayAccess.Admin;

    public RelayCommandAttribute(string verb)
    {
        Verb = verb;
    }
}
