# delete-contacts

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `delete-contacts` | [Admin](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `DeleteContactsCommand` |
<!-- relay:end header -->

Deletes the contacts that match a SQL condition, through Kentico's contact deletion service.

<!-- relay:begin parameters -->
## Parameters

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `whereClause` | string | Yes | A SQL WHERE expression, without the `WHERE` keyword, over the contact table's columns, e.g. `ContactEmail IS NULL`. |
| `batchSize` | integer | No | When omitted (or 0), Kentico deletes every matching contact, in batches of 1000. When given, only a single batch of this size is deleted per call. |
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result -->

## Behavior

- **Admin only, and irreversible.** Kentico's contact deletion service also deletes the data that belongs to the contacts, such as their activities and consent agreements. There is no recycle bin.
- **The condition.** `whereClause` is a SQL `WHERE` expression, without the `WHERE` keyword, over the contact table's columns, for example `ContactEmail IS NULL` or `ContactCreated < '2024-01-01' AND ContactEmail IS NULL`. It is required: Relay won't delete every contact when it is empty.
- **Batches.** Leave `batchSize` out, or use 0, and Kentico deletes every matching contact, in batches of 1000. Give a `batchSize` and a single batch of that many contacts is deleted per call; repeat the call for more.
- **No count.** The answer doesn't say how many contacts were deleted. Count them with [`query-sql`](../sql/query-sql.md) before and after: `SELECT COUNT(*) FROM OM_Contact WHERE ContactEmail IS NULL`.

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Delete one batch only

Three contacts have no email address; one batch of one contact was deleted.

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "delete-contacts",
      "parameters": {
        "whereClause": "ContactEmail IS NULL",
        "batchSize": 1
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new DeleteContactsCommand
    {
        WhereClause = "ContactEmail IS NULL",
        BatchSize = 1,
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Deleted contacts matching condition 'ContactEmail IS NULL' in a single batch of 1.",
  "error": null,
  "data": null
}
```

### Delete every contact that has no email address

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "delete-contacts",
      "parameters": {
        "whereClause": "ContactEmail IS NULL"
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new DeleteContactsCommand
    {
        WhereClause = "ContactEmail IS NULL",
    });
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Deleted contacts matching condition 'ContactEmail IS NULL' across all batches.",
  "error": null,
  "data": null
}
```
<!-- relay:end examples -->

<!-- relay:begin errors -->
## Errors

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "No condition given"

    Parameters:

    ```json
    {}
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "WhereClause is required.",
      "data": null
    }
    ```
<!-- relay:end errors -->

## See also

- [`query-sql`](../sql/query-sql.md) to preview which contacts match
- [`get-form-submissions`](get-form-submissions.md)
