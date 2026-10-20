# update-slug

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `update-slug` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `UpdateSlugCommand` |
<!-- relay:end header -->

Changes a page's URL slug, the last part of its URL path, in one language.

<!-- relay:begin parameters -->
## Parameters

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `webPageId` | integer | Yes | ID of the web page. |
| `languageName` | string | No | Defaults to [`RelayKenticoOptions.DefaultLanguageName`](../../configuration.md#defaultlanguagename) when null. |
| `slug` | string | Yes | The new slug (the last part of the URL path). |
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result -->

## Behavior

- **Same flow as [`update-web-page`](update-web-page.md).** A published page is published again, so the new URL is live as soon as the answer comes back. A page that is a draft or unpublished keeps the new slug in its draft, and the answer says it isn't live. See [Drafts and publishing](../../concepts/drafts-and-publishing.md).
- **What the slug is.** It is the page's own part of the URL, after the parts of its parents: with the slug `cold-brew-guide`, the article under Articles is served at `/articles/cold-brew-guide`. The page's tree path (`/Articles/Cold_brew_at_home`) does not change.
- **One language.** Only `languageName` changes; the page keeps its URLs in other languages.
- **Checking the result.** [`query-web-page-items`](query-web-page-items.md) with `includeUrls` shows the URL a page has now.

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Change the URL slug of a published page

The page was published, so the new slug is published too and goes live immediately.

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "update-slug",
      "parameters": {
        "webPageId": 68,
        "slug": "cold-brew-guide"
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new UpdateSlugCommand
    {
        WebPageId = 68,
        Slug = "cold-brew-guide",
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Updated slug on web page 68 to 'cold-brew-guide'.",
  "error": null,
  "data": null
}
```
<!-- relay:end examples -->

<!-- relay:begin errors -->
<!-- relay:end errors -->

## See also

- [`update-web-page`](update-web-page.md) to change the fields
- [`query-web-page-items`](query-web-page-items.md) to see URLs
- [Drafts and publishing](../../concepts/drafts-and-publishing.md)
