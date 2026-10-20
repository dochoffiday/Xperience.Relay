using Xperience.Relay.Kentico.Security;

namespace Xperience.Relay.Kentico.Internal;

/// <summary>
/// Things relay's generic commands (custom objects, raw SQL) must not touch, so a key can't read other keys'
/// hashes or mint itself a higher tier. A guardrail, not a security boundary: Admin keys are fully trusted.
/// </summary>
internal static class ProtectedObjects
{
    public static bool IsProtectedObjectType(string objectTypeName) =>
        string.Equals(objectTypeName, RelayApiKeyInfo.OBJECT_TYPE, StringComparison.OrdinalIgnoreCase);

    public static bool MentionsProtectedTable(string sqlText) =>
        sqlText.Contains(RelayApiKeyInfo.TABLE_NAME, StringComparison.OrdinalIgnoreCase);
}
