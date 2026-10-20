# query-web-page-items

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `query-web-page-items` | [Read](../../concepts/access-tiers.md) | [`QueryItemsResult`](../../reference/types.md#queryitemsresult) | `QueryWebPageItemsCommand` |
<!-- relay:end header -->

Lists the pages of one or more content types in a channel and returns only the columns you name. It is the command for finding pages: by type, by the value of a field, by publication status, or together with their URLs.

<!-- relay:begin parameters -->
## Parameters

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `contentTypeNames` | string[] | Yes | Code names of the content types to include, e.g. `DancingGoat.ArticlePage`. At least one. |
| `languageName` | string | No | Defaults to [`RelayKenticoOptions.DefaultLanguageName`](../../configuration.md#defaultlanguagename) when null. |
| `columns` | string[] | Yes | Columns to fetch and return. At least one. |
| `whereEquals` | { name: any } | No | Optional column-equality filters. All conditions are ANDed together. Each column must also be listed in `columns`. |
| `publishedOnly` | boolean | No | When true, returns the published version of each item, and leaves out items that have none. Defaults to false, which returns each item's latest version, drafts included. |
| `websiteChannelName` | string | No | Code name of the website channel to query. Defaults to [`RelayKenticoOptions.DefaultWebsiteChannelName`](../../configuration.md#defaultwebsitechannelname) when null; the command fails if neither is set. |
| `includeUrls` | boolean | No | When true, each item also gets `WebPageRelativeUrl` and `WebPageAbsoluteUrl` (empty strings for pages with no URL path). Defaults to false. |
| `includeStatus` | boolean | No | When true, each item also gets `WebPageStatus` -- the status of its latest version: InitialDraft, Draft, Published or Unpublished -- and `WebPageIsPublished`: whether a published version exists. That is true for Published and for Draft, which is a change on top of a published version (a page that was never published is an InitialDraft), so a live page with unpublished changes has status Draft and is still published. The query still returns every page, drafts included. Defaults to false. |
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns a [QueryItemsResult](../../reference/types.md#queryitemsresult) in `data`:

| Field | Type | Description |
|---|---|---|
| `items` | { name: any }[] | One entry per item, mapping each requested column to its value. |
<!-- relay:end result -->

## Behavior

- **Columns.** Name the columns you want, at least one: the content type's field names, plus system columns such as `WebPageItemID`, `WebPageItemGUID` and `WebPageUrlPath`. Each item in the answer is an object with just those columns. Reference and tag fields come back as JSON values (arrays of objects), not as strings.
- **Filtering.** `whereEquals` keeps the pages whose columns equal the values given; the conditions are combined with AND. A column used in `whereEquals` must also be listed in `columns`, otherwise the query fails with "Invalid column name". Values are strings, numbers or booleans.
- **Which version.** By default each page is its latest version, drafts included. With `publishedOnly: true` it is the published version of each page, field values included, and pages that have no published version are left out.
- **Several types.** `contentTypeNames` can list more than one content type; the answer is the union of their pages.
- **`includeStatus`.** Adds `WebPageStatus`, the status of the page's latest version (`InitialDraft`, `Draft`, `Published` or `Unpublished`), and `WebPageIsPublished`, which says whether a published version exists. That is `true` for `Published` and for `Draft` (a draft is a change on top of a published version) and `false` for `InitialDraft` and `Unpublished`. So a live page with unpublished changes has the status `Draft` and is still published. Both come from the same query, so they cost nothing extra.
- **`includeUrls`.** Adds `WebPageRelativeUrl` (`~/articles/cold-brew-guide`) and `WebPageAbsoluteUrl` (with the channel's domain), formatted the way the site serves them, which means lower case, so they can differ in case from `WebPageUrlPath`. Both are empty strings for a page with no URL path. They are the latest URLs: a published page with an unpublished URL change shows its new URL, unless `publishedOnly` is set. A page served through language fallback gets its fallback language's URL.
- **Language.** Pages come from `languageName`, with Kentico's language fallback. The status and URL describe the language variant a row comes from.
- **No paging.** The answer holds every matching page, so narrow the query with `whereEquals` when a type has many pages.
- **Channel.** `websiteChannelName` is the channel's code name; it defaults to [`DefaultWebsiteChannelName`](../../configuration.md#defaultwebsitechannelname), and the command fails if neither is set.

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### List the article pages and their titles

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "query-web-page-items",
      "parameters": {
        "websiteChannelName": "DancingGoatPages",
        "contentTypeNames": [
          "DancingGoat.ArticlePage"
        ],
        "columns": [
          "WebPageItemID",
          "ArticleTitle"
        ]
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new QueryWebPageItemsCommand
    {
        WebsiteChannelName = "DancingGoatPages",
        ContentTypeNames = ["DancingGoat.ArticlePage"],
        Columns = ["WebPageItemID", "ArticleTitle"],
    });
    var data = result.GetData<QueryItemsResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": null,
  "error": null,
  "data": {
    "items": [
      {
        "WebPageItemID": 9,
        "ArticleTitle": "Coffee Beverages Explained"
      },
      {
        "WebPageItemID": 10,
        "ArticleTitle": "Coffee processing techniques"
      },
      {
        "WebPageItemID": 11,
        "ArticleTitle": "Donate with us"
      },
      {
        "WebPageItemID": 12,
        "ArticleTitle": "On Roasts"
      },
      {
        "WebPageItemID": 13,
        "ArticleTitle": "Origins of Arabica Bourbon"
      },
      {
        "WebPageItemID": 14,
        "ArticleTitle": "The Resilient Robusta: A Coffee with Character"
      },
      {
        "WebPageItemID": 15,
        "ArticleTitle": "Which brewing fits you?"
      },
      {
        "WebPageItemID": 68,
        "ArticleTitle": "Cold brew at home: a beginner's guide"
      },
      {
        "WebPageItemID": 69,
        "ArticleTitle": "Brewing methods compared"
      }
    ]
  }
}
```

### Add each page's URL and publication status

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "query-web-page-items",
      "parameters": {
        "websiteChannelName": "DancingGoatPages",
        "contentTypeNames": [
          "DancingGoat.ArticlePage"
        ],
        "columns": [
          "WebPageItemID",
          "ArticleTitle"
        ],
        "whereEquals": {
          "WebPageItemID": 68
        },
        "includeUrls": true,
        "includeStatus": true
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new QueryWebPageItemsCommand
    {
        WebsiteChannelName = "DancingGoatPages",
        ContentTypeNames = ["DancingGoat.ArticlePage"],
        Columns = ["WebPageItemID", "ArticleTitle"],
        WhereEquals = new()
        {
            ["WebPageItemID"] = JsonSerializer.SerializeToElement(68),
        },
        IncludeUrls = true,
        IncludeStatus = true,
    });
    var data = result.GetData<QueryItemsResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": null,
  "error": null,
  "data": {
    "items": [
      {
        "WebPageItemID": 68,
        "ArticleTitle": "Cold brew at home: a beginner's guide",
        "WebPageStatus": "Published",
        "WebPageIsPublished": true,
        "WebPageRelativeUrl": "~/articles/cold-brew-guide",
        "WebPageAbsoluteUrl": "https://localhost/articles/cold-brew-guide"
      }
    ]
  }
}
```

### Only the published version of each page

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "query-web-page-items",
      "parameters": {
        "websiteChannelName": "DancingGoatPages",
        "contentTypeNames": [
          "DancingGoat.ArticlePage"
        ],
        "columns": [
          "WebPageItemID",
          "ArticleTitle"
        ],
        "whereEquals": {
          "WebPageItemID": 68
        },
        "publishedOnly": true
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new QueryWebPageItemsCommand
    {
        WebsiteChannelName = "DancingGoatPages",
        ContentTypeNames = ["DancingGoat.ArticlePage"],
        Columns = ["WebPageItemID", "ArticleTitle"],
        WhereEquals = new()
        {
            ["WebPageItemID"] = JsonSerializer.SerializeToElement(68),
        },
        PublishedOnly = true,
    });
    var data = result.GetData<QueryItemsResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": null,
  "error": null,
  "data": {
    "items": [
      {
        "WebPageItemID": 68,
        "ArticleTitle": "Cold brew at home: a beginner's guide"
      }
    ]
  }
}
```
<!-- relay:end examples -->

<!-- relay:begin errors -->
## Errors

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "No columns named"

    Parameters:

    ```json
    {
      "websiteChannelName": "DancingGoatPages",
      "contentTypeNames": [
        "DancingGoat.ArticlePage"
      ]
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Columns must list at least one column.",
      "data": null
    }
    ```
<!-- relay:end errors -->

## See also

- [`get-page-info`](get-page-info.md) and [`get-page`](get-page.md) for one page
- [`query-reusable-items`](../content-items/query-reusable-items.md) for reusable content
- [`search-content`](../content-items/search-content.md) to search text
- [Query content](../../guides/querying.md)
