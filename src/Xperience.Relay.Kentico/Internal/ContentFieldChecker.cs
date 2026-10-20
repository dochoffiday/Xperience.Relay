using System.Text.Json;
using CMS.ContentEngine.Internal;
using CMS.DataEngine;
using CMS.FormEngine;
using Xperience.Relay.Contracts.Commands;
using Xperience.Relay.Kentico.Handlers;

namespace Xperience.Relay.Kentico.Internal;

/// <summary>The fields a create or update command sets, as they arrive on the command.</summary>
public sealed record ContentFields(
    Dictionary<string, JsonElement>? Values,
    Dictionary<string, List<Guid>>? LinkedItems,
    Dictionary<string, List<Guid>>? Tags,
    List<RelayAsset>? Assets);

/// <summary>
/// Checks the fields of a create or update command against the content type, before anything is created or changed:
/// that every field exists, and that plain values fit their field. See <see cref="FieldValueChecker"/>.
/// </summary>
public sealed class ContentFieldChecker(IInfoProvider<ContentItemInfo> contentItemProvider)
{
    /// <summary>Returns why <paramref name="fields"/> don't fit the content type of an existing item, or null if they do.</summary>
    public string? CheckItem(int contentItemId, ContentFields fields)
    {
        var contentItem = contentItemProvider.Get()
            .WhereEquals(nameof(ContentItemInfo.ContentItemID), contentItemId)
            .FirstOrDefault();

        var contentType = contentItem is null
            ? null
            : DataClassInfoProvider.GetClasses()
                .WhereEquals(nameof(DataClassInfo.ClassID), contentItem.ContentItemContentTypeID)
                .FirstOrDefault();

        // An item or type that isn't there is reported by the caller's own checks.
        return contentType is null ? null : CheckType(contentType.ClassName, fields);
    }

    /// <summary>Returns why <paramref name="fields"/> don't fit the content type, or null if they do.</summary>
    public string? CheckType(string contentTypeName, ContentFields fields)
    {
        if (string.IsNullOrWhiteSpace(contentTypeName))
        {
            return null;
        }

        // The merged form of the type and its reusable field schemas, so schema fields count as fields too.
        var form = FormHelper.GetFormInfo(ReusableFieldSchemaUtils.GetPrefixedContentTypeName(contentTypeName), clone: false);

        if (form is null)
        {
            return null;
        }

        var rules = form
            .GetFields(visible: true, invisible: true, includeSystem: false)
            .Select(f => new FieldRule(f.Name, f.DataType, f.Size, f.Precision, f.AllowEmpty))
            .ToList();

        var values = fields.Values?.Select(f => KeyValuePair.Create(f.Key, QueryItemsHelpers.DeserializeJsonElement(f.Value))) ?? [];

        // Assets that are incomplete are skipped by the handlers, so they are not checked either.
        var typed = (fields.LinkedItems?.Keys ?? Enumerable.Empty<string>()).Select(name => new TypedField(name, FieldUse.LinkedItems))
            .Concat((fields.Tags?.Keys ?? Enumerable.Empty<string>()).Select(name => new TypedField(name, FieldUse.Tags)))
            .Concat((fields.Assets?.Where(a => a.IsValid()).Select(a => a.FieldName) ?? []).Select(name => new TypedField(name, FieldUse.Asset)));

        return FieldValueChecker.Check(contentTypeName, rules, values, typed);
    }
}
