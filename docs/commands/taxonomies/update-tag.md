# update-tag

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `update-tag` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `UpdateTagCommand` |
<!-- relay:end header -->

Changes a tag's title, its description, or both.

<!-- relay:begin parameters -->
## Parameters

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `tagId` | integer | Yes | ID of the tag. |
| `title` | string | No | New title. Left as it is when omitted. |
| `description` | string | No | New description. Left as it is when omitted; an empty string clears it. |
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result -->

## Behavior

- **What changes.** Only the fields you give. Leave `title` or `description` out to keep it as it is; an empty `description` clears it. The code name, taxonomy and parent are not changed; [`move-tag`](move-tag.md) changes the parent.
- **Something to change.** At least one of `title` and `description` must be given.

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Change a tag's title and description

An empty `description` clears it.

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "update-tag",
      "parameters": {
        "tagId": 25,
        "title": "Spring (Mar-May)",
        "description": ""
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new UpdateTagCommand
    {
        TagId = 25,
        Title = "Spring (Mar-May)",
        Description = "",
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Updated tag 25.",
  "error": null,
  "data": null
}
```
<!-- relay:end examples -->

<!-- relay:begin errors -->
## Errors

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "A tag that does not exist"

    Parameters:

    ```json
    {
      "tagId": 99999,
      "title": "Autumn"
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Tag 99999 was not found.",
      "data": null
    }
    ```
<!-- relay:end errors -->

## See also

- [`get-tags`](get-tags.md)
- [`move-tag`](move-tag.md)
- [`delete-tag`](delete-tag.md)
