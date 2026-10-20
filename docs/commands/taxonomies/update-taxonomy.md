# update-taxonomy

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `update-taxonomy` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `UpdateTaxonomyCommand` |
<!-- relay:end header -->

Changes a taxonomy's title, its description, or both.

<!-- relay:begin parameters -->
## Parameters

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `taxonomyId` | integer | Yes | ID of the taxonomy. |
| `title` | string | No | New title. Left as it is when omitted. |
| `description` | string | No | New description. Left as it is when omitted; an empty string clears it. |
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result -->

## Behavior

- **What changes.** Only the fields you give. Leave `title` or `description` out to keep it as it is; an empty `description` clears it. The code name is not changed.
- **Something to change.** At least one of `title` and `description` must be given.

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Change a taxonomy's title

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "update-taxonomy",
      "parameters": {
        "taxonomyId": 7,
        "title": "Seasonal themes"
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new UpdateTaxonomyCommand
    {
        TaxonomyId = 7,
        Title = "Seasonal themes",
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Updated taxonomy 7.",
  "error": null,
  "data": null
}
```
<!-- relay:end examples -->

<!-- relay:begin errors -->
## Errors

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "A taxonomy that does not exist"

    Parameters:

    ```json
    {
      "taxonomyId": 99999,
      "title": "Seasonal themes"
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Taxonomy 99999 was not found.",
      "data": null
    }
    ```
<!-- relay:end errors -->

## See also

- [`get-taxonomies`](get-taxonomies.md)
- [`delete-taxonomy`](delete-taxonomy.md)
