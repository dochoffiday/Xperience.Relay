# create-web-page

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `create-web-page` | [Write](../../concepts/access-tiers.md) | [`CreateWebPageResult`](../../reference/types.md#createwebpageresult) | `CreateWebPageCommand` |
<!-- relay:end header -->

Creates a web page under a parent page, in one language, with the field values you give it. The page starts as a draft; set `publishAfterCreate` to publish it straight away.

Everything is checked before anything is created. The content type has to exist, every field has to be a field of that type, and every value has to fit its field, so a request that fails a check creates nothing.

<!-- relay:begin parameters -->
## Parameters

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `websiteChannelName` | string | No | Website channel that owns the page. Defaults to [`RelayKenticoOptions.DefaultWebsiteChannelName`](../../configuration.md#defaultwebsitechannelname) when null. |
| `parentWebPageItemId` | integer | No | ID of the parent web page, or 0 (the default) for the root of the channel. The parent's content type has to allow the new page's content type as a child. |
| `contentTypeName` | string | Yes | Code name of the page's content type, e.g. `DancingGoat.ArticlePage`. |
| `displayName` | string | Yes | Display name of the new page. |
| `languageName` | string | No | Defaults to [`RelayKenticoOptions.DefaultLanguageName`](../../configuration.md#defaultlanguagename) when null. |
| `urlSlug` | string | No | Optional URL slug. When null, Kentico generates one from the display name. |
| `fields` | { name: any } | No | Plain field values by field name (text, numbers, booleans, dates, ...). |
| `linkedItemFields` | { name: guid[] } | No | Linked item fields -- field name to list of content item GUIDs. The field must be a content item reference field. |
| `tagFields` | { name: guid[] } | No | Tag fields -- field name to list of tag GUIDs. The field must be a tag (taxonomy) field. |
| `publishAfterCreate` | boolean | No | When true, publishes the page immediately after creation. Defaults to false. |
| `assets` | [RelayAsset](../../reference/types.md#relayasset)[] | No | Optional binary assets to upload into asset fields. Each entry maps to one field. |
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns a [CreateWebPageResult](../../reference/types.md#createwebpageresult) in `data`:

| Field | Type | Description |
|---|---|---|
| `webPageItemId` | integer | ID of the new page, for the other page commands. |
| `webPageItemGuid` | guid | GUID of the new page. |
<!-- relay:end result -->

## Behavior

- **State.** The page is created as an `InitialDraft`, which is not on the live site yet. With `publishAfterCreate` set to `true` it is published right after it is created. If Kentico creates the page but then declines to publish it, the result is a failure that names the new page's ID, so a retry doesn't create a second page. See [Drafts and publishing](../../concepts/drafts-and-publishing.md).
- **Channel.** `websiteChannelName` is the channel's code name, not its display name. When it is left out Relay uses [`DefaultWebsiteChannelName`](../../configuration.md#defaultwebsitechannelname), and the command fails if that isn't set either.
- **Parent.** `parentWebPageItemId` is the ID of the parent page; find it with [`get-page-info`](get-page-info.md) or [`query-web-page-items`](query-web-page-items.md). `0`, the default, creates the page at the root of the channel. Kentico decides what can go where: the parent's content type has to allow the new page's content type as a child, otherwise Kentico refuses with "Content type with ID ... is not allowed as a child of web page with ID ...".
- **URL slug.** When `urlSlug` is left out Kentico makes one from the display name. Change it later with [`update-slug`](update-slug.md).
- **Fields.** `fields` takes plain values: text, numbers, booleans and dates. Field names are not case sensitive. A field the content type doesn't have is refused (Kentico would drop it without saying so), and a value has to fit its field: a whole number for an integer field, a date that parses for a date field, text no longer than the field allows, and so on. Every problem found is reported in one answer. Fields that come from a reusable field schema count as fields of the type.
- **Linked items, tags and files.** `linkedItemFields` takes content item GUIDs for content item reference fields, `tagFields` takes tag GUIDs for tag fields, and `assets` takes Base64 files for asset fields. Each one only goes into a field of that kind. See [Upload files](../../guides/uploading-assets.md).
- **Language.** The page is created in `languageName`, or in [`DefaultLanguageName`](../../configuration.md#defaultlanguagename). Relay has no command that adds another language to an existing page.
- **The result.** `webPageItemId` is the ID that the other page commands call `webPageId`.

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Create an article as a draft

The parent (`parentWebPageItemId` 1) is the Articles section of the Dancing Goat sample site. The page starts as an `InitialDraft`.

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "create-web-page",
      "parameters": {
        "websiteChannelName": "DancingGoatPages",
        "parentWebPageItemId": 1,
        "contentTypeName": "DancingGoat.ArticlePage",
        "displayName": "Cold brew at home",
        "fields": {
          "ArticleTitle": "Cold brew at home",
          "ArticlePageSummary": "Steep coarse grounds in cold water overnight for a smooth, low-acid coffee.",
          "ArticlePageText": "<p>Mix 100 g of coarse grounds with 1 litre of cold water and leave it for 12 hours.</p>",
          "ArticlePagePublishDate": "2026-10-01"
        }
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new CreateWebPageCommand
    {
        WebsiteChannelName = "DancingGoatPages",
        ParentWebPageItemId = 1,
        ContentTypeName = "DancingGoat.ArticlePage",
        DisplayName = "Cold brew at home",
        Fields = new()
        {
            ["ArticleTitle"] = JsonSerializer.SerializeToElement("Cold brew at home"),
            ["ArticlePageSummary"] = JsonSerializer.SerializeToElement("Steep coarse grounds in cold water overnight for a smooth, low-acid coffee."),
            ["ArticlePageText"] = JsonSerializer.SerializeToElement("<p>Mix 100 g of coarse grounds with 1 litre of cold water and leave it for 12 hours.</p>"),
            ["ArticlePagePublishDate"] = JsonSerializer.SerializeToElement("2026-10-01"),
        },
    });
    var data = result.GetData<CreateWebPageResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Created web page 'Cold brew at home' (ID=68), draft.",
  "error": null,
  "data": {
    "webPageItemId": 68,
    "webPageItemGuid": "5b308d07-cbb6-4ccb-8d85-6068cc689e17"
  }
}
```

### Create a page, set its URL slug and publish it straight away

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "create-web-page",
      "parameters": {
        "websiteChannelName": "DancingGoatPages",
        "parentWebPageItemId": 1,
        "contentTypeName": "DancingGoat.ArticlePage",
        "displayName": "Brewing methods compared",
        "urlSlug": "brewing-methods-compared",
        "publishAfterCreate": true,
        "fields": {
          "ArticleTitle": "Brewing methods compared",
          "ArticlePageSummary": "Pour-over, French press or AeroPress? What each one does to the cup."
        }
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new CreateWebPageCommand
    {
        WebsiteChannelName = "DancingGoatPages",
        ParentWebPageItemId = 1,
        ContentTypeName = "DancingGoat.ArticlePage",
        DisplayName = "Brewing methods compared",
        UrlSlug = "brewing-methods-compared",
        PublishAfterCreate = true,
        Fields = new()
        {
            ["ArticleTitle"] = JsonSerializer.SerializeToElement("Brewing methods compared"),
            ["ArticlePageSummary"] = JsonSerializer.SerializeToElement("Pour-over, French press or AeroPress? What each one does to the cup."),
        },
    });
    var data = result.GetData<CreateWebPageResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Created web page 'Brewing methods compared' (ID=69), published.",
  "error": null,
  "data": {
    "webPageItemId": 69,
    "webPageItemGuid": "41dca4c0-2931-4949-b905-c5def7b31a8c"
  }
}
```
<!-- relay:end examples -->

<!-- relay:begin errors -->
## Errors

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "A content type that is not allowed under the parent"

    The root of the channel only allows the content types the channel permits there; an article page is not one of them.

    Parameters:

    ```json
    {
      "websiteChannelName": "DancingGoatPages",
      "parentWebPageItemId": 0,
      "contentTypeName": "DancingGoat.ArticlePage",
      "displayName": "Cold brew at the root"
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Unhandled exception executing verb 'create-web-page': [InvalidOperationException] Content type with ID 5665 is not allowed as a child of web page with ID 0.",
      "data": null
    }
    ```

???+ failure "Fields the content type does not have, or values that do not fit"

    Parameters:

    ```json
    {
      "websiteChannelName": "DancingGoatPages",
      "parentWebPageItemId": 1,
      "contentTypeName": "DancingGoat.ArticlePage",
      "displayName": "Cold brew at home",
      "fields": {
        "ArticleSubtitle": "A field this type does not have",
        "ArticlePagePublishDate": "last Tuesday"
      }
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Field 'ArticleSubtitle' was not found on content type 'DancingGoat.ArticlePage'. Field 'ArticlePagePublishDate' expects a date, but got 'last Tuesday'.",
      "data": null
    }
    ```

???+ failure "No website channel named, and no default configured"

    Parameters:

    ```json
    {
      "parentWebPageItemId": 1,
      "contentTypeName": "DancingGoat.ArticlePage",
      "displayName": "Cold brew at home"
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "WebsiteChannelName is required. Set it on the command or configure RelayKenticoOptions.DefaultWebsiteChannelName.",
      "data": null
    }
    ```
<!-- relay:end errors -->

## See also

- [`update-web-page`](update-web-page.md) to change the fields later
- [`publish-web-page`](publish-web-page.md) to publish a draft
- [`get-page-info`](get-page-info.md) to look up the parent's ID
- [Create, edit and publish a page](../../guides/page-lifecycle.md)
