using CMS.Membership;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.UIPages;
using Xperience.Relay.Kentico.Admin;

[assembly: UIApplication(
    identifier: RelayApiKeysApplication.IDENTIFIER,
    type: typeof(RelayApiKeysApplication),
    slug: "relay-api-keys",
    name: "Relay API keys",
    category: BaseApplicationCategories.CONFIGURATION,
    icon: Icons.Key,
    templateName: TemplateNames.SECTION_LAYOUT)]

namespace Xperience.Relay.Kentico.Admin;

[UIPermission(SystemPermissions.VIEW)]
[UIPermission(SystemPermissions.CREATE)]
[UIPermission(SystemPermissions.DELETE)]
public class RelayApiKeysApplication : ApplicationPage
{
    public const string IDENTIFIER = "Xperience.Relay.ApiKeys";
}
