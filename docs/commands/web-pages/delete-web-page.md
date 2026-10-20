# delete-web-page

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `delete-web-page` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `DeleteWebPageCommand` |
<!-- relay:end header -->

Deletes a page in one language. Unless you ask for `permanently`, the page goes to the recycle bin, where it can be restored from the admin UI.

<!-- relay:begin parameters -->
## Parameters

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `webPageId` | integer | Yes | ID of the web page. |
| `languageName` | string | No | Defaults to [`RelayKenticoOptions.DefaultLanguageName`](../../configuration.md#defaultlanguagename) when null. |
| `redirectToWebPageId` | integer | No | When set, a redirect is created pointing to this web page after deletion. |
| `permanently` | boolean | No | Bypasses the recycle bin and permanently removes the page. Defaults to false. |
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result -->

## Behavior

- **Recycle bin, or gone for good.** By default the page and the pages below it are sent to the recycle bin. With `permanently: true` they are removed for good, and there is no way back.
- **The pages below it go too.** Deleting a page deletes its child pages with it.
- **The language.** The command deletes the language variant you name. When that is the page's only variant, the page itself is deleted; a page that also has other languages keeps those.
- **Redirect.** `redirectToWebPageId` asks Kentico to redirect visitors of the deleted page's URLs to another page. The target page has to exist.
- **Missing pages.** A page that doesn't exist, or has no variant in `languageName`, fails. Relay never reports a deletion that didn't happen.

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Delete a page (it goes to the recycle bin)

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "delete-web-page",
      "parameters": {
        "webPageId": 68,
        "languageName": "en"
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new DeleteWebPageCommand
    {
        WebPageId = 68,
        LanguageName = "en",
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Deleted web page 68 (en).",
  "error": null,
  "data": null
}
```

### Delete a page for good

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "delete-web-page",
      "parameters": {
        "webPageId": 69,
        "permanently": true
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new DeleteWebPageCommand
    {
        WebPageId = 69,
        Permanently = true,
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Deleted web page 69 (en) permanently.",
  "error": null,
  "data": null
}
```
<!-- relay:end examples -->

<!-- relay:begin errors -->
## Errors

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "A page that does not exist"

    Parameters:

    ```json
    {
      "webPageId": 99999
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Web page 99999 was not found.",
      "data": null
    }
    ```

???+ failure "A language the page has no variant in"

    Page 9 exists, but only in English.

    Parameters:

    ```json
    {
      "webPageId": 9,
      "languageName": "es"
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Web page 9 (es) was not found in that language.",
      "data": null
    }
    ```
<!-- relay:end errors -->

## See also

- [`unpublish-web-page`](unpublish-web-page.md) to take a page offline without deleting it
- [`move-web-page`](move-web-page.md)
- [`get-page-info`](get-page-info.md) to check a page exists
