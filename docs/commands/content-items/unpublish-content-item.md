# unpublish-content-item

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `unpublish-content-item` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `UnpublishContentItemCommand` |
<!-- relay:end header -->

Takes a published reusable content item in one language offline.

<!-- relay:begin parameters -->
## Parameters

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `contentItemId` | integer | Yes | ID of the reusable content item. |
| `languageName` | string | No | Defaults to [`RelayKenticoOptions.DefaultLanguageName`](../../configuration.md#defaultlanguagename) when null. |
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result -->

## Behavior

What happens depends on the item's latest version:

| Latest version | What Relay does |
|---|---|
| Published | Unpublishes it |
| Unpublished | Nothing; the answer says it is already unpublished |
| A draft that was never published | Nothing; the answer says the item isn't published and only has a draft |
| Published, with a newer draft | Refuses: Kentico won't unpublish an item while it has a draft with unpublished changes. Publish or discard the draft first |

- **The content stays.** An unpublished item isn't deleted; [`publish-content-item`](publish-content-item.md) puts it back online.
- **Reusable items only.** Use [`unpublish-web-page`](../web-pages/unpublish-web-page.md) for pages.

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Take a published item offline

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "unpublish-content-item",
      "parameters": {
        "contentItemId": 184
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new UnpublishContentItemCommand
    {
        ContentItemId = 184,
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Unpublished content item 184 (en).",
  "error": null,
  "data": null
}
```

### Unpublish an item that is already unpublished

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "unpublish-content-item",
      "parameters": {
        "contentItemId": 184
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new UnpublishContentItemCommand
    {
        ContentItemId = 184,
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Content item 184 (en) is already unpublished.",
  "error": null,
  "data": null
}
```
<!-- relay:end examples -->

<!-- relay:begin errors -->
<!-- relay:end errors -->

## See also

- [`publish-content-item`](publish-content-item.md)
- [`delete-content-item`](delete-content-item.md) to remove an item instead
- [Drafts and publishing](../../concepts/drafts-and-publishing.md)
