# execute-sql

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `execute-sql` | [Admin](../../concepts/access-tiers.md) | [`ExecuteSqlResult`](../../reference/types.md#executesqlresult) | `ExecuteSqlCommand` |
<!-- relay:end header -->

Runs SQL that can change the database, in two steps: a preview that checks and describes the statements, then the real run.

<!-- relay:begin parameters -->
## Parameters

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `query` | string | Yes | The SQL to validate or run. May hold several statements. A trailing semicolon is ignored. |
| `confirm` | boolean | No | False (the default) only previews the statements; true runs them. |
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns a [ExecuteSqlResult](../../reference/types.md#executesqlresult) in `data`:

| Field | Type | Description |
|---|---|---|
| `confirmed` | boolean | True when the query was actually executed; false when this is a preview. |
| `statementSummaries` | string[] | One entry per statement describing the verb and tables involved (e.g. "UPDATE [CMS_User]"). Populated in both preview and execute modes. |
| `columns` | string[] | Column names, when execution returned a result set. |
| `rows` | string[][] | Row data, when execution returned a result set. |
<!-- relay:end result -->

## Behavior

- **Two steps.** Without `confirm: true` the SQL is only parsed and described: `statementSummaries` lists each statement as its verb and the tables it touches, for example `UPDATE [CMS_Tag]`, and nothing runs. Check the summary, then send the same query with `confirm: true`.
- **What is allowed.** `SELECT`, `DECLARE`, `INSERT`, `UPDATE`, `DELETE`, `TRUNCATE`, `EXECUTE` and the `CREATE`, `ALTER` and `DROP` statements. The same names are blocked as in [`query-sql`](query-sql.md) (`xp_cmdshell` and the others), and so is any mention of the table that holds Relay's API keys. See [Guardrails](../../concepts/guardrails.md).
- **One batch.** Several statements run as a single batch. Relay doesn't add a transaction, so write `BEGIN TRANSACTION` and `COMMIT` yourself when the statements must succeed or fail together.
- **The result.** When the SQL returns rows (a `SELECT`, or an `UPDATE` with `OUTPUT`), the first result set comes back in `columns` and `rows`, as text. Otherwise the message is "Command executed successfully."
- **Failures.** SQL Server's error comes back as "Query failed: ...". The query is cut off after [`SqlQueryTimeoutSeconds`](../../configuration.md#sqlquerytimeoutseconds) seconds (30 by default).
- **Irreversible.** Relay can't undo what the SQL did. Preview first, and take a backup before changing the schema.

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Preview a statement (nothing is run)

Without `confirm: true` the statements are only checked and described.

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "execute-sql",
      "parameters": {
        "query": "UPDATE CMS_Tag SET TagDescription = N'\''Warm-weather drinks'\'' WHERE TagName = N'\''happyhour'\''"
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new ExecuteSqlCommand
    {
        Query = "UPDATE CMS_Tag SET TagDescription = N'Warm-weather drinks' WHERE TagName = N'happyhour'",
    });
    var data = result.GetData<ExecuteSqlResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Preview only — set Confirm: true to execute.",
  "error": null,
  "data": {
    "confirmed": false,
    "statementSummaries": [
      "UPDATE [CMS_Tag]"
    ],
    "columns": null,
    "rows": null
  }
}
```

### Run it

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "execute-sql",
      "parameters": {
        "query": "UPDATE CMS_Tag SET TagDescription = N'\''Warm-weather drinks'\'' WHERE TagName = N'\''HappyHour'\''",
        "confirm": true
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new ExecuteSqlCommand
    {
        Query = "UPDATE CMS_Tag SET TagDescription = N'Warm-weather drinks' WHERE TagName = N'HappyHour'",
        Confirm = true,
    });
    var data = result.GetData<ExecuteSqlResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Command executed successfully.",
  "error": null,
  "data": {
    "confirmed": true,
    "statementSummaries": [
      "UPDATE [CMS_Tag]"
    ],
    "columns": null,
    "rows": null
  }
}
```

### A statement that returns rows

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "execute-sql",
      "parameters": {
        "query": "UPDATE CMS_Tag SET TagDescription = N'\'''\'' OUTPUT inserted.TagID, inserted.TagName WHERE TagName = N'\''HappyHour'\''",
        "confirm": true
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new ExecuteSqlCommand
    {
        Query = "UPDATE CMS_Tag SET TagDescription = N'' OUTPUT inserted.TagID, inserted.TagName WHERE TagName = N'HappyHour'",
        Confirm = true,
    });
    var data = result.GetData<ExecuteSqlResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Query returned 1 row(s).",
  "error": null,
  "data": {
    "confirmed": true,
    "statementSummaries": [
      "UPDATE [CMS_Tag]"
    ],
    "columns": [
      "TagID",
      "TagName"
    ],
    "rows": [
      [
        "11",
        "HappyHour"
      ]
    ]
  }
}
```

### Several statements

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "execute-sql",
      "parameters": {
        "query": "INSERT INTO CMS_Tag (TagName) VALUES (N'\''x'\''); DELETE FROM CMS_Tag WHERE TagName = N'\''x'\''"
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new ExecuteSqlCommand
    {
        Query = "INSERT INTO CMS_Tag (TagName) VALUES (N'x'); DELETE FROM CMS_Tag WHERE TagName = N'x'",
    });
    var data = result.GetData<ExecuteSqlResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Preview only — set Confirm: true to execute.",
  "error": null,
  "data": {
    "confirmed": false,
    "statementSummaries": [
      "INSERT [CMS_Tag]",
      "DELETE [CMS_Tag]"
    ],
    "columns": null,
    "rows": null
  }
}
```
<!-- relay:end examples -->

<!-- relay:begin errors -->
<!-- relay:end errors -->

## See also

- [`query-sql`](query-sql.md)
- [Guardrails](../../concepts/guardrails.md)
