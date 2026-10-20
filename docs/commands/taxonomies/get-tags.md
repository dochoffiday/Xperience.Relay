# get-tags

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `get-tags` | [Read](../../concepts/access-tiers.md) | [`GetTagsResult`](../../reference/types.md#gettagsresult) | `GetTagsCommand` |
<!-- relay:end header -->

Lists tags, by taxonomy or by the tag itself.

<!-- relay:begin parameters -->
## Parameters

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `taxonomyId` | integer | No | All tags of the taxonomy with this ID. |
| `taxonomyName` | string | No | All tags of the taxonomy with this code name. |
| `tagId` | integer | No | The tag with this ID. |
| `tagName` | string | No | The tag(s) with this code name. |
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns a [GetTagsResult](../../reference/types.md#gettagsresult) in `data`:

| Field | Type | Description |
|---|---|---|
| `tags` | [TagResult](../../reference/types.md#tagresult)[] | The tags that matched. |
<!-- relay:end result -->

## Behavior

- **A filter is required.** Give at least one of `taxonomyId`, `taxonomyName`, `tagId` and `tagName`. When several are given only one is used, in this order: `tagId`, `tagName`, `taxonomyId`, `taxonomyName`.
- **Flat list.** Tags are returned as a flat list. A nested tag has the ID of its parent in `parentTagId`; a tag at the root of its taxonomy has `null`. `tagOrder` is the position among the siblings.
- **Names.** `tagName` is a tag's code name. Tags in different taxonomies can share one, so a name can return more than one tag.
- **Unknown taxonomy.** A `taxonomyName` that doesn't exist fails with "Taxonomy '...' was not found.", while a `taxonomyId` that doesn't exist gives an empty list.

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### List the tags of a taxonomy

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

### Find a tag by its code name

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
<!-- relay:end examples -->

<!-- relay:begin errors -->
## Errors

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
<!-- relay:end errors -->

## See also

- [`get-taxonomies`](get-taxonomies.md)
- [`create-tag`](create-tag.md)
- [`move-tag`](move-tag.md)
