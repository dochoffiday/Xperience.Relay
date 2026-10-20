# update-custom-object

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `update-custom-object` | [Admin](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `UpdateCustomObjectCommand` |
<!-- relay:end header -->

Changes columns of an object of any registered Kentico object type, by ID. Only the columns you give change.

<!-- relay:begin parameters -->
## Parameters

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `objectTypeName` | string | Yes | Registered object type name, e.g. `custom.redirect`. |
| `id` | integer | Yes | ID of the object to update. Must be positive. |
| `fields` | { name: any } | No | Column name to new value. Only these columns change. At least one is required. |
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result -->

## Behavior

- **Admin only.** These commands read and write any registered Kentico object type, so they need an Admin key. See [Access tiers and API keys](../../concepts/access-tiers.md).
- **Object type names.** `objectTypeName` is the registered name of the type, such as `custom.redirect` for a custom module class, or `cms.country` for a built-in type. The examples on this page use `cms.country`, which exists on every site. A name Kentico doesn't know fails with "Object type '...' was not found."
- **Protected.** The type that holds Relay's own API keys can't be reached through these commands: it fails with "Object type '...' is protected."

- **Through Kentico.** The object is saved with Kentico's own update, so its events, validation and cache invalidation run as for any other save.
- **Columns.** `fields` maps column names to new values and can't be empty. A column the object type doesn't have is refused.
- **The object.** `id` has to be positive and belong to an existing object: "'cms.country' with ID N was not found."

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Change some of an object's columns

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "update-custom-object",
      "parameters": {
        "objectTypeName": "cms.country",
        "id": 771,
        "fields": {
          "CountryDisplayName": "Elbonia (Republic of)"
        }
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new UpdateCustomObjectCommand
    {
        ObjectTypeName = "cms.country",
        Id = 771,
        Fields = new()
        {
            ["CountryDisplayName"] = JsonSerializer.SerializeToElement("Elbonia (Republic of)"),
        },
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Updated 'cms.country' with ID 771.",
  "error": null,
  "data": null
}
```
<!-- relay:end examples -->

<!-- relay:begin errors -->
## Errors

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "An object that does not exist"

    Parameters:

    ```json
    {
      "objectTypeName": "cms.country",
      "id": 99999,
      "fields": {
        "CountryDisplayName": "Nowhere"
      }
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "'cms.country' with ID 99999 was not found.",
      "data": null
    }
    ```

???+ failure "A column the object type does not have"

    Parameters:

    ```json
    {
      "objectTypeName": "cms.country",
      "id": 271,
      "fields": {
        "CountryFlag": "blue"
      }
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Column 'CountryFlag' was not found on object type 'cms.country'.",
      "data": null
    }
    ```
<!-- relay:end errors -->

## See also

- [`create-custom-object`](create-custom-object.md)
- [`query-custom-objects`](query-custom-objects.md)
