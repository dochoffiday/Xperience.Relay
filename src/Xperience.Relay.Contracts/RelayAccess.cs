namespace Xperience.Relay.Contracts;

/// <summary>
/// The tier of access a command requires, and the tier an API key grants. Each tier includes
/// everything in the tiers below it.
/// </summary>
public enum RelayAccess
{
    /// <summary>Typed read commands (get-*, query-web-page-items, search-content, ...).</summary>
    Read = 1,

    /// <summary>Content and taxonomy changes (create, update, move, publish, delete, assets).</summary>
    Write = 2,

    /// <summary>Raw SQL, generic custom-object access, bulk contact deletion.</summary>
    Admin = 3,
}
