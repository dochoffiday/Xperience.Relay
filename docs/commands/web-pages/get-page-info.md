# get-page-info

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `get-page-info` | [Read](../../concepts/access-tiers.md) | [`WebPageInfo`](../../reference/types.md#webpageinfo) | `GetPageInfoCommand` |
<!-- relay:end header -->

Looks up a page's system fields: its content type, its place in the content tree, its parent and its channel. It doesn't return field values, so it is the cheap way to check that a page exists, or to turn a page into the IDs and paths other commands need.

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

A successful run returns a [WebPageInfo](../../reference/types.md#webpageinfo) in `data`:

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
<!-- relay:end result -->

## Behavior

- **Tree path, not URL.** `treePath` is the page's position in the content tree (for example `/Articles/Coffee_Beverages_Explained`). For URLs use [`query-web-page-items`](query-web-page-items.md) with `includeUrls`.
- **Parent.** `parentWebPageId` is `null` for a page at the root of the channel.
- **Language.** `languageName` in the answer is the language you asked for. The lookup doesn't check that the page has a variant in it.
- **Not found.** A page that doesn't exist fails with "Web page N was not found."

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Look up a page

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "get-page-info",
      "parameters": {
        "webPageId": 68
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new GetPageInfoCommand
    {
        WebPageId = 68,
    });
    var data = result.GetData<WebPageInfo>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": null,
  "error": null,
  "data": {
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
## Errors

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "A page that does not exist"

    Parameters:

    ```json
    {
      "webPageId": 99999
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Web page 99999 was not found.",
      "data": null
    }
    ```
<!-- relay:end errors -->

## See also

- [`get-page`](get-page.md) for the field values as well
- [`query-web-page-items`](query-web-page-items.md) to find pages
- [`move-web-page`](move-web-page.md) and [`create-web-page`](create-web-page.md) take IDs from here
