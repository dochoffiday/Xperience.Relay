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

    public RelayCommandAttribute(string verb)
    {
        Verb = verb;
    }
}
