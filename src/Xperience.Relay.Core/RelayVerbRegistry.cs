using System.Reflection;
using Xperience.Relay.Contracts;

namespace Xperience.Relay.Core;

/// <summary>
/// Maps verb names to the <see cref="IRelayCommand"/> type that implements them, built by scanning
/// assemblies for types carrying <see cref="RelayCommandAttribute"/>. Used by the HTTP layer to turn
/// a verb + JSON payload into a concrete command type, and to answer discovery requests.
/// </summary>
public class RelayVerbRegistry
{
    private readonly Dictionary<string, Type> _commandTypesByVerb;

    public RelayVerbRegistry(IEnumerable<Assembly> assembliesToScan)
    {
        _commandTypesByVerb = assembliesToScan
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IRelayCommand).IsAssignableFrom(t) && !t.IsAbstract)
            .Select(t => new { Type = t, Attribute = t.GetCustomAttribute<RelayCommandAttribute>() })
            .Where(x => x.Attribute is not null)
            .ToDictionary(x => x.Attribute!.Verb, x => x.Type, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyCollection<string> Verbs => _commandTypesByVerb.Keys;

    public bool TryGetCommandType(string verb, out Type commandType) =>
        _commandTypesByVerb.TryGetValue(verb, out commandType!);
}
