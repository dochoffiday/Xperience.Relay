# unpublish-web-page

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `unpublish-web-page` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `UnpublishWebPageCommand` |
<!-- relay:end header -->

Takes a published page in one language offline.

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

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result -->

## Behavior

What happens depends on the page's latest version:

| Latest version | What Relay does |
|---|---|
| Published | Unpublishes it |
| Unpublished | Nothing; the answer says it is already unpublished |
| A draft that was never published | Nothing; the answer says the page isn't published and only has a draft |
| Published, with a newer draft | Refuses: Kentico won't unpublish a page while it has a draft with unpublished changes. Publish or discard the draft first |

- **The content stays.** An unpublished page isn't deleted; [`publish-web-page`](publish-web-page.md) puts it back online.
- **Safe to repeat.** Asking for a state the page is already in succeeds without changing anything.
- **One language.** Only `languageName` is unpublished.

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Take a published page offline

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "unpublish-web-page",
      "parameters": {
        "webPageId": 68
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new UnpublishWebPageCommand
    {
        WebPageId = 68,
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Unpublished web page 68 (en).",
  "error": null,
  "data": null
}
```
<!-- relay:end examples -->

<!-- relay:begin errors -->
<!-- relay:end errors -->

## See also

- [`publish-web-page`](publish-web-page.md)
- [`delete-web-page`](delete-web-page.md) to remove a page instead
- [Drafts and publishing](../../concepts/drafts-and-publishing.md)
