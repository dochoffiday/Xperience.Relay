using System.Data;
using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using Xperience.Relay.Kentico.Security;

[assembly: RegisterObjectType(typeof(RelayApiKeyInfo), RelayApiKeyInfo.OBJECT_TYPE)]

namespace Xperience.Relay.Kentico.Security;

/// <summary>
/// A relay API key. Only a hash of the key is stored; the key itself is shown once when it's created.
/// </summary>
public class RelayApiKeyInfo : AbstractInfo<RelayApiKeyInfo, IInfoProvider<RelayApiKeyInfo>>, IInfoWithId
{
    public const string OBJECT_TYPE = "relay.apikey";
    public const string CLASS_NAME = "Relay.ApiKey";
    public const string TABLE_NAME = "Relay_ApiKey";

    public static readonly ObjectTypeInfo TYPEINFO = new(
        typeof(IInfoProvider<RelayApiKeyInfo>), OBJECT_TYPE, CLASS_NAME,
        nameof(ApiKeyID), null, null, null, nameof(ApiKeyDisplayName), null, null, null)
    {
        TouchCacheDependencies = true,

        // Keys are per-environment secrets: never serialize them into a CI/CD repository.
        ContinuousIntegrationSettings = { Enabled = false },
    };

    [DatabaseField]
    public virtual int ApiKeyID
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(ApiKeyID)), 0);
        set => SetValue(nameof(ApiKeyID), value);
    }

    [DatabaseField]
    public virtual string ApiKeyDisplayName
    {
        get => ValidationHelper.GetString(GetValue(nameof(ApiKeyDisplayName)), string.Empty);
        set => SetValue(nameof(ApiKeyDisplayName), value);
    }

    /// <summary>The first few characters of the key, so keys can be told apart in the admin listing.</summary>
    [DatabaseField]
    public virtual string ApiKeyPrefix
    {
        get => ValidationHelper.GetString(GetValue(nameof(ApiKeyPrefix)), string.Empty);
        set => SetValue(nameof(ApiKeyPrefix), value);
    }

    [DatabaseField]
    public virtual string ApiKeyHash
    {
        get => ValidationHelper.GetString(GetValue(nameof(ApiKeyHash)), string.Empty);
        set => SetValue(nameof(ApiKeyHash), value);
    }

    /// <summary>A <c>RelayAccess</c> value.</summary>
    [DatabaseField]
    public virtual int ApiKeyAccess
    {
        get => ValidationHelper.GetInteger(GetValue(nameof(ApiKeyAccess)), 0);
        set => SetValue(nameof(ApiKeyAccess), value);
    }

    [DatabaseField]
    public virtual DateTime ApiKeyCreated
    {
        get => ValidationHelper.GetDateTime(GetValue(nameof(ApiKeyCreated)), DateTimeHelper.ZERO_TIME);
        set => SetValue(nameof(ApiKeyCreated), value);
    }

    /// <summary><see cref="DateTimeHelper.ZERO_TIME"/> when the key has never been used.</summary>
    [DatabaseField]
    public virtual DateTime ApiKeyLastUsed
    {
        get => ValidationHelper.GetDateTime(GetValue(nameof(ApiKeyLastUsed)), DateTimeHelper.ZERO_TIME);
        set => SetValue(nameof(ApiKeyLastUsed), value);
    }

    protected override void DeleteObject()
    {
        Provider.Delete(this);
    }

    protected override void SetObject()
    {
        Provider.Set(this);
    }

    public RelayApiKeyInfo()
        : base(TYPEINFO)
    {
    }

    public RelayApiKeyInfo(DataRow dr)
        : base(TYPEINFO, dr)
    {
    }
}
