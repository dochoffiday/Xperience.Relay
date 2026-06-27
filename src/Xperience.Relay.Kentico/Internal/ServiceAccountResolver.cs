using CMS.DataEngine;
using CMS.Membership;
using Microsoft.Extensions.Options;

namespace Xperience.Relay.Kentico.Internal;

/// <summary>
/// Resolves the Kentico <see cref="UserInfo.UserID"/> attributed to relay-driven changes, based on
/// <see cref="RelayKenticoOptions.ServiceAccountUserName"/>.
/// </summary>
public class ServiceAccountResolver
{
    private readonly IInfoProvider<UserInfo> _userInfoProvider;
    private readonly RelayKenticoOptions _options;

    public ServiceAccountResolver(IInfoProvider<UserInfo> userInfoProvider, IOptions<RelayKenticoOptions> options)
    {
        _userInfoProvider = userInfoProvider;
        _options = options.Value;
    }

    public int ResolveUserId()
    {
        if (string.IsNullOrEmpty(_options.ServiceAccountUserName))
        {
            throw new InvalidOperationException(
                "RelayKenticoOptions.ServiceAccountUserName must be configured before relay commands that modify content can run.");
        }

        var user = _userInfoProvider.Get()
            .WhereEquals(nameof(UserInfo.UserName), _options.ServiceAccountUserName)
            .FirstOrDefault();

        return user?.UserID
            ?? throw new InvalidOperationException($"Relay service account user '{_options.ServiceAccountUserName}' was not found.");
    }
}
