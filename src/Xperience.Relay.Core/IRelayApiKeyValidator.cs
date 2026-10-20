using Xperience.Relay.Contracts;

namespace Xperience.Relay.Core;

/// <summary>
/// Decides whether an API key presented to a relay endpoint is valid, and what it's allowed to do.
/// Xperience.Relay.Kentico provides the implementation backed by the admin-managed key table.
/// </summary>
public interface IRelayApiKeyValidator
{
    /// <summary>Returns the key's identity if <paramref name="presentedKey"/> is valid, otherwise null.</summary>
    Task<RelayApiKeyIdentity?> ValidateAsync(string presentedKey, CancellationToken cancellationToken = default);
}

/// <summary>An authenticated API key: which key it is and the access tier it grants.</summary>
public sealed record RelayApiKeyIdentity(int Id, string Name, RelayAccess Access);
