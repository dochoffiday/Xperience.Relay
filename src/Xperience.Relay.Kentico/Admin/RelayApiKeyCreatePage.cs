using System.Net;
using CMS.DataEngine;
using CMS.Membership;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Base.Forms.Internal;
using Xperience.Relay.Contracts;
using Xperience.Relay.Core;
using Xperience.Relay.Kentico.Admin;
using Xperience.Relay.Kentico.Security;

[assembly: UIPage(
    parentType: typeof(RelayApiKeyListingPage),
    slug: "create",
    uiPageType: typeof(RelayApiKeyCreatePage),
    name: "Create API key",
    templateName: TemplateNames.EDIT,
    order: 10)]

namespace Xperience.Relay.Kentico.Admin;

public class CreateApiKeyModel
{
    [TextInputComponent(Label = "Name", Order = 1, ExplanationText = "Who or what will use this key, e.g. \"Intranet WebForms app\".")]
    [RequiredValidationRule]
    [MaxLengthValidationRule(200)]
    public string Name { get; set; } = string.Empty;

    [DropDownComponent(
        Label = "Access",
        Order = 2,
        Options = "Read;Read\r\nWrite;Write\r\nAdmin;Admin",
        ExplanationText = "Read: fetch and query content. Write: also create, update, move, publish and delete content. Admin: also raw SQL, custom-object access and bulk contact deletion.")]
    public string Access { get; set; } = nameof(RelayAccess.Read);
}

[UIEvaluatePermission(SystemPermissions.CREATE)]
public class RelayApiKeyCreatePage(
    IFormItemCollectionProvider formItemCollectionProvider,
    IFormDataBinder formDataBinder,
    IInfoProvider<RelayApiKeyInfo> keyProvider,
    IPageLinkGenerator pageLinkGenerator) : ModelEditPage<CreateApiKeyModel>(formItemCollectionProvider, formDataBinder)
{
    protected override CreateApiKeyModel Model { get; } = new();

    public override Task ConfigurePage()
    {
        PageConfiguration.SubmitConfiguration.Label = "Create API key";
        return base.ConfigurePage();
    }

    protected override Task<ICommandResponse> ProcessFormData(CreateApiKeyModel model, ICollection<IFormItem> formItems)
    {
        if (!Enum.TryParse<RelayAccess>(model.Access, out var access) || !Enum.IsDefined(access))
        {
            return Task.FromResult<ICommandResponse>(
                ResponseFrom(new FormSubmissionResult(FormSubmissionStatus.ValidationFailure))
                    .AddErrorMessage("Choose an access level."));
        }

        var name = model.Name.Trim();
        if (name.Length == 0)
        {
            return Task.FromResult<ICommandResponse>(
                ResponseFrom(new FormSubmissionResult(FormSubmissionStatus.ValidationFailure))
                    .AddErrorMessage("Enter a name for the key."));
        }

        var generated = RelayApiKeyGenerator.Generate();

        keyProvider.Set(new RelayApiKeyInfo
        {
            ApiKeyDisplayName = name,
            ApiKeyPrefix = generated.Prefix,
            ApiKeyHash = generated.Hash,
            ApiKeyAccess = (int)access,
            ApiKeyCreated = DateTime.Now,
        });

        // The key exists nowhere but this message -- only its hash is stored. A warning, because the admin UI
        // dismisses info and success messages after 4 seconds but keeps warnings until they're closed.
        var message = $"API key <strong>{WebUtility.HtmlEncode(name)}</strong> created. Copy it now; it won't be shown again:<br/><code>{generated.Key}</code>";

        return Task.FromResult<ICommandResponse>(
            NavigateTo(pageLinkGenerator.GetPath<RelayApiKeyListingPage>(), RefetchAll)
                .AddWarningMessage(message, messageAsHtml: true));
    }

    [PageCommand(Permission = SystemPermissions.CREATE)]
    public override Task<ICommandResponse> Submit(FormSubmissionCommandArguments args) => base.Submit(args);

    [PageCommand(Permission = SystemPermissions.CREATE)]
    public override Task<ICommandResponse<FormChangeResult>> Change(FormChangeCommandArguments args) => base.Change(args);
}
