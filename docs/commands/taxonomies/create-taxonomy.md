# create-taxonomy

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `create-taxonomy` | [Write](../../concepts/access-tiers.md) | [`CreateTaxonomyResult`](../../reference/types.md#createtaxonomyresult) | `CreateTaxonomyCommand` |
<!-- relay:end header -->

Creates a taxonomy: a named group of tags.

<!-- relay:begin parameters -->
## Parameters

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `title` | string | Yes | Display title of the taxonomy. |
| `description` | string | No | Optional description. |
| `codeName` | string | No | Optional code name. Made from `title` when omitted: lower case, with each run of characters other than letters and digits replaced by a hyphen (`Coffee tastes` becomes `coffee-tastes`). |
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns a [CreateTaxonomyResult](../../reference/types.md#createtaxonomyresult) in `data`:

| Field | Type | Description |
|---|---|---|
| `taxonomyId` | integer | ID of the new taxonomy. |
| `taxonomyGuid` | guid | GUID of the new taxonomy. |
| `taxonomyName` | string | The taxonomy's code name. |
| `taxonomyTitle` | string | Display title of the new taxonomy. |
<!-- relay:end result -->

## Behavior

- **Code name.** When `codeName` is left out it is made from the title: lower case, with each run of characters other than letters and digits replaced by a hyphen (`Coffee tastes` becomes `coffee-tastes`).
- **The result.** `taxonomyId` and `taxonomyName` identify the taxonomy for [`create-tag`](create-tag.md), which takes either one.

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Create a taxonomy

No `codeName` was given, so Kentico made one from the title.

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "create-taxonomy",
      "parameters": {
        "title": "Seasons",
        "description": "Seasonal menu themes"
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new CreateTaxonomyCommand
    {
        Title = "Seasons",
        Description = "Seasonal menu themes",
    });
    var data = result.GetData<CreateTaxonomyResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Created taxonomy 'Seasons' (ID=7).",
  "error": null,
  "data": {
    "taxonomyId": 7,
    "taxonomyGuid": "877667ca-3a01-49c3-bd01-e541b61675fd",
    "taxonomyName": "seasons",
    "taxonomyTitle": "Seasons"
  }
}
```
<!-- relay:end examples -->

<!-- relay:begin errors -->
## Errors

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "No title"

    Parameters:

    ```json
    {
      "description": "Seasonal menu themes"
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Title is required.",
      "data": null
    }
    ```
<!-- relay:end errors -->

## See also

- [`create-tag`](create-tag.md)
- [`update-taxonomy`](update-taxonomy.md)
- [`get-taxonomies`](get-taxonomies.md)
