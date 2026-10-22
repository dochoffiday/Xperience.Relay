---
toc_depth: 2
---

# SQL

Raw SQL against the Xperience database, for what the typed commands can't do. Both commands need an Admin key and are guarded: see [Guardrails](../../concepts/guardrails.md). Prefer a typed command when there is one.

<!-- relay:begin commands -->
| Command | Access | What it does |
|---|---|---|
| [`query-sql`](#query-sql) | Admin | Executes a read-only SQL query against the Xperience database. |
| [`execute-sql`](#execute-sql) | Admin | Runs SQL against the Xperience database in two steps. |
<!-- relay:end commands -->

---

## query-sql

<!-- relay:begin header:query-sql -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `query-sql` | [Admin](../../concepts/access-tiers.md) | [`QuerySqlResult`](../../reference/types.md#querysqlresult) | `QuerySqlCommand` |
<!-- relay:end header:query-sql -->

Runs a read-only SQL query against the Xperience database and returns the first result set.

<!-- relay:begin parameters:query-sql -->
### Parameters { #query-sql-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `query` | string | Yes | The SQL query to execute. May hold several statements. A trailing semicolon is ignored. |
<!-- relay:end parameters:query-sql -->

<!-- relay:begin result:query-sql -->
### Result { #query-sql-result }

A successful run returns a [QuerySqlResult](../../reference/types.md#querysqlresult) in `data`:

| Field | Type | Description |
|---|---|---|
| `columns` | string[] | Column names of the result set. |
| `rows` | string[][] | One list of values per row, in `columns` order. Every value is text; null for SQL NULL. |
<!-- relay:end result:query-sql -->

### Behavior { #query-sql-behavior }

- **What is allowed.** `SELECT` (including `WITH ... SELECT`) and `DECLARE`. The text is parsed with Microsoft's T-SQL parser rather than matched against keywords, so comments and string literals can't smuggle another statement in. Anything else is refused: "'DELETE' statements are not allowed in this context." `SELECT ... INTO` is refused too, because it creates a table; a temporary `#table` is fine.
- **Blocked names.** `xp_cmdshell`, `OPENROWSET`, `OPENDATASOURCE`, `OPENQUERY`, `OPENXML` and `sp_configure` are refused wherever they appear, and so is any mention of the table that holds Relay's API keys, even inside a string. See [Guardrails](../../concepts/guardrails.md).
- **The result.** `columns` and `rows`, from the first result set only. Every value is text, and `null` for a SQL NULL. A query that returns no result set fails with "The query returned no result set. End it with a SELECT."
- **Syntax errors.** A query that doesn't parse fails with the parser's message ("SQL parsing error: ...").
- **Time limit.** The query is cut off after [`SqlQueryTimeoutSeconds`](../../configuration.md#sqlquerytimeoutseconds) seconds (30 by default).
- **A guardrail, not a security boundary.** The checks stop mistakes and the obvious abuses. They don't replace access control: give an Admin key only to callers you trust completely.

<!-- relay:begin examples:query-sql -->
### Examples { #query-sql-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Read from the database { #query-sql-example-select }

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
<!-- relay:end examples:query-sql -->

<!-- relay:begin errors:query-sql -->
### Errors { #query-sql-errors }

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
<!-- relay:end errors:query-sql -->

### See also { #query-sql-see-also }

- [`execute-sql`](#execute-sql) for statements that change data
- [`query-custom-objects`](../custom-objects/index.md#query-custom-objects) and [`query-web-page-items`](../web-pages/index.md#query-web-page-items), which don't need SQL
- [Guardrails](../../concepts/guardrails.md)

---

## execute-sql

<!-- relay:begin header:execute-sql -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `execute-sql` | [Admin](../../concepts/access-tiers.md) | [`ExecuteSqlResult`](../../reference/types.md#executesqlresult) | `ExecuteSqlCommand` |
<!-- relay:end header:execute-sql -->

Runs SQL that can change the database, in two steps: a preview that checks and describes the statements, then the real run.

<!-- relay:begin parameters:execute-sql -->
### Parameters { #execute-sql-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `query` | string | Yes | The SQL to validate or run. May hold several statements. A trailing semicolon is ignored. |
| `confirm` | boolean | No | False (the default) only previews the statements; true runs them. |
<!-- relay:end parameters:execute-sql -->

<!-- relay:begin result:execute-sql -->
### Result { #execute-sql-result }

A successful run returns a [ExecuteSqlResult](../../reference/types.md#executesqlresult) in `data`:

| Field | Type | Description |
|---|---|---|
| `confirmed` | boolean | True when the query was actually executed; false when this is a preview. |
| `statementSummaries` | string[] | One entry per statement describing the verb and tables involved (e.g. "UPDATE [CMS_User]"). Populated in both preview and execute modes. |
| `columns` | string[] | Column names, when execution returned a result set. |
| `rows` | string[][] | Row data, when execution returned a result set. |
<!-- relay:end result:execute-sql -->

### Behavior { #execute-sql-behavior }

- **Two steps.** Without `confirm: true` the SQL is only parsed and described: `statementSummaries` lists each statement as its verb and the tables it touches, for example `UPDATE [CMS_Tag]`, and nothing runs. Check the summary, then send the same query with `confirm: true`.
- **What is allowed.** `SELECT`, `DECLARE`, `INSERT`, `UPDATE`, `DELETE`, `TRUNCATE`, `EXECUTE` and the `CREATE`, `ALTER` and `DROP` statements. The same names are blocked as in [`query-sql`](#query-sql) (`xp_cmdshell` and the others), and so is any mention of the table that holds Relay's API keys. See [Guardrails](../../concepts/guardrails.md).
- **One batch.** Several statements run as a single batch. Relay doesn't add a transaction, so write `BEGIN TRANSACTION` and `COMMIT` yourself when the statements must succeed or fail together.
- **The result.** When the SQL returns rows (a `SELECT`, or an `UPDATE` with `OUTPUT`), the first result set comes back in `columns` and `rows`, as text. Otherwise the message is "Command executed successfully."
- **Failures.** SQL Server's error comes back as "Query failed: ...". The query is cut off after [`SqlQueryTimeoutSeconds`](../../configuration.md#sqlquerytimeoutseconds) seconds (30 by default).
- **Irreversible.** Relay can't undo what the SQL did. Preview first, and take a backup before changing the schema.

<!-- relay:begin examples:execute-sql -->
### Examples { #execute-sql-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Preview a statement (nothing is run) { #execute-sql-example-preview }

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

#### Run it { #execute-sql-example-confirmed }

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

#### A statement that returns rows { #execute-sql-example-select-output }

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

#### Several statements { #execute-sql-example-multiple }

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
<!-- relay:end examples:execute-sql -->

<!-- relay:begin errors:execute-sql -->
<!-- relay:end errors:execute-sql -->

### See also { #execute-sql-see-also }

- [`query-sql`](#query-sql)
- [Guardrails](../../concepts/guardrails.md)
