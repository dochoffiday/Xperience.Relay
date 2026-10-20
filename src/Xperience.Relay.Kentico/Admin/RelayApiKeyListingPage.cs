using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using Kentico.Xperience.Admin.Base;
using Xperience.Relay.Contracts;
using Xperience.Relay.Kentico.Admin;
using Xperience.Relay.Kentico.Security;

[assembly: UIPage(
    parentType: typeof(RelayApiKeysApplication),
    slug: "keys",
    uiPageType: typeof(RelayApiKeyListingPage),
    name: "API keys",
    templateName: TemplateNames.LISTING,
    order: 0)]

namespace Xperience.Relay.Kentico.Admin;

public class RelayApiKeyListingPage : ListingPage
{
    protected override string ObjectType => RelayApiKeyInfo.OBJECT_TYPE;

    public override async Task ConfigurePage()
    {
        PageConfiguration.ColumnConfigurations
            .AddColumn(nameof(RelayApiKeyInfo.ApiKeyDisplayName), "Name", searchable: true)
            .AddColumn(nameof(RelayApiKeyInfo.ApiKeyPrefix), "Key", sortable: false)
            .AddColumn(nameof(RelayApiKeyInfo.ApiKeyAccess), "Access", formatter: FormatAccess)
            .AddColumn(nameof(RelayApiKeyInfo.ApiKeyCreated), "Created", defaultSortDirection: SortTypeEnum.Desc, formatter: FormatDate)
            .AddColumn(nameof(RelayApiKeyInfo.ApiKeyLastUsed), "Last used", formatter: FormatLastUsed);

        PageConfiguration.HeaderActions.AddLink<RelayApiKeyCreatePage>("New API key");
        PageConfiguration.TableActions.AddDeleteAction(nameof(Delete));

        await base.ConfigurePage();
    }

    [PageCommand(Permission = SystemPermissions.DELETE)]
    public override Task<ICommandResponse<RowActionResult>> Delete(int id) => base.Delete(id);

    private static string FormatAccess(object value, IDataContainer row) =>
        Enum.IsDefined(typeof(RelayAccess), ValidationHelper.GetInteger(value, 0))
            ? ((RelayAccess)ValidationHelper.GetInteger(value, 0)).ToString()
            : "Unknown";

    private static string FormatDate(object value, IDataContainer row) =>
        ValidationHelper.GetDateTime(value, DateTimeHelper.ZERO_TIME) is var date && date != DateTimeHelper.ZERO_TIME
            ? date.ToString("yyyy-MM-dd HH:mm")
            : string.Empty;

    private static string FormatLastUsed(object value, IDataContainer row) =>
        ValidationHelper.GetDateTime(value, DateTimeHelper.ZERO_TIME) is var date && date != DateTimeHelper.ZERO_TIME
            ? date.ToString("yyyy-MM-dd HH:mm")
            : "Never";
}
