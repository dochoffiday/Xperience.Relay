# create-tag

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `create-tag` | [Write](../../concepts/access-tiers.md) | [`CreateTagResult`](../../reference/types.md#createtagresult) | `CreateTagCommand` |
<!-- relay:end header -->

Creates a tag in a taxonomy, at the root or under another tag.

<!-- relay:begin parameters -->
## Parameters

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
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns a [CreateTagResult](../../reference/types.md#createtagresult) in `data`:

| Field | Type | Description |
|---|---|---|
| `tagId` | integer | ID of the new tag. |
| `tagGuid` | guid | GUID of the new tag, for putting it in a tag field. |
| `tagName` | string | The tag's code name. |
| `tagTitle` | string | Display title of the new tag. |
<!-- relay:end result -->

## Behavior

- **The taxonomy.** Give `taxonomyId` or `taxonomyName` (the taxonomy's code name); one of them is required. If both are given the ID is used.
- **Code name.** When `codeName` is left out it is made from the title: lower case, with each run of characters other than letters and digits replaced by a hyphen (`Early spring` becomes `early-spring`).
- **Parent.** `parentTagId` puts the tag under another tag. Leave it out, or use `0`, for a tag at the root. The parent has to exist and belong to the same taxonomy.
- **Order.** `order` is the position among the siblings. When it is left out the order isn't set, and the tag reads back with order 0.
- **The result.** `tagGuid` is what a tag field takes when you create or update content: see [`create-content-item`](../content-items/create-content-item.md).

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Create a tag at the root of a taxonomy

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

### Create a tag under another tag, by taxonomy code name

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
<!-- relay:end examples -->

<!-- relay:begin errors -->
## Errors

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
<!-- relay:end errors -->

## See also

- [`get-tags`](get-tags.md)
- [`move-tag`](move-tag.md)
- [`create-taxonomy`](create-taxonomy.md)
