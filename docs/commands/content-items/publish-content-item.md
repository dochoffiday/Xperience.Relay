# publish-content-item

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `publish-content-item` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `PublishContentItemCommand` |
<!-- relay:end header -->

Publishes a reusable content item in one language.

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
| A draft (including a draft over a published version) | Publishes the draft |
| Unpublished | Makes a draft first, then publishes it, so an unpublished item can be published again |
| Published | Nothing; the answer says the item is already published |

- **Safe to repeat.** Asking for a state the item is already in succeeds without changing anything.
- **Never a false answer.** An item, or a variant in that language, that doesn't exist is a failure, and so is Kentico declining to publish.
- **Reusable items only.** The content item of a web page is refused with a pointer to the page commands; use [`publish-web-page`](../web-pages/publish-web-page.md) for pages.

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Publish an item that was unpublished

An unpublished item gets a draft first, then that draft is published.

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "publish-content-item",
      "parameters": {
        "contentItemId": 184
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new PublishContentItemCommand
    {
        ContentItemId = 184,
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Published content item 184 (en).",
  "error": null,
  "data": null
}
```

### Publish an item that is already published

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "publish-content-item",
      "parameters": {
        "contentItemId": 184
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new PublishContentItemCommand
    {
        ContentItemId = 184,
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Content item 184 (en) is already published.",
  "error": null,
  "data": null
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

- [`unpublish-content-item`](unpublish-content-item.md)
- [`update-content-item`](update-content-item.md)
- [Drafts and publishing](../../concepts/drafts-and-publishing.md)
