namespace Xperience.Relay.Kentico;

/// <summary>
/// Configuration for relay handlers that talk to the Kentico Xperience API. Register via
/// <c>services.Configure&lt;RelayKenticoOptions&gt;(...)</c> in the host application.
/// </summary>
public class RelayKenticoOptions
{
    /// <summary>
    /// User name of the Kentico user attributed to relay-driven changes (drafts, moves, ...) for
    /// auditing purposes. The user's actual Kentico permissions are not checked by the relay.
    /// </summary>
    public string ServiceAccountUserName { get; set; } = string.Empty;

    /// <summary>Language used by commands that don't specify one explicitly.</summary>
    public string DefaultLanguageName { get; set; } = "en";
}
