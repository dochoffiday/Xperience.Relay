using System.Collections.Concurrent;
using CMS.DataEngine;
using CMS.Helpers;
using Microsoft.Extensions.Logging;
using Xperience.Relay.Contracts;
using Xperience.Relay.Core;

namespace Xperience.Relay.Kentico.Security;

/// <summary>
/// Validates API keys against the admin-managed <c>Relay_ApiKey</c> table. The (tiny) table is held in memory and
/// refreshed whenever Kentico reports a change to it, so adding or deleting a key takes effect immediately and
/// requests don't hit the database.
/// </summary>
internal sealed class KenticoRelayApiKeyValidator(
    IInfoProvider<RelayApiKeyInfo> keyProvider,
    IProgressiveCache cache,
    ILogger<KenticoRelayApiKeyValidator> logger) : IRelayApiKeyValidator
{
    private const int CacheMinutes = 10;
    private static readonly TimeSpan LastUsedWriteInterval = TimeSpan.FromMinutes(5);

    private readonly ConcurrentDictionary<int, DateTime> _lastUsedWrites = new();

    public async Task<RelayApiKeyIdentity?> ValidateAsync(string presentedKey, CancellationToken cancellationToken = default)
    {
        var keys = await LoadKeysAsync();

        if (!keys.TryGetValue(RelayApiKeyGenerator.Hash(presentedKey), out var key))
        {
            return null;
        }

        RecordUse(key.Id);
        return new RelayApiKeyIdentity(key.Id, key.Name, key.Access);
    }

    private Task<Dictionary<string, CachedKey>> LoadKeysAsync() =>
        cache.LoadAsync(async cacheSettings =>
        {
            cacheSettings.CacheDependency = CacheHelper.GetCacheDependency(
                CacheHelper.GetCacheItemName(null, RelayApiKeyInfo.OBJECT_TYPE, "all"));

            var keys = new Dictionary<string, CachedKey>(StringComparer.Ordinal);

            foreach (var info in await keyProvider.Get().GetEnumerableTypedResultAsync())
            {
                var access = (RelayAccess)info.ApiKeyAccess;
                if (!Enum.IsDefined(access))
                {
                    logger.LogWarning("Relay API key '{KeyName}' has an unrecognised access level ({Access}) and was ignored.", info.ApiKeyDisplayName, info.ApiKeyAccess);
                    continue;
                }

                keys[info.ApiKeyHash] = new CachedKey(info.ApiKeyID, info.ApiKeyDisplayName, access);
            }

            return keys;
        }, new CacheSettings(CacheMinutes, "Xperience.Relay", "ApiKeys"));

    // Written straight to the table rather than through the info object: updating the object would invalidate the
    // cached keys on every use. Throttled, so a busy key costs one small write every few minutes, not one per request.
    private void RecordUse(int keyId)
    {
        var now = DateTime.Now;

        if (_lastUsedWrites.TryGetValue(keyId, out var lastWrite) && now - lastWrite < LastUsedWriteInterval)
        {
            return;
        }

        _lastUsedWrites[keyId] = now;

        try
        {
            ConnectionHelper.ExecuteNonQuery(
                $"UPDATE {RelayApiKeyInfo.TABLE_NAME} SET ApiKeyLastUsed = @LastUsed WHERE ApiKeyID = @ApiKeyID",
                new QueryDataParameters { { "@LastUsed", now }, { "@ApiKeyID", keyId } },
                QueryTypeEnum.SQLQuery);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Could not record the last-used time for relay API key {KeyId}.", keyId);
        }
    }

    private sealed record CachedKey(int Id, string Name, RelayAccess Access);
}
