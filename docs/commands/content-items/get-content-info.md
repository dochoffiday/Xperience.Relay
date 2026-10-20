# get-content-info

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `get-content-info` | [Read](../../concepts/access-tiers.md) | [`ContentInfo`](../../reference/types.md#contentinfo) | `GetContentInfoCommand` |
<!-- relay:end header -->

Looks up a reusable content item's system fields: its code name, content type and workspace. It returns no field values, so it is the cheap way to check that an item exists.

<!-- relay:begin parameters -->
## Parameters

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `contentItemId` | integer | Yes | ID of the reusable content item. |
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns a [ContentInfo](../../reference/types.md#contentinfo) in `data`:

| Field | Type | Description |
|---|---|---|
| `contentItemId` | integer | ID of the content item. |
| `contentItemGuid` | guid | GUID of the content item. |
| `name` | string | The item's code name. |
| `contentType` | string | Code name of the item's content type, e.g. `DancingGoat.Image`. |
| `languageName` | string | Empty from [`get-content-info`](get-content-info.md) (the item's system fields don't vary by language); the requested language from [`get-content`](get-content.md). |
| `workspaceName` | string | The workspace's numeric ID, as text. Kentico's public API doesn't expose a workspace's name. |
| `contentFolderPath` | string | Reserved. Not filled in yet, so currently always null. |
<!-- relay:end result -->

## Behavior

- **No language.** `languageName` is empty in the answer, because the system fields don't vary by language. [`get-content`](get-content.md) reports the language you asked for.
- **Workspace.** `workspaceName` holds the workspace's numeric ID as text (for example `"2"`), because Kentico's public API doesn't expose a workspace's name.
- **Folder.** `contentFolderPath` is not filled in yet and is always `null`.
- **Not found.** An item that doesn't exist fails with "Content item N was not found."

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Look up a content item

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "get-content-info",
      "parameters": {
        "contentItemId": 184
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new GetContentInfoCommand
    {
        ContentItemId = 184,
    });
    var data = result.GetData<ContentInfo>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": null,
  "error": null,
  "data": {
    "contentItemId": 184,
    "contentItemGuid": "a244387c-f5fc-4fe2-861c-d33832e7b90d",
    "name": "ColdBrewWorkshop-23s43e2s",
    "contentType": "DancingGoat.Event",
    "languageName": "",
    "workspaceName": "2",
    "contentFolderPath": null
  }
}
```
<!-- relay:end examples -->

<!-- relay:begin errors -->
## Errors

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "An item that does not exist"

    Parameters:

    ```json
    {
      "contentItemId": 99999
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
<!-- relay:end errors -->

## See also

- [`get-content`](get-content.md) for the field values as well
- [`query-reusable-items`](query-reusable-items.md) to find items
