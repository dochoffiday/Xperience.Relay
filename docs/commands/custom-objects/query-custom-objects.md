# query-custom-objects

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `query-custom-objects` | [Admin](../../concepts/access-tiers.md) | [`QueryCustomObjectsResult`](../../reference/types.md#querycustomobjectsresult) | `QueryCustomObjectsCommand` |
<!-- relay:end header -->

Reads objects of any registered Kentico object type, typically a custom module class, and returns the columns you ask for.

<!-- relay:begin parameters -->
## Parameters

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `objectTypeName` | string | Yes | Registered object type name, e.g. `custom.redirect`. |
| `columns` | string[] | No | Columns to return. All of the object's columns when omitted or empty. |
| `whereEquals` | { name: any } | No | Optional column-equality filters. All conditions are ANDed together. |
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns a [QueryCustomObjectsResult](../../reference/types.md#querycustomobjectsresult) in `data`:

| Field | Type | Description |
|---|---|---|
| `items` | { name: string }[] | One dictionary per object, mapping column name to its value as text (null for SQL NULL). |
<!-- relay:end result -->

## Behavior

- **Admin only.** These commands read and write any registered Kentico object type, so they need an Admin key. See [Access tiers and API keys](../../concepts/access-tiers.md).
- **Object type names.** `objectTypeName` is the registered name of the type, such as `custom.redirect` for a custom module class, or `cms.country` for a built-in type. The examples on this page use `cms.country`, which exists on every site. A name Kentico doesn't know fails with "Object type '...' was not found."
- **Protected.** The type that holds Relay's own API keys can't be reached through these commands: it fails with "Object type '...' is protected."

- **Columns.** `columns` names the columns to return; leave it out for all of them. `whereEquals` keeps the objects whose columns equal the values given, combined with AND.
- **Text values.** Every value comes back as text, formatted the way the server formats it, so a date reads like `10/18/2024 5:06:33 AM`. A SQL NULL is `null`.
- **No paging.** The answer holds every matching object.

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Find a country by its two-letter code

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "query-custom-objects",
      "parameters": {
        "objectTypeName": "cms.country",
        "columns": [
          "CountryID",
          "CountryName",
          "CountryDisplayName",
          "CountryTwoLetterCode"
        ],
        "whereEquals": {
          "CountryTwoLetterCode": "US"
        }
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new QueryCustomObjectsCommand
    {
        ObjectTypeName = "cms.country",
        Columns = ["CountryID", "CountryName", "CountryDisplayName", "CountryTwoLetterCode"],
        WhereEquals = new()
        {
            ["CountryTwoLetterCode"] = JsonSerializer.SerializeToElement("US"),
        },
    });
    var data = result.GetData<QueryCustomObjectsResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Found 1 object(s) of type 'cms.country'.",
  "error": null,
  "data": {
    "items": [
      {
        "CountryID": "271",
        "CountryName": "USA",
        "CountryDisplayName": "USA",
        "CountryTwoLetterCode": "US"
      }
    ]
  }
}
```

### No columns named: every column comes back

Every value is text, formatted the way the server formats it: that includes `CountryLastModified`.

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "query-custom-objects",
      "parameters": {
        "objectTypeName": "cms.country",
        "whereEquals": {
          "CountryTwoLetterCode": "US"
        }
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new QueryCustomObjectsCommand
    {
        ObjectTypeName = "cms.country",
        WhereEquals = new()
        {
            ["CountryTwoLetterCode"] = JsonSerializer.SerializeToElement("US"),
        },
    });
    var data = result.GetData<QueryCustomObjectsResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Found 1 object(s) of type 'cms.country'.",
  "error": null,
  "data": {
    "items": [
      {
        "CountryID": "271",
        "CountryDisplayName": "USA",
        "CountryName": "USA",
        "CountryGUID": "e431b7e6-9e6c-409d-a1d9-748cbf51b5d6",
        "CountryLastModified": "10/18/2024 5:06:33 AM",
        "CountryTwoLetterCode": "US",
        "CountryThreeLetterCode": "USA"
      }
    ]
  }
}
```
<!-- relay:end examples -->

<!-- relay:begin errors -->
## Errors

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "An object type that is not registered"

    Parameters:

    ```json
    {
      "objectTypeName": "custom.redirect"
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Object type 'custom.redirect' was not found.",
      "data": null
    }
    ```

???+ failure "The type that holds the API keys"

    Relay refuses to read or write its own API key objects through the generic commands.

    Parameters:

    ```json
    {
      "objectTypeName": "relay.apikey"
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Object type 'relay.apikey' is protected.",
      "data": null
    }
    ```
<!-- relay:end errors -->

## See also

- [`create-custom-object`](create-custom-object.md)
- [`update-custom-object`](update-custom-object.md)
- [`query-sql`](../sql/query-sql.md) for anything these commands can't express
