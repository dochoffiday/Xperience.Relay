# delete-taxonomy

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `delete-taxonomy` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `DeleteTaxonomyCommand` |
<!-- relay:end header -->

Deletes a taxonomy together with every tag in it.

<!-- relay:begin parameters -->
## Parameters

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `taxonomyId` | integer | Yes | ID of the taxonomy to delete. |
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result -->

## Behavior

- **The tags go too.** All tags of the taxonomy, nested ones included, are deleted with it.
- **Not reversible through Relay.** There is no command to bring a deleted taxonomy or its tags back.
- **Missing taxonomies.** A taxonomy that doesn't exist fails with "Taxonomy N was not found."

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Delete a taxonomy

Every tag in the taxonomy is deleted with it.

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "delete-taxonomy",
      "parameters": {
        "taxonomyId": 7
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new DeleteTaxonomyCommand
    {
        TaxonomyId = 7,
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Deleted taxonomy 7.",
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
      "taxonomyId": 99999
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

- [`delete-tag`](delete-tag.md) to delete a single tag
- [`get-tags`](get-tags.md) to see what a taxonomy holds
