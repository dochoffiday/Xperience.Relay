# search-content

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `search-content` | [Read](../../concepts/access-tiers.md) | [`SearchContentResult`](../../reference/types.md#searchcontentresult) | `SearchContentCommand` |
<!-- relay:end header -->

Searches the published items of one content type for some text, without regard to case, across every text, long text and rich text field. It works for reusable content types and for page types.

<!-- relay:begin parameters -->
## Parameters

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `contentTypeName` | string | Yes | Code name of the content type to search, e.g. `DancingGoat.ArticlePage`. |
| `filter` | string | Yes | The text to look for. |
| `languageName` | string | No | Defaults to [`RelayKenticoOptions.DefaultLanguageName`](../../configuration.md#defaultlanguagename) when null. |
| `websiteChannelName` | string | No | Website channel to search, for web page content types. Defaults to [`RelayKenticoOptions.DefaultWebsiteChannelName`](../../configuration.md#defaultwebsitechannelname) when null. Not used for reusable content types. |
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns a [SearchContentResult](../../reference/types.md#searchcontentresult) in `data`:

| Field | Type | Description |
|---|---|---|
| `matches` | [SearchContentMatch](../../reference/types.md#searchcontentmatch)[] | The items that matched. |
<!-- relay:end result -->

## Behavior

- **What is searched.** Only published items, and only fields of the types `Text`, `LongText` and `RichTextHTML`. The text is matched anywhere in a field; for a rich text field that includes its HTML.
- **What comes back.** One entry per item with at least one matching field. `matchedFields` has the whole value of each field that matched, so it can be long for rich text. `location` is the page's tree path for a page, and `null` for a reusable item. `contentItemId` is a content item ID, also for pages.
- **Pages.** For a page type, `websiteChannelName` says which channel to search (it defaults to [`DefaultWebsiteChannelName`](../../configuration.md#defaultwebsitechannelname)), and the command fails when neither is set. It isn't used for reusable types.
- **No matches.** That is a success with an empty list. A content type without any searchable text field also succeeds, with a message that says so.
- **Cost.** The text is matched inside Relay after Kentico returns the published items, so avoid it on a content type with a very large number of items.
- **Language.** `languageName` defaults to [`DefaultLanguageName`](../../configuration.md#defaultlanguagename).

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Search the published article pages for some text

The search covers published items only and every text, long text and rich text field. `location` is the page's tree path.

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "search-content",
      "parameters": {
        "contentTypeName": "DancingGoat.ArticlePage",
        "websiteChannelName": "DancingGoatPages",
        "filter": "cold brew"
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new SearchContentCommand
    {
        ContentTypeName = "DancingGoat.ArticlePage",
        WebsiteChannelName = "DancingGoatPages",
        Filter = "cold brew",
    });
    var data = result.GetData<SearchContentResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Found 1 matching item(s) in 'DancingGoat.ArticlePage'.",
  "error": null,
  "data": {
    "matches": [
      {
        "contentItemId": 182,
        "contentItemName": "ColdBrewAtHome-aqfgqi7s",
        "contentTypeName": "DancingGoat.ArticlePage",
        "location": "/Articles/Cold_brew_at_home",
        "matchedFields": {
          "ArticleTitle": "Cold brew at home: a beginner's guide",
          "ArticlePageSummary": "Everything you need to make cold brew with a jar and a sieve - no special equipment."
        }
      }
    ]
  }
}
```

### Search the published cafes for some text

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "search-content",
      "parameters": {
        "contentTypeName": "DancingGoat.Cafe",
        "filter": "seattle"
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new SearchContentCommand
    {
        ContentTypeName = "DancingGoat.Cafe",
        Filter = "seattle",
    });
    var data = result.GetData<SearchContentResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Found 1 matching item(s) in 'DancingGoat.Cafe'.",
  "error": null,
  "data": {
    "matches": [
      {
        "contentItemId": 185,
        "contentItemName": "SeattleCoffeePlace-0fxj79os",
        "contentTypeName": "DancingGoat.Cafe",
        "location": null,
        "matchedFields": {
          "CafeName": "Seattle Coffee Place",
          "CafeCity": "Seattle"
        }
      }
    ]
  }
}
```
<!-- relay:end examples -->

<!-- relay:begin errors -->
## Errors

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "No text to look for"

    Parameters:

    ```json
    {
      "contentTypeName": "DancingGoat.Cafe"
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Filter is required.",
      "data": null
    }
    ```
<!-- relay:end errors -->

## See also

- [`query-web-page-items`](../web-pages/query-web-page-items.md) and [`query-reusable-items`](query-reusable-items.md) to filter by column values
- [`get-page`](../web-pages/get-page.md) and [`get-content`](get-content.md) to read a match in full
