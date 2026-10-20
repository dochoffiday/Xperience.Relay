# create-custom-object

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `create-custom-object` | [Admin](../../concepts/access-tiers.md) | [`CreateCustomObjectResult`](../../reference/types.md#createcustomobjectresult) | `CreateCustomObjectCommand` |
<!-- relay:end header -->

Creates an object of any registered Kentico object type, typically a custom module class, by setting columns and inserting it.

<!-- relay:begin parameters -->
## Parameters

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `objectTypeName` | string | Yes | Registered object type name, e.g. `custom.redirect`. |
| `fields` | { name: any } | Yes | Column name to value for the new object. |
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns a [CreateCustomObjectResult](../../reference/types.md#createcustomobjectresult) in `data`:

| Field | Type | Description |
|---|---|---|
| `id` | integer | ID of the new object. |
<!-- relay:end result -->

## Behavior

- **Admin only.** These commands read and write any registered Kentico object type, so they need an Admin key. See [Access tiers and API keys](../../concepts/access-tiers.md).
- **Object type names.** `objectTypeName` is the registered name of the type, such as `custom.redirect` for a custom module class, or `cms.country` for a built-in type. The examples on this page use `cms.country`, which exists on every site. A name Kentico doesn't know fails with "Object type '...' was not found."
- **Protected.** The type that holds Relay's own API keys can't be reached through these commands: it fails with "Object type '...' is protected."

- **Through Kentico.** The object is saved with Kentico's own insert, so its events, validation and cache invalidation run as for any other save.
- **Columns.** `fields` maps column names to values (text, numbers or booleans) and can't be empty. A column the object type doesn't have is refused, and a column that must have a value has to be given, otherwise the database refuses and the answer carries its message.
- **The result.** `id` is the new object's ID, for [`update-custom-object`](update-custom-object.md) and [`delete-custom-object`](delete-custom-object.md).

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Create an object

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "create-custom-object",
      "parameters": {
        "objectTypeName": "cms.country",
        "fields": {
          "CountryName": "Elbonia",
          "CountryDisplayName": "Elbonia",
          "CountryTwoLetterCode": "EB",
          "CountryThreeLetterCode": "ELB"
        }
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new CreateCustomObjectCommand
    {
        ObjectTypeName = "cms.country",
        Fields = new()
        {
            ["CountryName"] = JsonSerializer.SerializeToElement("Elbonia"),
            ["CountryDisplayName"] = JsonSerializer.SerializeToElement("Elbonia"),
            ["CountryTwoLetterCode"] = JsonSerializer.SerializeToElement("EB"),
            ["CountryThreeLetterCode"] = JsonSerializer.SerializeToElement("ELB"),
        },
    });
    var data = result.GetData<CreateCustomObjectResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Created 'cms.country' with ID 771.",
  "error": null,
  "data": {
    "id": 771
  }
}
```
<!-- relay:end examples -->

<!-- relay:begin errors -->
## Errors

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "A column the object type does not have"

    Parameters:

    ```json
    {
      "objectTypeName": "cms.country",
      "fields": {
        "CountryName": "Elbonia",
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

???+ failure "A required column left out"

    Parameters:

    ```json
    {
      "objectTypeName": "cms.country",
      "fields": {
        "CountryTwoLetterCode": "EB"
      }
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Unhandled exception executing verb 'create-custom-object': [CodeNameNotValidException] The object code name '' is not valid. The code name can contain only alphanumeric characters, some special characters (_, -, .) and cannot start or end with '.'.",
      "data": null
    }
    ```
<!-- relay:end errors -->

## See also

- [`update-custom-object`](update-custom-object.md)
- [`delete-custom-object`](delete-custom-object.md)
- [`query-custom-objects`](query-custom-objects.md)
