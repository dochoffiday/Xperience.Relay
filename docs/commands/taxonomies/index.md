---
toc_depth: 2
---

# Taxonomies and tags

A taxonomy is a named group of tags, and tags can be nested. Content refers to tags by GUID, in tag fields: [`create-tag`](#create-tag) and [`get-tags`](#get-tags) return the GUIDs that [`create-content-item`](../content-items/index.md#create-content-item) and the update commands take.

Both kinds of commands identify things by ID or by code name; the code name is made from the title when you don't give one.

<!-- relay:begin commands -->
| Command | Access | What it does |
|---|---|---|
| [`create-taxonomy`](#create-taxonomy) | Write | Creates a taxonomy. |
| [`update-taxonomy`](#update-taxonomy) | Write | Changes a taxonomy's title and/or description. |
| [`delete-taxonomy`](#delete-taxonomy) | Write | Deletes a taxonomy. |
| [`get-taxonomies`](#get-taxonomies) | Read | Lists all taxonomies. |
| [`create-tag`](#create-tag) | Write | Creates a tag in a taxonomy, at the root or under another tag. |
| [`update-tag`](#update-tag) | Write | Changes a tag's title and/or description. |
| [`move-tag`](#move-tag) | Write | Moves a tag under another tag, or to the root of its taxonomy, at a given position. |
| [`delete-tag`](#delete-tag) | Write | Deletes a tag. |
| [`get-tags`](#get-tags) | Read | Returns tags matching the given filter. |
<!-- relay:end commands -->

---

## create-taxonomy

<!-- relay:begin header:create-taxonomy -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `create-taxonomy` | [Write](../../concepts/access-tiers.md) | [`CreateTaxonomyResult`](../../reference/types.md#createtaxonomyresult) | `CreateTaxonomyCommand` |
<!-- relay:end header:create-taxonomy -->

Creates a taxonomy: a named group of tags.

<!-- relay:begin parameters:create-taxonomy -->
### Parameters { #create-taxonomy-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `title` | string | Yes | Display title of the taxonomy. |
| `description` | string | No | Optional description. |
| `codeName` | string | No | Optional code name. Made from `title` when omitted: lower case, with each run of characters other than letters and digits replaced by a hyphen (`Coffee tastes` becomes `coffee-tastes`). |
<!-- relay:end parameters:create-taxonomy -->

<!-- relay:begin result:create-taxonomy -->
### Result { #create-taxonomy-result }

A successful run returns a [CreateTaxonomyResult](../../reference/types.md#createtaxonomyresult) in `data`:

| Field | Type | Description |
|---|---|---|
| `taxonomyId` | integer | ID of the new taxonomy. |
| `taxonomyGuid` | guid | GUID of the new taxonomy. |
| `taxonomyName` | string | The taxonomy's code name. |
| `taxonomyTitle` | string | Display title of the new taxonomy. |
<!-- relay:end result:create-taxonomy -->

### Behavior { #create-taxonomy-behavior }

- **Code name.** When `codeName` is left out it is made from the title: lower case, with each run of characters other than letters and digits replaced by a hyphen (`Coffee tastes` becomes `coffee-tastes`).
- **The result.** `taxonomyId` and `taxonomyName` identify the taxonomy for [`create-tag`](#create-tag), which takes either one.

<!-- relay:begin examples:create-taxonomy -->
### Examples { #create-taxonomy-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Create a taxonomy { #create-taxonomy-example-seasons }

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
<!-- relay:end examples:create-taxonomy -->

<!-- relay:begin errors:create-taxonomy -->
### Errors { #create-taxonomy-errors }

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
<!-- relay:end errors:create-taxonomy -->

### See also { #create-taxonomy-see-also }

- [`create-tag`](#create-tag)
- [`update-taxonomy`](#update-taxonomy)
- [`get-taxonomies`](#get-taxonomies)

---

## update-taxonomy

<!-- relay:begin header:update-taxonomy -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `update-taxonomy` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `UpdateTaxonomyCommand` |
<!-- relay:end header:update-taxonomy -->

Changes a taxonomy's title, its description, or both.

<!-- relay:begin parameters:update-taxonomy -->
### Parameters { #update-taxonomy-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `taxonomyId` | integer | Yes | ID of the taxonomy. |
| `title` | string | No | New title. Left as it is when omitted. |
| `description` | string | No | New description. Left as it is when omitted; an empty string clears it. |
<!-- relay:end parameters:update-taxonomy -->

<!-- relay:begin result:update-taxonomy -->
### Result { #update-taxonomy-result }

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result:update-taxonomy -->

### Behavior { #update-taxonomy-behavior }

- **What changes.** Only the fields you give. Leave `title` or `description` out to keep it as it is; an empty `description` clears it. The code name is not changed.
- **Something to change.** At least one of `title` and `description` must be given.

<!-- relay:begin examples:update-taxonomy -->
### Examples { #update-taxonomy-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Change a taxonomy's title { #update-taxonomy-example-title }

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
<!-- relay:end examples:update-taxonomy -->

<!-- relay:begin errors:update-taxonomy -->
### Errors { #update-taxonomy-errors }

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
<!-- relay:end errors:update-taxonomy -->

### See also { #update-taxonomy-see-also }

- [`get-taxonomies`](#get-taxonomies)
- [`delete-taxonomy`](#delete-taxonomy)

---

## delete-taxonomy

<!-- relay:begin header:delete-taxonomy -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `delete-taxonomy` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `DeleteTaxonomyCommand` |
<!-- relay:end header:delete-taxonomy -->

Deletes a taxonomy together with every tag in it.

<!-- relay:begin parameters:delete-taxonomy -->
### Parameters { #delete-taxonomy-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `taxonomyId` | integer | Yes | ID of the taxonomy to delete. |
<!-- relay:end parameters:delete-taxonomy -->

<!-- relay:begin result:delete-taxonomy -->
### Result { #delete-taxonomy-result }

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result:delete-taxonomy -->

### Behavior { #delete-taxonomy-behavior }

- **The tags go too.** All tags of the taxonomy, nested ones included, are deleted with it.
- **Not reversible through Relay.** There is no command to bring a deleted taxonomy or its tags back.
- **Missing taxonomies.** A taxonomy that doesn't exist fails with "Taxonomy N was not found."

<!-- relay:begin examples:delete-taxonomy -->
### Examples { #delete-taxonomy-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Delete a taxonomy { #delete-taxonomy-example-seasons }

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
<!-- relay:end examples:delete-taxonomy -->

<!-- relay:begin errors:delete-taxonomy -->
### Errors { #delete-taxonomy-errors }

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
<!-- relay:end errors:delete-taxonomy -->

### See also { #delete-taxonomy-see-also }

- [`delete-tag`](#delete-tag) to delete a single tag
- [`get-tags`](#get-tags) to see what a taxonomy holds

---

## get-taxonomies

<!-- relay:begin header:get-taxonomies -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `get-taxonomies` | [Read](../../concepts/access-tiers.md) | [`GetTaxonomiesResult`](../../reference/types.md#gettaxonomiesresult) | `GetTaxonomiesCommand` |
<!-- relay:end header:get-taxonomies -->

Lists every taxonomy with its ID, GUID, code name, title and description.

<!-- relay:begin parameters:get-taxonomies -->
### Parameters { #get-taxonomies-parameters }

This command takes no parameters. Send an empty `parameters` object, or leave `parameters` out.
<!-- relay:end parameters:get-taxonomies -->

<!-- relay:begin result:get-taxonomies -->
### Result { #get-taxonomies-result }

A successful run returns a [GetTaxonomiesResult](../../reference/types.md#gettaxonomiesresult) in `data`:

| Field | Type | Description |
|---|---|---|
| `taxonomies` | [TaxonomyResult](../../reference/types.md#taxonomyresult)[] | All taxonomies. |
<!-- relay:end result:get-taxonomies -->

### Behavior { #get-taxonomies-behavior }

- **No parameters.** Send `{}` as the parameters, or leave `parameters` out.
- **All of them.** There is no paging or filtering; a site has few taxonomies.
- **Next step.** [`get-tags`](#get-tags) lists the tags of a taxonomy, by its ID or its code name.

<!-- relay:begin examples:get-taxonomies -->
### Examples { #get-taxonomies-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### List the taxonomies { #get-taxonomies-example-all }

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "get-taxonomies",
      "parameters": {}
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new GetTaxonomiesCommand());
    var data = result.GetData<GetTaxonomiesResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Retrieved 6 taxonomy/taxonomies.",
  "error": null,
  "data": {
    "taxonomies": [
      {
        "taxonomyId": 1,
        "taxonomyGuid": "0eb82e7d-4af6-496a-bfc2-c0b3419956b7",
        "taxonomyName": "CafePromotion",
        "taxonomyTitle": "Cafe Promotions & Offers",
        "taxonomyDescription": ""
      },
      {
        "taxonomyId": 2,
        "taxonomyGuid": "3029353a-41f7-404b-a8be-3dd7e3f437e0",
        "taxonomyName": "CoffeeProcessing",
        "taxonomyTitle": "Coffee processing",
        "taxonomyDescription": ""
      },
      {
        "taxonomyId": 3,
        "taxonomyGuid": "408d5f84-95c6-4ff4-8479-5e2930dbf755",
        "taxonomyName": "CoffeeTastes",
        "taxonomyTitle": "Coffee tastes",
        "taxonomyDescription": ""
      },
      {
        "taxonomyId": 4,
        "taxonomyGuid": "b54af6a7-0bff-40e0-a29b-e7580ce78c6a",
        "taxonomyName": "ProductCategories",
        "taxonomyTitle": "Product categories",
        "taxonomyDescription": ""
      },
      {
        "taxonomyId": 5,
        "taxonomyGuid": "d1e158ea-b068-436c-8d6f-eb0c9bfdcec2",
        "taxonomyName": "ProductManufacturers",
        "taxonomyTitle": "Product manufacturers",
        "taxonomyDescription": ""
      },
      {
        "taxonomyId": 6,
        "taxonomyGuid": "1477faae-3c0b-4d40-baf0-e0d1315b8238",
        "taxonomyName": "ProductTags",
        "taxonomyTitle": "Product tags",
        "taxonomyDescription": ""
      }
    ]
  }
}
```
<!-- relay:end examples:get-taxonomies -->

<!-- relay:begin errors:get-taxonomies -->
<!-- relay:end errors:get-taxonomies -->

### See also { #get-taxonomies-see-also }

- [`get-tags`](#get-tags)
- [`create-taxonomy`](#create-taxonomy)

---

## create-tag

<!-- relay:begin header:create-tag -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `create-tag` | [Write](../../concepts/access-tiers.md) | [`CreateTagResult`](../../reference/types.md#createtagresult) | `CreateTagCommand` |
<!-- relay:end header:create-tag -->

Creates a tag in a taxonomy, at the root or under another tag.

<!-- relay:begin parameters:create-tag -->
### Parameters { #create-tag-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `taxonomyId` | integer | No | ID of the taxonomy to add the tag to. Give this or `taxonomyName`; one is required. |
| `taxonomyName` | string | No | Code name of the taxonomy to add the tag to. Give this or `taxonomyId`; one is required. |
| `title` | string | Yes | Display title of the tag. |
| `description` | string | No | Optional description. |
| `codeName` | string | No | Optional code name. Made from `title` when omitted: lower case, with each run of characters other than letters and digits replaced by a hyphen (`Early spring` becomes `early-spring`). |
| `parentTagId` | integer | No | ID of the parent tag. Omit or set to 0 for a root-level tag. |
| `order` | integer | No | Position among the tag's siblings. When omitted the order isn't set, and the tag reads back with order 0. |
<!-- relay:end parameters:create-tag -->

<!-- relay:begin result:create-tag -->
### Result { #create-tag-result }

A successful run returns a [CreateTagResult](../../reference/types.md#createtagresult) in `data`:

| Field | Type | Description |
|---|---|---|
| `tagId` | integer | ID of the new tag. |
| `tagGuid` | guid | GUID of the new tag, for putting it in a tag field. |
| `tagName` | string | The tag's code name. |
| `tagTitle` | string | Display title of the new tag. |
<!-- relay:end result:create-tag -->

### Behavior { #create-tag-behavior }

- **The taxonomy.** Give `taxonomyId` or `taxonomyName` (the taxonomy's code name); one of them is required. If both are given the ID is used.
- **Code name.** When `codeName` is left out it is made from the title: lower case, with each run of characters other than letters and digits replaced by a hyphen (`Early spring` becomes `early-spring`).
- **Parent.** `parentTagId` puts the tag under another tag. Leave it out, or use `0`, for a tag at the root. The parent has to exist and belong to the same taxonomy.
- **Order.** `order` is the position among the siblings. When it is left out the order isn't set, and the tag reads back with order 0.
- **The result.** `tagGuid` is what a tag field takes when you create or update content: see [`create-content-item`](../content-items/index.md#create-content-item).

<!-- relay:begin examples:create-tag -->
### Examples { #create-tag-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Create a tag at the root of a taxonomy { #create-tag-example-root }

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "create-tag",
      "parameters": {
        "taxonomyId": 7,
        "title": "Spring",
        "description": "March to May"
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new CreateTagCommand
    {
        TaxonomyId = 7,
        Title = "Spring",
        Description = "March to May",
    });
    var data = result.GetData<CreateTagResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Created tag 'Spring' (ID=25) in taxonomy 'seasons'.",
  "error": null,
  "data": {
    "tagId": 25,
    "tagGuid": "562fbc6d-61b3-471e-84ae-ff133abeeb1e",
    "tagName": "spring",
    "tagTitle": "Spring"
  }
}
```

#### Create a tag under another tag, by taxonomy code name { #create-tag-example-child }

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "create-tag",
      "parameters": {
        "taxonomyName": "seasons",
        "title": "Early spring",
        "parentTagId": 25,
        "order": 1
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new CreateTagCommand
    {
        TaxonomyName = "seasons",
        Title = "Early spring",
        ParentTagId = 25,
        Order = 1,
    });
    var data = result.GetData<CreateTagResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Created tag 'Early spring' (ID=26) in taxonomy 'seasons'.",
  "error": null,
  "data": {
    "tagId": 26,
    "tagGuid": "ed37d5b6-af83-4fc7-989d-ee23de2bcbce",
    "tagName": "early-spring",
    "tagTitle": "Early spring"
  }
}
```
<!-- relay:end examples:create-tag -->

<!-- relay:begin errors:create-tag -->
### Errors { #create-tag-errors }

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "No taxonomy named"

    Parameters:

    ```json
    {
      "title": "Autumn"
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Either TaxonomyId or TaxonomyName is required.",
      "data": null
    }
    ```

???+ failure "A taxonomy that does not exist"

    Parameters:

    ```json
    {
      "taxonomyName": "no-such-taxonomy",
      "title": "Autumn"
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Taxonomy 'no-such-taxonomy' was not found.",
      "data": null
    }
    ```

???+ failure "A parent tag that does not exist"

    Parameters:

    ```json
    {
      "taxonomyId": 7,
      "title": "Autumn",
      "parentTagId": 99999
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Parent tag 99999 was not found.",
      "data": null
    }
    ```
<!-- relay:end errors:create-tag -->

### See also { #create-tag-see-also }

- [`get-tags`](#get-tags)
- [`move-tag`](#move-tag)
- [`create-taxonomy`](#create-taxonomy)

---

## update-tag

<!-- relay:begin header:update-tag -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `update-tag` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `UpdateTagCommand` |
<!-- relay:end header:update-tag -->

Changes a tag's title, its description, or both.

<!-- relay:begin parameters:update-tag -->
### Parameters { #update-tag-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `tagId` | integer | Yes | ID of the tag. |
| `title` | string | No | New title. Left as it is when omitted. |
| `description` | string | No | New description. Left as it is when omitted; an empty string clears it. |
<!-- relay:end parameters:update-tag -->

<!-- relay:begin result:update-tag -->
### Result { #update-tag-result }

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result:update-tag -->

### Behavior { #update-tag-behavior }

- **What changes.** Only the fields you give. Leave `title` or `description` out to keep it as it is; an empty `description` clears it. The code name, taxonomy and parent are not changed; [`move-tag`](#move-tag) changes the parent.
- **Something to change.** At least one of `title` and `description` must be given.

<!-- relay:begin examples:update-tag -->
### Examples { #update-tag-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Change a tag's title and description { #update-tag-example-title }

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
<!-- relay:end examples:update-tag -->

<!-- relay:begin errors:update-tag -->
### Errors { #update-tag-errors }

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
<!-- relay:end errors:update-tag -->

### See also { #update-tag-see-also }

- [`get-tags`](#get-tags)
- [`move-tag`](#move-tag)
- [`delete-tag`](#delete-tag)

---

## move-tag

<!-- relay:begin header:move-tag -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `move-tag` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `MoveTagCommand` |
<!-- relay:end header:move-tag -->

Moves a tag under another tag of the same taxonomy, or to the root of its taxonomy, at a given position.

<!-- relay:begin parameters:move-tag -->
### Parameters { #move-tag-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `tagId` | integer | Yes | ID of the tag to move. |
| `targetParentTagId` | integer | Yes | ID of the new parent tag. Use 0 to move to the root of the taxonomy. |
| `order` | integer | Yes | Position among the new siblings. |
<!-- relay:end parameters:move-tag -->

<!-- relay:begin result:move-tag -->
### Result { #move-tag-result }

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result:move-tag -->

### Behavior { #move-tag-behavior }

- **The parent.** `targetParentTagId` is the new parent's ID, or `0` for the root of the taxonomy. The parent has to exist and belong to the same taxonomy as the tag, and a tag can't be moved under itself or under one of its own child tags. Kentico doesn't check this, so Relay does.
- **The children come along.** A tag's child tags move with it.
- **The position.** `order` is the position among the new siblings.
- **Missing tags.** A tag that doesn't exist fails with "Tag N was not found."

<!-- relay:begin examples:move-tag -->
### Examples { #move-tag-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Move a tag under another tag { #move-tag-example-under-another }

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "move-tag",
      "parameters": {
        "tagId": 27,
        "targetParentTagId": 25,
        "order": 2
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new MoveTagCommand
    {
        TagId = 27,
        TargetParentTagId = 25,
        Order = 2,
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Moved tag 27 to parent 25 at order 2.",
  "error": null,
  "data": null
}
```

#### Move a tag to the root of its taxonomy { #move-tag-example-to-root }

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "move-tag",
      "parameters": {
        "tagId": 27,
        "targetParentTagId": 0,
        "order": 2
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new MoveTagCommand
    {
        TagId = 27,
        TargetParentTagId = 0,
        Order = 2,
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Moved tag 27 to parent 0 at order 2.",
  "error": null,
  "data": null
}
```
<!-- relay:end examples:move-tag -->

<!-- relay:begin errors:move-tag -->
### Errors { #move-tag-errors }

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "A tag that does not exist"

    Parameters:

    ```json
    {
      "tagId": 99999,
      "targetParentTagId": 0,
      "order": 1
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

???+ failure "A tag moved under one of its own child tags"

    Tag 17 ("Sour") has the child tag 21 ("Soury") in the sample taxonomy.

    Parameters:

    ```json
    {
      "tagId": 17,
      "targetParentTagId": 21,
      "order": 1
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "A tag can't be moved under itself or under one of its own child tags.",
      "data": null
    }
    ```

???+ failure "A parent that belongs to another taxonomy"

    Parameters:

    ```json
    {
      "tagId": 11,
      "targetParentTagId": 7,
      "order": 1
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Tag 7 belongs to a different taxonomy, so tag 11 can't be moved under it.",
      "data": null
    }
    ```
<!-- relay:end errors:move-tag -->

### See also { #move-tag-see-also }

- [`get-tags`](#get-tags) to see the tree
- [`update-tag`](#update-tag)

---

## delete-tag

<!-- relay:begin header:delete-tag -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `delete-tag` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `DeleteTagCommand` |
<!-- relay:end header:delete-tag -->

Deletes a tag together with its child tags.

<!-- relay:begin parameters:delete-tag -->
### Parameters { #delete-tag-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `tagId` | integer | Yes | ID of the tag to delete. |
<!-- relay:end parameters:delete-tag -->

<!-- relay:begin result:delete-tag -->
### Result { #delete-tag-result }

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result:delete-tag -->

### Behavior { #delete-tag-behavior }

- **The child tags go too.** Deleting a tag deletes every tag below it.
- **Not reversible through Relay.** There is no command to bring a deleted tag back.
- **Missing tags.** A tag that doesn't exist fails with "Tag N was not found."

<!-- relay:begin examples:delete-tag -->
### Examples { #delete-tag-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Delete a tag that has child tags { #delete-tag-example-with-children }

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
<!-- relay:end examples:delete-tag -->

<!-- relay:begin errors:delete-tag -->
### Errors { #delete-tag-errors }

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
<!-- relay:end errors:delete-tag -->

### See also { #delete-tag-see-also }

- [`delete-taxonomy`](#delete-taxonomy) to delete a whole taxonomy
- [`get-tags`](#get-tags)

---

## get-tags

<!-- relay:begin header:get-tags -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `get-tags` | [Read](../../concepts/access-tiers.md) | [`GetTagsResult`](../../reference/types.md#gettagsresult) | `GetTagsCommand` |
<!-- relay:end header:get-tags -->

Lists tags, by taxonomy or by the tag itself.

<!-- relay:begin parameters:get-tags -->
### Parameters { #get-tags-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `taxonomyId` | integer | No | All tags of the taxonomy with this ID. |
| `taxonomyName` | string | No | All tags of the taxonomy with this code name. |
| `tagId` | integer | No | The tag with this ID. |
| `tagName` | string | No | The tag(s) with this code name. |
<!-- relay:end parameters:get-tags -->

<!-- relay:begin result:get-tags -->
### Result { #get-tags-result }

A successful run returns a [GetTagsResult](../../reference/types.md#gettagsresult) in `data`:

| Field | Type | Description |
|---|---|---|
| `tags` | [TagResult](../../reference/types.md#tagresult)[] | The tags that matched. |
<!-- relay:end result:get-tags -->

### Behavior { #get-tags-behavior }

- **A filter is required.** Give at least one of `taxonomyId`, `taxonomyName`, `tagId` and `tagName`. When several are given only one is used, in this order: `tagId`, `tagName`, `taxonomyId`, `taxonomyName`.
- **Flat list.** Tags are returned as a flat list. A nested tag has the ID of its parent in `parentTagId`; a tag at the root of its taxonomy has `null`. `tagOrder` is the position among the siblings.
- **Names.** `tagName` is a tag's code name. Tags in different taxonomies can share one, so a name can return more than one tag.
- **Unknown taxonomy.** A `taxonomyName` that doesn't exist fails with "Taxonomy '...' was not found.", while a `taxonomyId` that doesn't exist gives an empty list.

<!-- relay:begin examples:get-tags -->
### Examples { #get-tags-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### List the tags of a taxonomy { #get-tags-example-taxonomy }

The sample taxonomy nests tags: `parentTagId` is the ID of the tag a tag sits under, or `null` at the root.

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "get-tags",
      "parameters": {
        "taxonomyName": "CoffeeTastes"
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new GetTagsCommand
    {
        TaxonomyName = "CoffeeTastes",
    });
    var data = result.GetData<GetTagsResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Retrieved 6 tag(s).",
  "error": null,
  "data": {
    "tags": [
      {
        "tagId": 17,
        "tagGuid": "785839c6-f7c6-4006-b8c8-115fd57864dd",
        "tagName": "Sour",
        "tagTitle": "Sour",
        "tagDescription": "",
        "taxonomyId": 3,
        "parentTagId": null,
        "tagOrder": 2
      },
      {
        "tagId": 19,
        "tagGuid": "0c877356-23d4-4013-b2b3-e1359e419c70",
        "tagName": "Sweet",
        "tagTitle": "Sweet",
        "tagDescription": "",
        "taxonomyId": 3,
        "parentTagId": null,
        "tagOrder": 1
      },
      {
        "tagId": 21,
        "tagGuid": "af7934d5-aba8-45ef-ba7b-10f83956ca26",
        "tagName": "Soury",
        "tagTitle": "Soury",
        "tagDescription": "",
        "taxonomyId": 3,
        "parentTagId": 17,
        "tagOrder": 1
      },
      {
        "tagId": 22,
        "tagGuid": "f2dab53e-dd76-4e3d-a9e4-f86510ff0e42",
        "tagName": "Winey",
        "tagTitle": "Winey",
        "tagDescription": "",
        "taxonomyId": 3,
        "parentTagId": 17,
        "tagOrder": 2
      },
      {
        "tagId": 23,
        "tagGuid": "b9b0c153-2135-4ac7-a3c6-4b14780548cb",
        "tagName": "Acidy",
        "tagTitle": "Acidy",
        "tagDescription": "",
        "taxonomyId": 3,
        "parentTagId": 19,
        "tagOrder": 1
      },
      {
        "tagId": 24,
        "tagGuid": "098eee31-e8bb-49b8-b425-f9a72e0e130b",
        "tagName": "Mellow",
        "tagTitle": "Mellow",
        "tagDescription": "",
        "taxonomyId": 3,
        "parentTagId": 19,
        "tagOrder": 2
      }
    ]
  }
}
```

#### Find a tag by its code name { #get-tags-example-one-tag }

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "get-tags",
      "parameters": {
        "tagName": "HappyHour"
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new GetTagsCommand
    {
        TagName = "HappyHour",
    });
    var data = result.GetData<GetTagsResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Retrieved 1 tag(s).",
  "error": null,
  "data": {
    "tags": [
      {
        "tagId": 11,
        "tagGuid": "9966ec69-20f5-40f3-9833-9122ea53e416",
        "tagName": "HappyHour",
        "tagTitle": "Happy Hour",
        "tagDescription": "",
        "taxonomyId": 1,
        "parentTagId": null,
        "tagOrder": 1
      }
    ]
  }
}
```
<!-- relay:end examples:get-tags -->

<!-- relay:begin errors:get-tags -->
### Errors { #get-tags-errors }

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "No filter given"

    Parameters:

    ```json
    {}
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "At least one filter (TaxonomyId, TaxonomyName, TagId, or TagName) is required.",
      "data": null
    }
    ```
<!-- relay:end errors:get-tags -->

### See also { #get-tags-see-also }

- [`get-taxonomies`](#get-taxonomies)
- [`create-tag`](#create-tag)
- [`move-tag`](#move-tag)
