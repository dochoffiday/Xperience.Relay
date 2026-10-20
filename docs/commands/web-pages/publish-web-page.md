# publish-web-page

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `publish-web-page` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `PublishWebPageCommand` |
<!-- relay:end header -->

Publishes a page in one language.

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
| A draft (including a draft over a published version) | Publishes the draft |
| Unpublished | Makes a draft first, then publishes it, so an unpublished page can be published again |
| Published | Nothing; the answer says the page is already published |

- **Safe to repeat.** Asking for a state the page is already in succeeds without changing anything.
- **Never a false answer.** A page, or a variant in that language, that doesn't exist is a failure, and so is Kentico declining to publish ("Kentico did not publish ... Check the event log.").
- **One language.** Only `languageName` is published.

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Publish a page that is a draft

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "publish-web-page",
      "parameters": {
        "webPageId": 68
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new PublishWebPageCommand
    {
        WebPageId = 68,
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Published web page 68 (en).",
  "error": null,
  "data": null
}
```

### Publish a page that is already published

Asking for a state the page is already in succeeds without changing anything, so the command is safe to repeat.

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "publish-web-page",
      "parameters": {
        "webPageId": 68
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new PublishWebPageCommand
    {
        WebPageId = 68,
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Web page 68 (en) is already published.",
  "error": null,
  "data": null
}
```

### Publish a page that was unpublished

An unpublished page gets a draft first, then that draft is published.

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "publish-web-page",
      "parameters": {
        "webPageId": 68
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new PublishWebPageCommand
    {
        WebPageId = 68,
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Published web page 68 (en).",
  "error": null,
  "data": null
}
```
<!-- relay:end examples -->

<!-- relay:begin errors -->
<!-- relay:end errors -->

## See also

- [`unpublish-web-page`](unpublish-web-page.md) to take a page offline
- [`update-web-page`](update-web-page.md) to change a page without publishing it
- [Drafts and publishing](../../concepts/drafts-and-publishing.md)
