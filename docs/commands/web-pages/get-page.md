# get-page

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `get-page` | [Read](../../concepts/access-tiers.md) | [`WebPageData`](../../reference/types.md#webpagedata) | `GetPageCommand` |
<!-- relay:end header -->

Reads a page's system fields, the same ones [`get-page-info`](get-page-info.md) returns, together with the field values of its latest version. The latest version includes drafts, so you see what editors see in the admin UI, not what the live site shows.

<!-- relay:begin parameters -->
## Parameters

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `webPageId` | integer | Yes | ID of the web page. |
| `languageName` | string | No | Defaults to [`RelayKenticoOptions.DefaultLanguageName`](../../configuration.md#defaultlanguagename) when null. |
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns a [WebPageData](../../reference/types.md#webpagedata) in `data`:

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
<!-- relay:end result -->

## Behavior

- **Field values.** `fields` is keyed by the content type's own field names. Text is a string, numbers and booleans are JSON values, dates are ISO strings without a time zone (`"2026-10-01T00:00:00"`), and a value that isn't set is `null`. Reference, tag and asset fields come as the JSON text Kentico stores in them: a string that contains JSON, for example `"[{\"Identifier\":\"b8117d96-...\"}]"`. Fields that come from reusable field schemas aren't included.
- **Published version.** To read what is live, use [`query-web-page-items`](query-web-page-items.md) with `publishedOnly`.
- **Language.** `languageName` defaults to [`DefaultLanguageName`](../../configuration.md#defaultlanguagename), and the answer reports the language you asked for. A page that has no variant in it comes back with the values of the language Kentico falls back to.

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Read a page with its field values

The page is still an `InitialDraft`, and `get-page` returns its latest version, so the fields are the draft values.

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "get-page",
      "parameters": {
        "webPageId": 68,
        "languageName": "en"
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new GetPageCommand
    {
        WebPageId = 68,
        LanguageName = "en",
    });
    var data = result.GetData<WebPageData>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": null,
  "error": null,
  "data": {
    "fields": {
      "ArticleTitle": "Cold brew at home",
      "ArticlePagePublishDate": "2026-10-01T00:00:00",
      "ArticlePageTeaser": null,
      "ArticlePageSummary": "Steep coarse grounds in cold water overnight for a smooth, low-acid coffee.",
      "ArticlePageText": "<p>Mix 100 g of coarse grounds with 1 litre of cold water and leave it for 12 hours.</p>",
      "ArticleRelatedPages": null
    },
    "webPageId": 68,
    "webPageGuid": "5b308d07-cbb6-4ccb-8d85-6068cc689e17",
    "name": "ColdBrewAtHome-aqfgqi7s",
    "contentType": "DancingGoat.ArticlePage",
    "treePath": "/Articles/Cold_brew_at_home",
    "parentWebPageId": 1,
    "languageName": "en",
    "channelName": "DancingGoatPages"
  }
}
```
<!-- relay:end examples -->

<!-- relay:begin errors -->
<!-- relay:end errors -->

## See also

- [`get-page-info`](get-page-info.md) when you don't need the fields
- [`update-web-page`](update-web-page.md) to change them
- [`query-web-page-items`](query-web-page-items.md) to read many pages at once
