# query-sql

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `query-sql` | [Admin](../../concepts/access-tiers.md) | [`QuerySqlResult`](../../reference/types.md#querysqlresult) | `QuerySqlCommand` |
<!-- relay:end header -->

Runs a read-only SQL query against the Xperience database and returns the first result set.

<!-- relay:begin parameters -->
## Parameters

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `query` | string | Yes | The SQL query to execute. May hold several statements. A trailing semicolon is ignored. |
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns a [QuerySqlResult](../../reference/types.md#querysqlresult) in `data`:

| Field | Type | Description |
|---|---|---|
| `columns` | string[] | Column names of the result set. |
| `rows` | string[][] | One list of values per row, in `columns` order. Every value is text; null for SQL NULL. |
<!-- relay:end result -->

## Behavior

- **What is allowed.** `SELECT` (including `WITH ... SELECT`) and `DECLARE`. The text is parsed with Microsoft's T-SQL parser rather than matched against keywords, so comments and string literals can't smuggle another statement in. Anything else is refused: "'DELETE' statements are not allowed in this context." `SELECT ... INTO` is refused too, because it creates a table; a temporary `#table` is fine.
- **Blocked names.** `xp_cmdshell`, `OPENROWSET`, `OPENDATASOURCE`, `OPENQUERY`, `OPENXML` and `sp_configure` are refused wherever they appear, and so is any mention of the table that holds Relay's API keys, even inside a string. See [Guardrails](../../concepts/guardrails.md).
- **The result.** `columns` and `rows`, from the first result set only. Every value is text, and `null` for a SQL NULL. A query that returns no result set fails with "The query returned no result set. End it with a SELECT."
- **Syntax errors.** A query that doesn't parse fails with the parser's message ("SQL parsing error: ...").
- **Time limit.** The query is cut off after [`SqlQueryTimeoutSeconds`](../../configuration.md#sqlquerytimeoutseconds) seconds (30 by default).
- **A guardrail, not a security boundary.** The checks stop mistakes and the obvious abuses. They don't replace access control: give an Admin key only to callers you trust completely.

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Read from the database

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "query-sql",
      "parameters": {
        "query": "SELECT TOP 3 TagID, TagName, TagTitle FROM CMS_Tag ORDER BY TagID"
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new QuerySqlCommand
    {
        Query = "SELECT TOP 3 TagID, TagName, TagTitle FROM CMS_Tag ORDER BY TagID",
    });
    var data = result.GetData<QuerySqlResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Query returned 3 row(s).",
  "error": null,
  "data": {
    "columns": [
      "TagID",
      "TagName",
      "TagTitle"
    ],
    "rows": [
      [
        "1",
        "Accessories",
        "Accessories"
      ],
      [
        "2",
        "Aerobie",
        "Aerobie"
      ],
      [
        "3",
        "Bestsellers",
        "Bestsellers"
      ]
    ]
  }
}
```
<!-- relay:end examples -->

<!-- relay:begin errors -->
## Errors

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "A statement that changes data"

    Parameters:

    ```json
    {
      "query": "DELETE FROM CMS_Tag WHERE TagID = 1"
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "'DELETE' statements are not allowed in this context.",
      "data": null
    }
    ```

???+ failure "A function that is blocked"

    Parameters:

    ```json
    {
      "query": "SELECT * FROM OPENROWSET('SQLNCLI', 'Server=x;', 'SELECT 1')"
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "'OPENROWSET' is not allowed.",
      "data": null
    }
    ```

???+ failure "The table that holds the API keys"

    Parameters:

    ```json
    {
      "query": "SELECT * FROM Relay_ApiKey"
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "The relay API key table is protected and can't be queried or changed through relay.",
      "data": null
    }
    ```

???+ failure "SQL that does not parse"

    Parameters:

    ```json
    {
      "query": "SELECT FROM WHERE"
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "SQL parsing error: Incorrect syntax near 'FROM'.",
      "data": null
    }
    ```

???+ failure "A SELECT that creates a table"

    Parameters:

    ```json
    {
      "query": "SELECT TagID, TagName INTO Tag_Copy FROM CMS_Tag"
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "'SELECT ... INTO' creates a table, so it is not allowed here (a temporary #table is fine).",
      "data": null
    }
    ```

???+ failure "A query that returns nothing"

    Parameters:

    ```json
    {
      "query": "DECLARE @count int = 3"
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "The query returned no result set. End it with a SELECT.",
      "data": null
    }
    ```
<!-- relay:end errors -->

## See also

- [`execute-sql`](execute-sql.md) for statements that change data
- [`query-custom-objects`](../custom-objects/query-custom-objects.md) and [`query-web-page-items`](../web-pages/query-web-page-items.md), which don't need SQL
- [Guardrails](../../concepts/guardrails.md)
