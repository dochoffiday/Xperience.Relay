# update-web-page

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `update-web-page` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `UpdateWebPageCommand` |
<!-- relay:end header -->

Changes fields of a page in one language. Relay edits through a draft, the way the admin UI does, and leaves the page in the state it found it in: a page that was published is published again with the change, and a page that was a draft or unpublished stays that way.

<!-- relay:begin parameters -->
## Parameters

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `webPageId` | integer | Yes | ID of the web page. |
| `languageName` | string | No | Defaults to [`RelayKenticoOptions.DefaultLanguageName`](../../configuration.md#defaultlanguagename) when null. |
| `fields` | { name: any } | No | Plain field values by field name (text, numbers, booleans, dates, ...). |
| `linkedItemFields` | { name: guid[] } | No | Linked-items fields. Maps field name to a list of ContentItemGUIDs. The field must be a content item reference field. An empty list clears the field. |
| `tagFields` | { name: guid[] } | No | Tag fields. Maps field name to a list of tag GUIDs. The field must be a tag (taxonomy) field. An empty list clears the field. |
| `assets` | [RelayAsset](../../reference/types.md#relayasset)[] | No | Optional binary assets to upload into asset fields. Each entry maps to one field. |
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result -->

## Behavior

What happens to the page depends on its latest version:

| The page is | Relay | Afterwards |
|---|---|---|
| Published | makes a draft, saves the change, publishes it | Published, with the change |
| Published, with a newer draft | saves the change into that draft | Still a draft over the published version; the live page is unchanged |
| Never published | saves the change into its draft | Still never published |
| Unpublished | makes a draft and saves the change into it | Still not published |

- **The answer says when the change isn't live.** A page that isn't going live ends the message with "Saved as a draft; it is not published." or, when a published version exists, "Saved as a draft; the published version is unchanged."
- **One language.** Relay reads the page's state in `languageName` first. A page that has no variant in that language fails with "was not found in that language", and other languages are never touched.
- **Failures leave nothing behind.** If Kentico declines to create the draft, to save the change or to publish it, the result says which step failed. A draft made for an edit that then failed is discarded again (a draft that was already there is kept as it was), and Kentico's cached copy of the page is dropped so the next edit doesn't write the refused value again. If only the final publish fails, the change stays saved as a draft and the live page is unchanged. Relay never reports a success it didn't get.
- **Checks.** The fields are checked the way [`create-web-page`](create-web-page.md) checks them, before the draft is made, so a bad request changes nothing.
- **Clearing a field.** An empty list in `linkedItemFields` or `tagFields` clears that field. `null` in `fields` is refused for a field that must have a value.
- **Something to change.** At least one of `fields`, `linkedItemFields`, `tagFields` and `assets` must be given, otherwise the answer is "No fields provided — nothing to update."
- **The URL.** This command doesn't change the page's URL slug; use [`update-slug`](update-slug.md).

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Update fields on a page that has never been published

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "update-web-page",
      "parameters": {
        "webPageId": 68,
        "fields": {
          "ArticleTitle": "Cold brew at home: a beginner'\''s guide",
          "ArticlePageSummary": "Everything you need to make cold brew with a jar and a sieve."
        }
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new UpdateWebPageCommand
    {
        WebPageId = 68,
        Fields = new()
        {
            ["ArticleTitle"] = JsonSerializer.SerializeToElement("Cold brew at home: a beginner's guide"),
            ["ArticlePageSummary"] = JsonSerializer.SerializeToElement("Everything you need to make cold brew with a jar and a sieve."),
        },
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Updated 2 field(s) on web page 68. Saved as a draft; it is not published.",
  "error": null,
  "data": null
}
```

### Update a published page

The page was published, so Relay makes a draft, saves the change and publishes it again.

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "update-web-page",
      "parameters": {
        "webPageId": 68,
        "fields": {
          "ArticlePageSummary": "Everything you need to make cold brew with a jar and a sieve - no special equipment."
        }
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new UpdateWebPageCommand
    {
        WebPageId = 68,
        Fields = new()
        {
            ["ArticlePageSummary"] = JsonSerializer.SerializeToElement("Everything you need to make cold brew with a jar and a sieve - no special equipment."),
        },
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Updated 1 field(s) on web page 68.",
  "error": null,
  "data": null
}
```
<!-- relay:end examples -->

<!-- relay:begin errors -->
## Errors

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "A field the content type does not have, and a value that does not fit"

    Parameters:

    ```json
    {
      "webPageId": 68,
      "fields": {
        "ArticleSubtitle": "A field this type does not have",
        "ArticlePagePublishDate": "last Tuesday"
      }
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Field 'ArticleSubtitle' was not found on content type 'DancingGoat.ArticlePage'. Field 'ArticlePagePublishDate' expects a date, but got 'last Tuesday'.",
      "data": null
    }
    ```
<!-- relay:end errors -->

## See also

- [`update-slug`](update-slug.md) to change the URL
- [`publish-web-page`](publish-web-page.md) to put a draft live
- [`get-page`](get-page.md) to read the fields back
- [Drafts and publishing](../../concepts/drafts-and-publishing.md)
