# move-web-page

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `move-web-page` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `MoveWebPageCommand` |
<!-- relay:end header -->

Moves a page, together with the pages below it, under a different parent, or to the root of the channel.

<!-- relay:begin parameters -->
## Parameters

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `webPageId` | integer | Yes | ID of the page to move. |
| `parentWebPageId` | integer | Yes | ID of the new parent page, or 0 for the root of the channel. |
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result -->

## Behavior

- **The parent has to exist.** `parentWebPageId` is the ID of the new parent page, or `0` for the root of the channel. A parent that doesn't exist fails with "Web page N (the new parent) was not found." Find IDs with [`get-page-info`](get-page-info.md) or [`query-web-page-items`](query-web-page-items.md).
- **Kentico decides what may go where.** The parent's content type has to allow the page's content type as a child; otherwise Kentico refuses with "Content type with ID ... is not allowed as a child of web page with ID ...".
- **Position.** Relay doesn't set a sibling order. Kentico puts the moved page first among its new siblings.
- **Immediate.** A move takes effect at once; there is no draft to publish. [`get-page-info`](get-page-info.md) shows the new `treePath` and `parentWebPageId`.

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Move a navigation item out of the Store menu

Page 25 is `/Navigation_menu/Store/Merchandise` and page 5 is `/Navigation_menu`. Kentico puts the moved page first among its new siblings.

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "move-web-page",
      "parameters": {
        "webPageId": 25,
        "parentWebPageId": 5
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new MoveWebPageCommand
    {
        WebPageId = 25,
        ParentWebPageId = 5,
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Moved web page 25 under parent 5.",
  "error": null,
  "data": null
}
```
<!-- relay:end examples -->

<!-- relay:begin errors -->
## Errors

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "A parent that does not allow the page's content type"

    Parameters:

    ```json
    {
      "webPageId": 68,
      "parentWebPageId": 69
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Unhandled exception executing verb 'move-web-page': [InvalidOperationException] Content type with ID 5665 is not allowed as a child of web page with ID 69.",
      "data": null
    }
    ```

???+ failure "A parent that does not exist"

    Parameters:

    ```json
    {
      "webPageId": 68,
      "parentWebPageId": 99999
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Web page 99999 (the new parent) was not found.",
      "data": null
    }
    ```
<!-- relay:end errors -->

## See also

- [`get-page-info`](get-page-info.md) to look up IDs and the tree path
- [`create-web-page`](create-web-page.md)
- [`delete-web-page`](delete-web-page.md)
