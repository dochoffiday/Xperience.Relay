# query-reusable-items

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `query-reusable-items` | [Read](../../concepts/access-tiers.md) | [`QueryItemsResult`](../../reference/types.md#queryitemsresult) | `QueryReusableItemsCommand` |
<!-- relay:end header -->

Lists reusable content items of one or more content types and returns only the columns you name. It is the command for finding items: by type, by the value of a field, or by publication status.

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
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns a [QueryItemsResult](../../reference/types.md#queryitemsresult) in `data`:

| Field | Type | Description |
|---|---|---|
| `items` | { name: any }[] | One entry per item, mapping each requested column to its value. |
<!-- relay:end result -->

## Behavior

- **Columns.** Name the columns you want, at least one: the content type's field names, plus system columns such as `ContentItemID`, `ContentItemGUID` and `ContentItemName`. Each item in the answer is an object with just those columns. Reference and tag fields come back as JSON values (arrays of objects), not as strings.
- **Filtering.** `whereEquals` keeps the items whose columns equal the values given; the conditions are combined with AND. A column used in `whereEquals` must also be listed in `columns`, otherwise the query fails with "Invalid column name". Values are strings, numbers or booleans.
- **Which version.** By default each item is its latest version, drafts included. With `publishedOnly: true` it is the published version of each item, and items that have none are left out. To find out whether an item is published, run the query twice, once with `publishedOnly`, and compare the `ContentItemID`s.
- **Several types.** `contentTypeNames` can list more than one content type; the answer is the union of their items.
- **Language.** Items come from `languageName`, with Kentico's language fallback.
- **No paging.** The answer holds every matching item, so narrow the query with `whereEquals` when a type has many items.

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### List the cafes

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "query-reusable-items",
      "parameters": {
        "contentTypeNames": [
          "DancingGoat.Cafe"
        ],
        "columns": [
          "ContentItemID",
          "CafeName",
          "CafeCity"
        ]
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new QueryReusableItemsCommand
    {
        ContentTypeNames = ["DancingGoat.Cafe"],
        Columns = ["ContentItemID", "CafeName", "CafeCity"],
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
        "ContentItemID": 19,
        "CafeName": "Boston coffee place",
        "CafeCity": "Boston"
      },
      {
        "ContentItemID": 30,
        "CafeName": "Chicago",
        "CafeCity": "Chicago"
      },
      {
        "ContentItemID": 81,
        "CafeName": "New York",
        "CafeCity": "New York"
      },
      {
        "ContentItemID": 185,
        "CafeName": "Seattle Coffee Place",
        "CafeCity": "Seattle"
      }
    ]
  }
}
```

### Filter by a column value

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "query-reusable-items",
      "parameters": {
        "contentTypeNames": [
          "DancingGoat.Cafe"
        ],
        "columns": [
          "ContentItemID",
          "CafeName",
          "CafeCity"
        ],
        "whereEquals": {
          "CafeCity": "Seattle"
        }
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new QueryReusableItemsCommand
    {
        ContentTypeNames = ["DancingGoat.Cafe"],
        Columns = ["ContentItemID", "CafeName", "CafeCity"],
        WhereEquals = new()
        {
            ["CafeCity"] = JsonSerializer.SerializeToElement("Seattle"),
        },
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
        "ContentItemID": 185,
        "CafeName": "Seattle Coffee Place",
        "CafeCity": "Seattle"
      }
    ]
  }
}
```
<!-- relay:end examples -->

<!-- relay:begin errors -->
## Errors

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "No content types named"

    Parameters:

    ```json
    {
      "columns": [
        "ContentItemID"
      ]
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "ContentTypeNames must list at least one content type.",
      "data": null
    }
    ```
<!-- relay:end errors -->

## See also

- [`query-web-page-items`](../web-pages/query-web-page-items.md) for pages
- [`get-content`](get-content.md) for one item
- [`search-content`](search-content.md) to search text
- [Query content](../../guides/querying.md)
