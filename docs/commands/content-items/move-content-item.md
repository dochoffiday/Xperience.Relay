# move-content-item

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `move-content-item` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `MoveContentItemCommand` |
<!-- relay:end header -->

Moves one or more reusable content items into a content hub folder.

<!-- relay:begin parameters -->
## Parameters

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `contentItemIds` | integer[] | Yes | IDs of the content items to move. At least one. |
| `contentFolderId` | integer | Yes | ID of the destination content hub folder. |
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result -->

## Behavior

- **The folder.** `contentFolderId` is the destination folder's ID; [`get-content-hub-folder`](get-content-hub-folder.md) finds or creates it. A folder that doesn't exist fails with Kentico's "The content folder with ID ... does not exist."
- **The items.** Every ID has to be a reusable content item. An ID that doesn't exist, or that belongs to a web page, fails with a message that says so, and nothing is moved.
- **Many at once.** All of `contentItemIds` are moved in one call.

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Move an item to another folder

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "move-content-item",
      "parameters": {
        "contentItemIds": [
          184
        ],
        "contentFolderId": 6
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new MoveContentItemCommand
    {
        ContentItemIds = [184],
        ContentFolderId = 6,
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Moved 1 content item(s) to folder 6.",
  "error": null,
  "data": null
}
```
<!-- relay:end examples -->

<!-- relay:begin errors -->
## Errors

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "No items named"

    Parameters:

    ```json
    {
      "contentItemIds": [],
      "contentFolderId": 6
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "ContentItemIds must not be empty.",
      "data": null
    }
    ```

???+ failure "An item that does not exist"

    Parameters:

    ```json
    {
      "contentItemIds": [
        99999
      ],
      "contentFolderId": 6
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Content item 99999 was not found.",
      "data": null
    }
    ```

???+ failure "The content item behind a web page"

    Content item 132 belongs to the page "Coffee Beverages Explained". Pages are moved with `move-web-page`.

    Parameters:

    ```json
    {
      "contentItemIds": [
        132
      ],
      "contentFolderId": 6
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Content item 132 is not a reusable content item. Use move-web-page for web pages.",
      "data": null
    }
    ```

???+ failure "A folder that does not exist"

    Parameters:

    ```json
    {
      "contentItemIds": [
        184
      ],
      "contentFolderId": 99999
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Unhandled exception executing verb 'move-content-item': [ContentFolderNotFoundException] The content folder with ID 99999 does not exist.",
      "data": null
    }
    ```
<!-- relay:end errors -->

## See also

- [`get-content-hub-folder`](get-content-hub-folder.md)
- [`create-content-item`](create-content-item.md), which can place an item in a folder as it creates it
