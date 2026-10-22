---
toc_depth: 2
---

# Custom objects

Generic access to any object type registered in Kentico, by its object type name: custom module classes, and built-in types too. These commands need an Admin key, because they can reach anything the type's table holds.

The examples use `cms.country`, a built-in type that exists on every site. Use your own object type name, such as `custom.redirect`.

<!-- relay:begin commands -->
| Command | Access | What it does |
|---|---|---|
| [`create-custom-object`](#create-custom-object) | Admin | Creates an object of any registered Kentico object type (typically a custom module class) by setting the given columns and inserting it, so Kentico's events and cache invalidation run as usual. |
| [`update-custom-object`](#update-custom-object) | Admin | Updates an object of any registered Kentico object type by ID, changing only the given columns, through Kentico so its events and cache invalidation run as usual. |
| [`delete-custom-object`](#delete-custom-object) | Admin | Deletes an object of any registered Kentico object type by ID, through Kentico so its events and cache invalidation run as usual. |
| [`query-custom-objects`](#query-custom-objects) | Admin | Reads objects of any registered Kentico object type (typically a custom module class). |
<!-- relay:end commands -->

---

## create-custom-object

<!-- relay:begin header:create-custom-object -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `create-custom-object` | [Admin](../../concepts/access-tiers.md) | [`CreateCustomObjectResult`](../../reference/types.md#createcustomobjectresult) | `CreateCustomObjectCommand` |
<!-- relay:end header:create-custom-object -->

Creates an object of any registered Kentico object type, typically a custom module class, by setting columns and inserting it.

<!-- relay:begin parameters:create-custom-object -->
### Parameters { #create-custom-object-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `objectTypeName` | string | Yes | Registered object type name, e.g. `custom.redirect`. |
| `fields` | { name: any } | Yes | Column name to value for the new object. |
<!-- relay:end parameters:create-custom-object -->

<!-- relay:begin result:create-custom-object -->
### Result { #create-custom-object-result }

A successful run returns a [CreateCustomObjectResult](../../reference/types.md#createcustomobjectresult) in `data`:

| Field | Type | Description |
|---|---|---|
| `id` | integer | ID of the new object. |
<!-- relay:end result:create-custom-object -->

### Behavior { #create-custom-object-behavior }

- **Admin only.** These commands read and write any registered Kentico object type, so they need an Admin key. See [Access tiers and API keys](../../concepts/access-tiers.md).
- **Object type names.** `objectTypeName` is the registered name of the type, such as `custom.redirect` for a custom module class, or `cms.country` for a built-in type. The examples on this page use `cms.country`, which exists on every site. A name Kentico doesn't know fails with "Object type '...' was not found."
- **Protected.** The type that holds Relay's own API keys can't be reached through these commands: it fails with "Object type '...' is protected."

- **Through Kentico.** The object is saved with Kentico's own insert, so its events, validation and cache invalidation run as for any other save.
- **Columns.** `fields` maps column names to values (text, numbers or booleans) and can't be empty. A column the object type doesn't have is refused, and a column that must have a value has to be given, otherwise the database refuses and the answer carries its message.
- **The result.** `id` is the new object's ID, for [`update-custom-object`](#update-custom-object) and [`delete-custom-object`](#delete-custom-object).

<!-- relay:begin examples:create-custom-object -->
### Examples { #create-custom-object-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Create an object { #create-custom-object-example-country }

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
<!-- relay:end examples:create-custom-object -->

<!-- relay:begin errors:create-custom-object -->
### Errors { #create-custom-object-errors }

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
<!-- relay:end errors:create-custom-object -->

### See also { #create-custom-object-see-also }

- [`update-custom-object`](#update-custom-object)
- [`delete-custom-object`](#delete-custom-object)
- [`query-custom-objects`](#query-custom-objects)

---

## update-custom-object

<!-- relay:begin header:update-custom-object -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `update-custom-object` | [Admin](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `UpdateCustomObjectCommand` |
<!-- relay:end header:update-custom-object -->

Changes columns of an object of any registered Kentico object type, by ID. Only the columns you give change.

<!-- relay:begin parameters:update-custom-object -->
### Parameters { #update-custom-object-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `objectTypeName` | string | Yes | Registered object type name, e.g. `custom.redirect`. |
| `id` | integer | Yes | ID of the object to update. Must be positive. |
| `fields` | { name: any } | No | Column name to new value. Only these columns change. At least one is required. |
<!-- relay:end parameters:update-custom-object -->

<!-- relay:begin result:update-custom-object -->
### Result { #update-custom-object-result }

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result:update-custom-object -->

### Behavior { #update-custom-object-behavior }

- **Admin only.** These commands read and write any registered Kentico object type, so they need an Admin key. See [Access tiers and API keys](../../concepts/access-tiers.md).
- **Object type names.** `objectTypeName` is the registered name of the type, such as `custom.redirect` for a custom module class, or `cms.country` for a built-in type. The examples on this page use `cms.country`, which exists on every site. A name Kentico doesn't know fails with "Object type '...' was not found."
- **Protected.** The type that holds Relay's own API keys can't be reached through these commands: it fails with "Object type '...' is protected."

- **Through Kentico.** The object is saved with Kentico's own update, so its events, validation and cache invalidation run as for any other save.
- **Columns.** `fields` maps column names to new values and can't be empty. A column the object type doesn't have is refused.
- **The object.** `id` has to be positive and belong to an existing object: "'cms.country' with ID N was not found."

<!-- relay:begin examples:update-custom-object -->
### Examples { #update-custom-object-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Change some of an object's columns { #update-custom-object-example-country }

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
<!-- relay:end examples:update-custom-object -->

<!-- relay:begin errors:update-custom-object -->
### Errors { #update-custom-object-errors }

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
<!-- relay:end errors:update-custom-object -->

### See also { #update-custom-object-see-also }

- [`create-custom-object`](#create-custom-object)
- [`query-custom-objects`](#query-custom-objects)

---

## delete-custom-object

<!-- relay:begin header:delete-custom-object -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `delete-custom-object` | [Admin](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `DeleteCustomObjectCommand` |
<!-- relay:end header:delete-custom-object -->

Deletes an object of any registered Kentico object type, by ID.

<!-- relay:begin parameters:delete-custom-object -->
### Parameters { #delete-custom-object-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `objectTypeName` | string | Yes | Registered object type name, e.g. `custom.redirect`. |
| `id` | integer | Yes | ID of the object to delete. Must be positive. |
<!-- relay:end parameters:delete-custom-object -->

<!-- relay:begin result:delete-custom-object -->
### Result { #delete-custom-object-result }

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result:delete-custom-object -->

### Behavior { #delete-custom-object-behavior }

- **Admin only.** These commands read and write any registered Kentico object type, so they need an Admin key. See [Access tiers and API keys](../../concepts/access-tiers.md).
- **Object type names.** `objectTypeName` is the registered name of the type, such as `custom.redirect` for a custom module class, or `cms.country` for a built-in type. The examples on this page use `cms.country`, which exists on every site. A name Kentico doesn't know fails with "Object type '...' was not found."
- **Protected.** The type that holds Relay's own API keys can't be reached through these commands: it fails with "Object type '...' is protected."

- **Through Kentico.** The object is deleted with Kentico's own delete, so its events and cache invalidation run, and Kentico's rules about dependent objects apply.
- **Not reversible through Relay.** There is no command to bring a deleted object back.
- **The object.** `id` has to be positive and belong to an existing object: "'cms.country' with ID N was not found."

<!-- relay:begin examples:delete-custom-object -->
### Examples { #delete-custom-object-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Delete an object { #delete-custom-object-example-country }

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "delete-custom-object",
      "parameters": {
        "objectTypeName": "cms.country",
        "id": 771
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new DeleteCustomObjectCommand
    {
        ObjectTypeName = "cms.country",
        Id = 771,
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Deleted 'cms.country' with ID 771.",
  "error": null,
  "data": null
}
```
<!-- relay:end examples:delete-custom-object -->

<!-- relay:begin errors:delete-custom-object -->
### Errors { #delete-custom-object-errors }

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "An object that does not exist"

    Parameters:

    ```json
    {
      "objectTypeName": "cms.country",
      "id": 99999
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
<!-- relay:end errors:delete-custom-object -->

### See also { #delete-custom-object-see-also }

- [`query-custom-objects`](#query-custom-objects) to look the object up first
- [`create-custom-object`](#create-custom-object)

---

## query-custom-objects

<!-- relay:begin header:query-custom-objects -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `query-custom-objects` | [Admin](../../concepts/access-tiers.md) | [`QueryCustomObjectsResult`](../../reference/types.md#querycustomobjectsresult) | `QueryCustomObjectsCommand` |
<!-- relay:end header:query-custom-objects -->

Reads objects of any registered Kentico object type, typically a custom module class, and returns the columns you ask for.

<!-- relay:begin parameters:query-custom-objects -->
### Parameters { #query-custom-objects-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `objectTypeName` | string | Yes | Registered object type name, e.g. `custom.redirect`. |
| `columns` | string[] | No | Columns to return. All of the object's columns when omitted or empty. |
| `whereEquals` | { name: any } | No | Optional column-equality filters. All conditions are ANDed together. |
<!-- relay:end parameters:query-custom-objects -->

<!-- relay:begin result:query-custom-objects -->
### Result { #query-custom-objects-result }

A successful run returns a [QueryCustomObjectsResult](../../reference/types.md#querycustomobjectsresult) in `data`:

| Field | Type | Description |
|---|---|---|
| `items` | { name: string }[] | One dictionary per object, mapping column name to its value as text (null for SQL NULL). |
<!-- relay:end result:query-custom-objects -->

### Behavior { #query-custom-objects-behavior }

- **Admin only.** These commands read and write any registered Kentico object type, so they need an Admin key. See [Access tiers and API keys](../../concepts/access-tiers.md).
- **Object type names.** `objectTypeName` is the registered name of the type, such as `custom.redirect` for a custom module class, or `cms.country` for a built-in type. The examples on this page use `cms.country`, which exists on every site. A name Kentico doesn't know fails with "Object type '...' was not found."
- **Protected.** The type that holds Relay's own API keys can't be reached through these commands: it fails with "Object type '...' is protected."

- **Columns.** `columns` names the columns to return; leave it out for all of them. `whereEquals` keeps the objects whose columns equal the values given, combined with AND.
- **Text values.** Every value comes back as text, formatted the way the server formats it, so a date reads like `10/18/2024 5:06:33 AM`. A SQL NULL is `null`.
- **No paging.** The answer holds every matching object.

<!-- relay:begin examples:query-custom-objects -->
### Examples { #query-custom-objects-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Find a country by its two-letter code { #query-custom-objects-example-country }

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

#### No columns named: every column comes back { #query-custom-objects-example-all-columns }

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
<!-- relay:end examples:query-custom-objects -->

<!-- relay:begin errors:query-custom-objects -->
### Errors { #query-custom-objects-errors }

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
<!-- relay:end errors:query-custom-objects -->

### See also { #query-custom-objects-see-also }

- [`create-custom-object`](#create-custom-object)
- [`update-custom-object`](#update-custom-object)
- [`query-sql`](../sql/index.md#query-sql) for anything these commands can't express
