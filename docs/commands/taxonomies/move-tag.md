# move-tag

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `move-tag` | [Write](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `MoveTagCommand` |
<!-- relay:end header -->

Moves a tag under another tag of the same taxonomy, or to the root of its taxonomy, at a given position.

<!-- relay:begin parameters -->
## Parameters

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `tagId` | integer | Yes | ID of the tag to move. |
| `targetParentTagId` | integer | Yes | ID of the new parent tag. Use 0 to move to the root of the taxonomy. |
| `order` | integer | Yes | Position among the new siblings. |
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result -->

## Behavior

- **The parent.** `targetParentTagId` is the new parent's ID, or `0` for the root of the taxonomy. The parent has to exist and belong to the same taxonomy as the tag, and a tag can't be moved under itself or under one of its own child tags. Kentico doesn't check this, so Relay does.
- **The children come along.** A tag's child tags move with it.
- **The position.** `order` is the position among the new siblings.
- **Missing tags.** A tag that doesn't exist fails with "Tag N was not found."

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Move a tag under another tag

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

### Move a tag to the root of its taxonomy

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
<!-- relay:end examples -->

<!-- relay:begin errors -->
## Errors

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
<!-- relay:end errors -->

## See also

- [`get-tags`](get-tags.md) to see the tree
- [`update-tag`](update-tag.md)
