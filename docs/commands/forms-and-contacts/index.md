---
toc_depth: 2
---

# Forms and contacts

Two commands for online marketing data: reading the submissions of a form, and deleting contacts in bulk.

<!-- relay:begin commands -->
| Command | Access | What it does |
|---|---|---|
| [`get-form-submissions`](#get-form-submissions) | Read | Reads the submissions of a Kentico form (BizForm). |
| [`delete-contacts`](#delete-contacts) | Admin | Bulk-deletes the contacts that match a SQL condition, through Kentico's contact deletion service. |
<!-- relay:end commands -->

---

## get-form-submissions

<!-- relay:begin header:get-form-submissions -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `get-form-submissions` | [Read](../../concepts/access-tiers.md) | [`GetFormSubmissionsResult`](../../reference/types.md#getformsubmissionsresult) | `GetFormSubmissionsCommand` |
<!-- relay:end header:get-form-submissions -->

Reads the submissions of a Kentico form, newest first.

<!-- relay:begin parameters:get-form-submissions -->
### Parameters { #get-form-submissions-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `formClassName` | string | Yes | Class name of the form, as shown in the admin under the form's properties, e.g. `BizForm.DancingGoatContactUs`. |
| `top` | integer | No | Maximum number of submissions to return. All of them when omitted. Can't be negative. |
| `whereClause` | string | No | A condition over the form's table columns, without the `WHERE` keyword: comparisons of columns with values (`=`, `LIKE`, `IN`, `BETWEEN`, `IS NULL` and the like) combined with `AND`, `OR`, `NOT` and parentheses, at most 2000 characters. Function calls, subqueries and comments are refused. |
| `orderBy` | string | No | The form's table columns to sort by, separated by commas, each optionally followed by `ASC` or `DESC`, without the `ORDER BY` keywords. Defaults to `FormInserted DESC`. |
<!-- relay:end parameters:get-form-submissions -->

<!-- relay:begin result:get-form-submissions -->
### Result { #get-form-submissions-result }

A successful run returns a [GetFormSubmissionsResult](../../reference/types.md#getformsubmissionsresult) in `data`:

| Field | Type | Description |
|---|---|---|
| `formClassName` | string | The class name of the form, as it was given. |
| `columns` | string[] | Column names, taken from the first submission, so they are empty when there are no submissions. |
| `rows` | string[][] | One list of values per submission, in `columns` order. Every value is text; null for SQL NULL. |
<!-- relay:end result:get-form-submissions -->

### Behavior { #get-form-submissions-behavior }

- **The form.** `formClassName` is the form's class name, shown in the admin under the form's properties; the examples use `BizForm.DancingGoatContactUs`. A name that isn't a form fails with "Form '...' was not found."
- **Columns and rows.** `columns` are the columns of the form's table (its fields, the submission's ID, `FormInserted` and `FormUpdated`), taken from the first row, so they are empty when there are no submissions. Every value is text, formatted the way the server formats it (a date reads like `10/19/2026 2:05:00 PM`), and `null` for a SQL NULL.
- **Order and size.** Submissions come newest first (`FormInserted DESC`) unless `orderBy` says otherwise. `top` limits how many come back; without it all submissions do.
- **`whereClause` and `orderBy`.** Both are written without the `WHERE` and `ORDER BY` keywords, over the columns of the form's table. A condition compares columns with values (`=`, `<>`, `!=`, `<`, `>`, `<=`, `>=`, `LIKE`, `IN`, `BETWEEN`, `IS NULL`) and combines the comparisons with `AND`, `OR`, `NOT` and parentheses, for example `UserEmail LIKE 'grace@%'`. An ordering lists columns, each followed by `ASC` or `DESC` if you like. Function calls, subqueries and comments are refused, so a date filter takes the date written out (`FormInserted > '2026-10-01'`); [`query-sql`](../sql/index.md#query-sql) does what these can't. See [Checks on queries](../../concepts/guardrails.md#checks-on-queries).

<!-- relay:begin examples:get-form-submissions -->
### Examples { #get-form-submissions-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Read the newest submissions of a form { #get-form-submissions-example-contact-us }

Without `orderBy`, the newest submission comes first.

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "get-form-submissions",
      "parameters": {
        "formClassName": "BizForm.DancingGoatContactUs",
        "top": 10
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new GetFormSubmissionsCommand
    {
        FormClassName = "BizForm.DancingGoatContactUs",
        Top = 10,
    });
    var data = result.GetData<GetFormSubmissionsResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Retrieved 2 submission(s) for 'BizForm.DancingGoatContactUs'.",
  "error": null,
  "data": {
    "formClassName": "BizForm.DancingGoatContactUs",
    "columns": [
      "Form_2023_09_12_17_45ID",
      "FormInserted",
      "FormUpdated",
      "UserFirstName",
      "UserLastName",
      "UserEmail",
      "UserMessage"
    ],
    "rows": [
      [
        "2",
        "10/19/2026 2:05:00 PM",
        "10/19/2026 2:05:00 PM",
        "Grace",
        "Hopper",
        "grace@example.com",
        "Can I book the cupping room for a team event?"
      ],
      [
        "1",
        "10/18/2026 9:30:00 AM",
        "10/18/2026 9:30:00 AM",
        "Ada",
        "Lovelace",
        "ada@example.com",
        "Do you ship cold brew kits?"
      ]
    ]
  }
}
```

#### Filter and sort the submissions { #get-form-submissions-example-filtered }

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "get-form-submissions",
      "parameters": {
        "formClassName": "BizForm.DancingGoatContactUs",
        "whereClause": "UserEmail LIKE '\''grace@%'\''",
        "orderBy": "FormInserted ASC"
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new GetFormSubmissionsCommand
    {
        FormClassName = "BizForm.DancingGoatContactUs",
        WhereClause = "UserEmail LIKE 'grace@%'",
        OrderBy = "FormInserted ASC",
    });
    var data = result.GetData<GetFormSubmissionsResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Retrieved 1 submission(s) for 'BizForm.DancingGoatContactUs'.",
  "error": null,
  "data": {
    "formClassName": "BizForm.DancingGoatContactUs",
    "columns": [
      "Form_2023_09_12_17_45ID",
      "FormInserted",
      "FormUpdated",
      "UserFirstName",
      "UserLastName",
      "UserEmail",
      "UserMessage"
    ],
    "rows": [
      [
        "2",
        "10/19/2026 2:05:00 PM",
        "10/19/2026 2:05:00 PM",
        "Grace",
        "Hopper",
        "grace@example.com",
        "Can I book the cupping room for a team event?"
      ]
    ]
  }
}
```
<!-- relay:end examples:get-form-submissions -->

<!-- relay:begin errors:get-form-submissions -->
### Errors { #get-form-submissions-errors }

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "A form that does not exist"

    Parameters:

    ```json
    {
      "formClassName": "BizForm.NoSuchForm"
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Form 'BizForm.NoSuchForm' was not found. Use the form's class name, shown in the admin under the form's properties (for example BizForm.ContactUs).",
      "data": null
    }
    ```

???+ failure "A condition with a function call"

    Parameters:

    ```json
    {
      "formClassName": "BizForm.DancingGoatContactUs",
      "whereClause": "FormInserted > DATEADD(day, -7, GETDATE())"
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "WhereClause can't contain 'DATEADD(day, -7, GETDATE())'. A condition can compare columns with values (=, <>, !=, <, >, <=, >=, LIKE, IN, BETWEEN, IS NULL) and combine them with AND, OR, NOT and parentheses.",
      "data": null
    }
    ```
<!-- relay:end errors:get-form-submissions -->

### See also { #get-form-submissions-see-also }

- [`delete-contacts`](#delete-contacts)
- [`query-sql`](../sql/index.md#query-sql) for anything else in the database

---

## delete-contacts

<!-- relay:begin header:delete-contacts -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `delete-contacts` | [Admin](../../concepts/access-tiers.md) | nothing; the `message` says what happened | `DeleteContactsCommand` |
<!-- relay:end header:delete-contacts -->

Deletes the contacts that match a SQL condition, through Kentico's contact deletion service.

<!-- relay:begin parameters:delete-contacts -->
### Parameters { #delete-contacts-parameters }

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `whereClause` | string | Yes | A SQL WHERE expression, without the `WHERE` keyword, over the contact table's columns, e.g. `ContactEmail IS NULL`. |
| `batchSize` | integer | No | When omitted (or 0), Kentico deletes every matching contact, in batches of 1000. When given, only a single batch of this size is deleted per call. |
<!-- relay:end parameters:delete-contacts -->

<!-- relay:begin result:delete-contacts -->
### Result { #delete-contacts-result }

A successful run returns no data (`data` is `null`); the `message` says what was done.
<!-- relay:end result:delete-contacts -->

### Behavior { #delete-contacts-behavior }

- **Admin only, and irreversible.** Kentico's contact deletion service also deletes the data that belongs to the contacts, such as their activities and consent agreements. There is no recycle bin.
- **The condition.** `whereClause` is a SQL `WHERE` expression, without the `WHERE` keyword, over the contact table's columns, for example `ContactEmail IS NULL` or `ContactCreated < '2024-01-01' AND ContactEmail IS NULL`. It is required: Relay won't delete every contact when it is empty.
- **Batches.** Leave `batchSize` out, or use 0, and Kentico deletes every matching contact, in batches of 1000. Give a `batchSize` and a single batch of that many contacts is deleted per call; repeat the call for more.
- **No count.** The answer doesn't say how many contacts were deleted. Count them with [`query-sql`](../sql/index.md#query-sql) before and after: `SELECT COUNT(*) FROM OM_Contact WHERE ContactEmail IS NULL`.

<!-- relay:begin examples:delete-contacts -->
### Examples { #delete-contacts-examples }

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

#### Delete one batch only { #delete-contacts-example-one-batch }

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

#### Delete every contact that has no email address { #delete-contacts-example-no-email }

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
<!-- relay:end examples:delete-contacts -->

<!-- relay:begin errors:delete-contacts -->
### Errors { #delete-contacts-errors }

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
<!-- relay:end errors:delete-contacts -->

### See also { #delete-contacts-see-also }

- [`query-sql`](../sql/index.md#query-sql) to preview which contacts match
- [`get-form-submissions`](#get-form-submissions)
