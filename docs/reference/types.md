# Types

The JSON shapes Relay sends and receives. Property names are camelCase on the wire. In the tables, `guid` is a GUID written as a string, `datetime` is an ISO 8601 date-time string, `any` is any JSON value, `T[]` is an array, and `{ name: T }` is an object whose property names are chosen by the caller.

## Envelopes and results

What every call sends and every answer carries.

### RelayCommandEnvelope

The wire format for a single command, both on its own ("POST /commands") and inside a [`RelayBatchRequest`](types.md#relaybatchrequest). `verb` identifies which `IRelayCommand` type to deserialize `parameters` into.

| Field | Type | Description |
|---|---|---|
| `verb` | string | The command's verb, e.g. `create-web-page`. See "GET /verbs" for the full list. |
| `as` | string | Reserved for naming a command's result so a later command in the same batch could refer to it. The relay accepts the value but doesn't do anything with it yet: commands in a batch can't use each other's results, so look up the IDs you need before building the batch. |
| `parameters` | any | The command's properties, as the JSON object they serialize to (property names in camelCase). |

### RelayCommandResult

The outcome of executing a single `IRelayCommand`.

| Field | Type | Description |
|---|---|---|
| `success` | boolean | True when the command was carried out. False when it wasn't; `error` says why. |
| `message` | string | A sentence about what was done, for commands that report one. Null when the command failed. |
| `error` | string | Why the command couldn't be carried out. Null when it succeeded. |
| `data` | any | Verb-specific payload (e.g. a [`WebPageInfo`](types.md#webpageinfo) for [`get-page-info`](../commands/web-pages/get-page-info.md)). Null when the command failed or has nothing to return. Travels as JSON; `GetData<T>` reads it as the type the command returns. |

### RelayBatchRequest

A request to run an ordered list of commands against a relay endpoint in a single call.

| Field | Type | Description |
|---|---|---|
| `commands` | [RelayCommandEnvelope](types.md#relaycommandenvelope)[] | The commands to run, in order. |

### RelayBatchResponse

The results of a [`RelayBatchRequest`](types.md#relaybatchrequest), in the same order as the request's commands.

| Field | Type | Description |
|---|---|---|
| `results` | [RelayCommandResult](types.md#relaycommandresult)[] | One result per command of the request, in the same order. A command that fails doesn't stop the ones after it. |

### RelayDiscoveryResponse

The response of a relay endpoint's discovery endpoint: which verbs it currently supports, and the version of Xperience.Relay it's running.

| Field | Type | Description |
|---|---|---|
| `relayVersion` | string | The version of Xperience.Relay the site is running. It is an assembly version, so 0.5.0 reads as 0.5.0.0. |
| `verbs` | [RelayVerbDescriptor](types.md#relayverbdescriptor)[] | Every verb the site accepts. |

### RelayVerbDescriptor

Describes one verb supported by a deployed relay endpoint, returned by its discovery endpoint. Lets a client validate a batch locally before sending it, instead of failing partway through a batch against a verb the deployed relay doesn't actually support yet.

| Field | Type | Description |
|---|---|---|
| `verb` | string | The verb, e.g. `create-web-page`. |
| `parameters` | { name: string } | Parameter name -&gt; CLR type name, for basic client-side validation. The names are .NET's short type names, so generic types read like `List`1` and optional values like `Nullable`1`; the access tier isn't included. |

## Shared input types

A type that is part of the parameters of more than one command.

### RelayAsset

A binary file to upload into an asset field. Used by the create and update commands for content items and web pages. An entry that is missing its `fieldName`, `fileName` or `base64` is ignored (not reported as an error).

| Field | Type | Description |
|---|---|---|
| `fieldName` | string | Name of the content item field to populate, e.g. "FileAsset". |
| `fileName` | string | File name including extension, e.g. "workshop-audio.mp3". Drives the asset metadata name and extension. |
| `base64` | string | Base64-encoded bytes of the file. |

## Command results

What a successful command returns in `data`. Each command page says which one.

### ContentData

A content item's system fields plus its content-type-specific field data. Returned by [`get-content`](../commands/content-items/get-content.md). `fields` is untyped since its shape depends on the content type.

| Field | Type | Description |
|---|---|---|
| `contentItemId` | integer | ID of the content item. |
| `contentItemGuid` | guid | GUID of the content item. |
| `name` | string | The item's code name. |
| `contentType` | string | Code name of the item's content type, e.g. `DancingGoat.Image`. |
| `languageName` | string | Empty from [`get-content-info`](../commands/content-items/get-content-info.md) (the item's system fields don't vary by language); the requested language from [`get-content`](../commands/content-items/get-content.md). |
| `workspaceName` | string | The workspace's numeric ID, as text. Kentico's public API doesn't expose a workspace's name. |
| `contentFolderPath` | string | Reserved. Not filled in yet, so currently always null. |
| `fields` | { name: any } | Field values of the item's latest version, by field name, as JSON (text, numbers, dates, ...). Asset, tag and reference fields come as the JSON text Kentico stores in them. Only the content type's own fields are listed: fields that come from reusable field schemas are not included. |

### ContentInfo

System-level fields describing a reusable content item, without its content-type-specific field data. Returned by [`get-content-info`](../commands/content-items/get-content-info.md) -- cheap enough to use for existence checks.

| Field | Type | Description |
|---|---|---|
| `contentItemId` | integer | ID of the content item. |
| `contentItemGuid` | guid | GUID of the content item. |
| `name` | string | The item's code name. |
| `contentType` | string | Code name of the item's content type, e.g. `DancingGoat.Image`. |
| `languageName` | string | Empty from [`get-content-info`](../commands/content-items/get-content-info.md) (the item's system fields don't vary by language); the requested language from [`get-content`](../commands/content-items/get-content.md). |
| `workspaceName` | string | The workspace's numeric ID, as text. Kentico's public API doesn't expose a workspace's name. |
| `contentFolderPath` | string | Reserved. Not filled in yet, so currently always null. |

### ContentItemUsageEntry

A content item language variant that references another content item.

| Field | Type | Description |
|---|---|---|
| `contentItemId` | integer | ID of the referencing content item. For a web page this is the page's content item ID, not its web page ID. |
| `languageName` | string | Language of the referencing variant, e.g. `en`. |
| `displayName` | string | Display name of the referencing item in that language. |
| `latestVersionStatus` | string | Status of the variant's latest version: `InitialDraft`, `Draft`, `Published` or `Unpublished`. |
| `createdWhen` | datetime | When the variant was created. |
| `modifiedWhen` | datetime | When the variant was last changed. |
| `hasImageAsset` | boolean | True when the variant has an image asset. |
| `scheduledPublishWhen` | datetime | When the variant is scheduled to be published, if it is. |
| `scheduledUnpublishWhen` | datetime | When the variant is scheduled to be unpublished, if it is. |

### CreateContentItemResult

Returned in `data` after a successful [`create-content-item`](../commands/content-items/create-content-item.md) command.

| Field | Type | Description |
|---|---|---|
| `contentItemGuid` | guid | GUID of the new item. |
| `contentItemId` | integer | ID of the new item. |

### CreateCustomObjectResult

Returned in `data` after a successful [`create-custom-object`](../commands/custom-objects/create-custom-object.md) command.

| Field | Type | Description |
|---|---|---|
| `id` | integer | ID of the new object. |

### CreateTagResult

Returned in `data` after a successful [`create-tag`](../commands/taxonomies/create-tag.md) command.

| Field | Type | Description |
|---|---|---|
| `tagId` | integer | ID of the new tag. |
| `tagGuid` | guid | GUID of the new tag, for putting it in a tag field. |
| `tagName` | string | The tag's code name. |
| `tagTitle` | string | Display title of the new tag. |

### CreateTaxonomyResult

Returned in `data` after a successful [`create-taxonomy`](../commands/taxonomies/create-taxonomy.md) command.

| Field | Type | Description |
|---|---|---|
| `taxonomyId` | integer | ID of the new taxonomy. |
| `taxonomyGuid` | guid | GUID of the new taxonomy. |
| `taxonomyName` | string | The taxonomy's code name. |
| `taxonomyTitle` | string | Display title of the new taxonomy. |

### CreateWebPageResult

Returned in `data` after a successful [`create-web-page`](../commands/web-pages/create-web-page.md) command.

| Field | Type | Description |
|---|---|---|
| `webPageItemId` | integer | ID of the new page, for the other page commands. |
| `webPageItemGuid` | guid | GUID of the new page. |

### ExecuteSqlResult

Returned in `data` after a successful [`execute-sql`](../commands/sql/execute-sql.md) command: the preview of what would run (`confirmed` is false), or the outcome of running it.

| Field | Type | Description |
|---|---|---|
| `confirmed` | boolean | True when the query was actually executed; false when this is a preview. |
| `statementSummaries` | string[] | One entry per statement describing the verb and tables involved (e.g. "UPDATE [CMS_User]"). Populated in both preview and execute modes. |
| `columns` | string[] | Column names, when execution returned a result set. |
| `rows` | string[][] | Row data, when execution returned a result set. |

### GetContentHubFolderResult

Returned in `data` after a successful [`get-content-hub-folder`](../commands/content-items/get-content-hub-folder.md) command.

| Field | Type | Description |
|---|---|---|
| `contentFolderId` | integer | ID of the folder that was found or created, for [`create-content-item`](../commands/content-items/create-content-item.md) and [`move-content-item`](../commands/content-items/move-content-item.md). |

### GetContentItemUsageResult

Returned in `data` after a successful [`get-content-item-usage`](../commands/content-items/get-content-item-usage.md) command.

| Field | Type | Description |
|---|---|---|
| `items` | [ContentItemUsageEntry](types.md#contentitemusageentry)[] | The content items that reference the item, one entry per language variant. |

### GetFormSubmissionsResult

Returned in `data` after a successful [`get-form-submissions`](../commands/forms-and-contacts/get-form-submissions.md) command.

| Field | Type | Description |
|---|---|---|
| `formClassName` | string | The class name of the form, as it was given. |
| `columns` | string[] | Column names, taken from the first submission, so they are empty when there are no submissions. |
| `rows` | string[][] | One list of values per submission, in `columns` order. Every value is text; null for SQL NULL. |

### GetTagsResult

Returned in `data` after a successful [`get-tags`](../commands/taxonomies/get-tags.md) command.

| Field | Type | Description |
|---|---|---|
| `tags` | [TagResult](types.md#tagresult)[] | The tags that matched. |

### GetTaxonomiesResult

Returned in `data` after a successful [`get-taxonomies`](../commands/taxonomies/get-taxonomies.md) command.

| Field | Type | Description |
|---|---|---|
| `taxonomies` | [TaxonomyResult](types.md#taxonomyresult)[] | All taxonomies. |

### QueryCustomObjectsResult

Returned in `data` after a successful [`query-custom-objects`](../commands/custom-objects/query-custom-objects.md) command.

| Field | Type | Description |
|---|---|---|
| `items` | { name: string }[] | One dictionary per object, mapping column name to its value as text (null for SQL NULL). |

### QueryItemsResult

Returned in `data` after a successful [`query-web-page-items`](../commands/web-pages/query-web-page-items.md) or [`query-reusable-items`](../commands/content-items/query-reusable-items.md) command. Each item is a dictionary of column name to its JSON-serialized value.

| Field | Type | Description |
|---|---|---|
| `items` | { name: any }[] | One entry per item, mapping each requested column to its value. |

### QuerySqlResult

Returned in `data` after a successful [`query-sql`](../commands/sql/query-sql.md) command.

| Field | Type | Description |
|---|---|---|
| `columns` | string[] | Column names of the result set. |
| `rows` | string[][] | One list of values per row, in `columns` order. Every value is text; null for SQL NULL. |

### SearchContentMatch

An item with at least one field that contains the searched text.

| Field | Type | Description |
|---|---|---|
| `contentItemId` | integer | ID of the matching content item. For a web page this is the page's content item ID, not its web page ID. |
| `contentItemName` | string | Code name of the matching item. |
| `contentTypeName` | string | Code name of the item's content type. |
| `location` | string | The page's tree path for web pages; null for reusable content items. |
| `matchedFields` | { name: string } | The fields that contain the text: field name to the field's whole value. |

### SearchContentResult

Returned in `data` after a successful [`search-content`](../commands/content-items/search-content.md) command.

| Field | Type | Description |
|---|---|---|
| `matches` | [SearchContentMatch](types.md#searchcontentmatch)[] | The items that matched. |

### TagResult

A tag, as listed by [`get-tags`](../commands/taxonomies/get-tags.md).

| Field | Type | Description |
|---|---|---|
| `tagId` | integer | ID of the tag. |
| `tagGuid` | guid | GUID of the tag, for putting it in a tag field. |
| `tagName` | string | The tag's code name. |
| `tagTitle` | string | Display title of the tag. |
| `tagDescription` | string | Description of the tag; empty when it has none. |
| `taxonomyId` | integer | ID of the taxonomy the tag belongs to. |
| `parentTagId` | integer | ID of the parent tag, or null for a tag at the root of its taxonomy. |
| `tagOrder` | integer | Position among the tag's siblings. |

### TaxonomyResult

A taxonomy, as listed by [`get-taxonomies`](../commands/taxonomies/get-taxonomies.md).

| Field | Type | Description |
|---|---|---|
| `taxonomyId` | integer | ID of the taxonomy. |
| `taxonomyGuid` | guid | GUID of the taxonomy. |
| `taxonomyName` | string | The taxonomy's code name. |
| `taxonomyTitle` | string | Display title of the taxonomy. |
| `taxonomyDescription` | string | Description of the taxonomy; empty when it has none. |

### WebPageData

A web page's system fields plus its content-type-specific field data. Returned by [`get-page`](../commands/web-pages/get-page.md). `fields` is untyped since its shape depends on the page's content type.

| Field | Type | Description |
|---|---|---|
| `webPageId` | integer | ID of the web page. |
| `webPageGuid` | guid | GUID of the web page. |
| `name` | string | The page's code name. |
| `contentType` | string | Code name of the page's content type, e.g. `DancingGoat.ArticlePage`. |
| `treePath` | string | The page's position in the content tree, e.g. `/Articles/Coffee_Beverages_Explained`. Not the URL. |
| `parentWebPageId` | integer | ID of the parent page, or null for a page at the root of the channel. |
| `languageName` | string | The language that was asked for (the page isn't checked to have a variant in it). |
| `channelName` | string | Code name of the website channel the page belongs to, e.g. `DancingGoatPages`. |
| `fields` | { name: any } | Field values of the page's latest version, by field name, as JSON (text, numbers, dates, ...). Asset, tag and reference fields come as the JSON text Kentico stores in them. Only the content type's own fields are listed: fields that come from reusable field schemas are not included. |

### WebPageInfo

System-level fields describing a web page, without its content-type-specific field data. Returned by [`get-page-info`](../commands/web-pages/get-page-info.md) -- cheap enough to use for existence checks or path resolution (e.g. before sending a [`move-web-page`](../commands/web-pages/move-web-page.md) command).

| Field | Type | Description |
|---|---|---|
| `webPageId` | integer | ID of the web page. |
| `webPageGuid` | guid | GUID of the web page. |
| `name` | string | The page's code name. |
| `contentType` | string | Code name of the page's content type, e.g. `DancingGoat.ArticlePage`. |
| `treePath` | string | The page's position in the content tree, e.g. `/Articles/Coffee_Beverages_Explained`. Not the URL. |
| `parentWebPageId` | integer | ID of the parent page, or null for a page at the root of the channel. |
| `languageName` | string | The language that was asked for (the page isn't checked to have a variant in it). |
| `channelName` | string | Code name of the website channel the page belongs to, e.g. `DancingGoatPages`. |
