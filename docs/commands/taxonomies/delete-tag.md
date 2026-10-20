# delete-tag

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `delete-tag` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `DeleteTagCommand` |
<!-- relay:end header -->

Deletes a tag together with its child tags.

<!-- relay:begin parameters -->
## Parameters

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `tagId` | integer | Yes | ID of the tag to delete. |
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result -->

## Behavior

- **The child tags go too.** Deleting a tag deletes every tag below it.
- **Not reversible through Relay.** There is no command to bring a deleted tag back.
- **Missing tags.** A tag that doesn't exist fails with "Tag N was not found."

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Delete a tag that has child tags

The child tag "Early spring" went with it.

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "delete-tag",
      "parameters": {
        "tagId": 25
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new DeleteTagCommand
    {
        TagId = 25,
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Deleted tag 25.",
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
      "tagId": 99999
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

- [`delete-taxonomy`](delete-taxonomy.md) to delete a whole taxonomy
- [`get-tags`](get-tags.md)
