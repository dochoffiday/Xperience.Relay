using System.Security.Cryptography;
using System.Text;

namespace Xperience.Relay.Core;

/// <summary>A newly generated key. <see cref="Key"/> is shown once and never stored; only <see cref="Hash"/> is.</summary>
public sealed record GeneratedRelayApiKey(string Key, string Prefix, string Hash);

/// <summary>
/// Creates and hashes relay API keys. Keys are 256 bits of randomness, so an unsalted SHA-256 is enough
/// to store them -- there is nothing to brute-force.
/// </summary>
public static class RelayApiKeyGenerator
{
    private const string KeyPrefix = "rly_";
    private const int SecretByteLength = 32;
    private const int VisiblePrefixLength = 10;

    public static GeneratedRelayApiKey Generate()
    {
        var secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(SecretByteLength))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        var key = KeyPrefix + secret;
        return new GeneratedRelayApiKey(key, key.Substring(0, VisiblePrefixLength), Hash(key));
    }

    public static string Hash(string key) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(key)));
}
