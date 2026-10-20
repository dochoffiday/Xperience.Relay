# get-content-item-usage

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `get-content-item-usage` | [Read](../../concepts/access-tiers.md) | [`GetContentItemUsageResult`](../../reference/types.md#getcontentitemusageresult) | `GetContentItemUsageCommand` |
<!-- relay:end header -->

Lists the content items that directly refer to one language variant of a content item: the "used by" list of the admin UI.

<!-- relay:begin parameters -->
## Parameters

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `contentItemId` | integer | Yes | ID of the content item (reusable item or page content item) to look up. Must be positive. |
| `languageName` | string | Yes | Language variant to look up, e.g. `en`. There is no default. |
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns a [GetContentItemUsageResult](../../reference/types.md#getcontentitemusageresult) in `data`:

| Field | Type | Description |
|---|---|---|
| `items` | [ContentItemUsageEntry](../../reference/types.md#contentitemusageentry)[] | The content items that reference the item, one entry per language variant. |
<!-- relay:end result -->

## Behavior

- **Check before deleting.** Kentico lets you delete an item that other items refer to, and the references are left pointing at nothing. [`delete-content-item`](delete-content-item.md) doesn't warn you; this command does the looking.
- **One entry per referencing variant.** An item can appear twice, once for each language variant that refers to it (the example shows an event in Spanish and in English). For a web page, `contentItemId` is the page's content item ID, not its web page ID.
- **Direct references only.** Items that refer to the referencing items aren't followed.
- **The language is required.** There is no default: `languageName` says which variant of the item to look at, and leaving it out fails with "LanguageName is required."

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Which items reference the Boston cafe?

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "get-content-item-usage",
      "parameters": {
        "contentItemId": 19,
        "languageName": "en"
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new GetContentItemUsageCommand
    {
        ContentItemId = 19,
        LanguageName = "en",
    });
    var data = result.GetData<GetContentItemUsageResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Found 4 usage(s) for content item 19 in language 'en'.",
  "error": null,
  "data": {
    "items": [
      {
        "contentItemId": 84,
        "languageName": "es",
        "displayName": "¡Nuestra primera cata ha llegado!",
        "latestVersionStatus": "Published",
        "createdWhen": "2023-09-07T07:23:55",
        "modifiedWhen": "2026-10-20T13:08:43.3414616",
        "hasImageAsset": false,
        "scheduledPublishWhen": null,
        "scheduledUnpublishWhen": null
      },
      {
        "contentItemId": 184,
        "languageName": "en",
        "displayName": "Cold brew workshop",
        "latestVersionStatus": "Published",
        "createdWhen": "2026-10-20T13:52:00.5742065",
        "modifiedWhen": "2026-10-20T13:52:00.7551969",
        "hasImageAsset": false,
        "scheduledPublishWhen": null,
        "scheduledUnpublishWhen": null
      },
      {
        "contentItemId": 84,
        "languageName": "en",
        "displayName": "Our first cupping is here!",
        "latestVersionStatus": "Published",
        "createdWhen": "2023-06-13T04:17:44",
        "modifiedWhen": "2026-10-20T13:08:43.3321995",
        "hasImageAsset": false,
        "scheduledPublishWhen": null,
        "scheduledUnpublishWhen": null
      },
      {
        "contentItemId": 106,
        "languageName": "en",
        "displayName": "The art of coffee cupping",
        "latestVersionStatus": "Published",
        "createdWhen": "2023-06-13T04:24:41",
        "modifiedWhen": "2026-10-20T13:08:43.7171427",
        "hasImageAsset": false,
        "scheduledPublishWhen": null,
        "scheduledUnpublishWhen": null
      }
    ]
  }
}
```
<!-- relay:end examples -->

<!-- relay:begin errors -->
## Errors

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "No language named"

    Parameters:

    ```json
    {
      "contentItemId": 19
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "LanguageName is required.",
      "data": null
    }
    ```
<!-- relay:end errors -->

## See also

- [`delete-content-item`](delete-content-item.md)
- [`get-content`](get-content.md)
