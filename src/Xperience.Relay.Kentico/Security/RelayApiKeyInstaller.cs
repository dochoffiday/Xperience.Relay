using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Modules;

namespace Xperience.Relay.Kentico.Security;

/// <summary>
/// Creates the module and the <c>Relay_ApiKey</c> table in code, so apps that install the package don't have
/// to set anything up by hand. Safe to run on every startup: it only changes what's missing.
/// </summary>
internal static class RelayApiKeyInstaller
{
    private const string ModuleName = "Xperience.Relay";

    public static void Install(IInfoProvider<ResourceInfo> resourceProvider)
    {
        var resource = resourceProvider.Get(ModuleName) ?? new ResourceInfo();
        resource.ResourceDisplayName = "Xperience Relay";
        resource.ResourceName = ModuleName;
        resource.ResourceDescription = "API keys for the Xperience Relay HTTP endpoints.";
        resource.ResourceIsInDevelopment = false;

        if (resource.HasChanged)
        {
            resourceProvider.Set(resource);
        }

        var info = DataClassInfoProvider.GetDataClassInfo(RelayApiKeyInfo.CLASS_NAME) ?? DataClassInfo.New(RelayApiKeyInfo.CLASS_NAME);
        info.ClassName = RelayApiKeyInfo.CLASS_NAME;
        info.ClassTableName = RelayApiKeyInfo.TABLE_NAME;
        info.ClassDisplayName = "Relay API key";
        info.ClassResourceID = resource.ResourceID;
        info.ClassType = ClassType.OTHER;

        var form = FormHelper.GetBasicFormDefinition(nameof(RelayApiKeyInfo.ApiKeyID));
        form.AddFormItem(Field(nameof(RelayApiKeyInfo.ApiKeyDisplayName), FieldDataType.Text, size: 200));
        form.AddFormItem(Field(nameof(RelayApiKeyInfo.ApiKeyPrefix), FieldDataType.Text, size: 20));
        form.AddFormItem(Field(nameof(RelayApiKeyInfo.ApiKeyHash), FieldDataType.Text, size: 64));
        form.AddFormItem(Field(nameof(RelayApiKeyInfo.ApiKeyAccess), FieldDataType.Integer));
        form.AddFormItem(Field(nameof(RelayApiKeyInfo.ApiKeyCreated), FieldDataType.DateTime));
        form.AddFormItem(Field(nameof(RelayApiKeyInfo.ApiKeyLastUsed), FieldDataType.DateTime, allowEmpty: true));

        SetFormDefinition(info, form);

        if (info.HasChanged)
        {
            DataClassInfoProvider.SetDataClassInfo(info);
        }
    }

    private static FormFieldInfo Field(string name, string dataType, int size = 0, bool allowEmpty = false) => new()
    {
        Name = name,
        DataType = dataType,
        Size = size,
        AllowEmpty = allowEmpty,
        Enabled = true,
    };

    private static void SetFormDefinition(DataClassInfo info, FormInfo form)
    {
        if (info.ClassID > 0)
        {
            var existing = new FormInfo(info.ClassFormDefinition);
            existing.CombineWithForm(form, new CombineWithFormSettings());
            info.ClassFormDefinition = existing.GetXmlDefinition();
        }
        else
        {
            info.ClassFormDefinition = form.GetXmlDefinition();
        }
    }
}
